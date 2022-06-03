using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using weka.classifiers.trees;
using weka.classifiers.evaluation;
using weka.core;
using java.io;
using java.lang;
using java.util;
using weka.classifiers.functions;
using weka.classifiers;
using weka.core.converters; 

public class Aprendizaje : MonoBehaviour
{
    weka.classifiers.trees.M5P saberPredecirDistancia, saberPredecirFuerzaX;
    weka.core.Instances casosEntrenamiento;
    private string ESTADO = "Sin conocimiento";
    public GameObject pelota;
    GameObject InstanciaPelota, PuntoObjetivo;
    public float valorMaximoFx, valorMaximoFy, pasoX, pasoY, Velocidad_Simulacion=1;
    float mejorFuerzaX, mejorFuerzaY, distanciaObjetivo;
    Rigidbody r;

    public Transform posTarget;
 
    void Start()
    {
        Time.timeScale = Velocidad_Simulacion;                                          
        if (ESTADO == "Sin conocimiento") StartCoroutine("Entrenamiento");              

    }

    IEnumerator Entrenamiento()
    {

        casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/WekaAll/Iniciales_Experiencias.arff"));  
        
        //Uso de una tabla con los datos del último entrenamiento:
        //casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Finales_Experiencias.arff"));   

        if (casosEntrenamiento.numInstances() < 10)
        {
            print("Datos de entrada: valorMaximoFx=" + valorMaximoFx + " valorMaximoFy="+ valorMaximoFy + "  " + ((valorMaximoFx == 0 || valorMaximoFy == 0) ? " ERROR: alguna fuerza es siempre 0" : ""));
            for (float Fx = 1; Fx <= valorMaximoFx; Fx = Fx +  pasoX)                      //Bucle de planificación de la fuerza FX durante el entrenamiento
            {
                for (float Fy = 1; Fy <= valorMaximoFy; Fy = Fy + pasoY)                    //Bucle de planificación del giro FY durante el entrenamiento
                {
                    InstanciaPelota = Instantiate(pelota) as GameObject;
                    Rigidbody rb = InstanciaPelota.GetComponent<Rigidbody>();              
                    rb.AddForce(transform.right * Fx, ForceMode.Impulse);                
                    rb.AddTorque(transform.rotation.eulerAngles + new Vector3(0,Fy,0));
                    yield return new WaitForSeconds(10);       

                    //Me interesa que quede lo más cercano al objetivo, resta de normas
                    Instance casoAaprender = new Instance(casosEntrenamiento.numAttributes());
                    print("ENTRENAMIENTO: con fuerza Fx " + Fx + " y Fy=" + Fy + " se alcanzó una distancia de " + (rb.transform.position - posTarget.position).sqrMagnitude + " m");
                    casoAaprender.setDataset(casosEntrenamiento);                          
                    casoAaprender.setValue(0, Fx);                                         
                    casoAaprender.setValue(1, Fy);
                    casoAaprender.setValue(2, (rb.transform.position - posTarget.position).sqrMagnitude);                    
                    casosEntrenamiento.add(casoAaprender);                                
                    Destroy(InstanciaPelota,0);                                             
                }                                                                          
            }


            File salida = new File("Assets/Finales_Experiencias.arff");
            if (!salida.exists())
                System.IO.File.Create(salida.getAbsoluteFile().toString()).Dispose();
            ArffSaver saver = new ArffSaver();
            saver.setInstances(casosEntrenamiento);
            saver.setFile(salida);
            saver.writeBatch();
        }

        //APRENDIZAJE CONOCIMIENTO:  
        saberPredecirFuerzaX = new M5P();                                                
        casosEntrenamiento.setClassIndex(0);                                             
        saberPredecirFuerzaX.buildClassifier(casosEntrenamiento);                        

        saberPredecirDistancia = new M5P();                                              
        casosEntrenamiento.setClassIndex(2);                                                                                                                                   
        saberPredecirDistancia.buildClassifier(casosEntrenamiento);                      

        ESTADO = "Con conocimiento";

        print(casosEntrenamiento.numInstances() +" espers "+ saberPredecirDistancia.toString());

        //EVALUACION DEL CONOCIMIENTO APRENDIDO: 
        if (casosEntrenamiento.numInstances() >= 10){
            casosEntrenamiento.setClassIndex(0);
            Evaluation evaluador = new Evaluation(casosEntrenamiento);                   
            evaluador.crossValidateModel(saberPredecirFuerzaX, casosEntrenamiento, 10, new java.util.Random(1));
            print("El Error Absoluto Promedio con Fx durante el entrenamiento fue de " + evaluador.meanAbsoluteError().ToString("0.000000") + " N");
            casosEntrenamiento.setClassIndex(2);
            evaluador.crossValidateModel(saberPredecirDistancia, casosEntrenamiento, 10, new java.util.Random(1));
            print("El Error Absoluto Promedio con Distancias durante el entrenamiento fue de " + evaluador.meanAbsoluteError().ToString("0.000000") + " m");
        }

            
       
        distanciaObjetivo = (transform.position - posTarget.position).sqrMagnitude;

       
    }

    //FALTA POR ENTENDER
    void FixedUpdate()                                                                                 
    {
        if ((ESTADO == "Con conocimiento") && (distanciaObjetivo > 0))
        {
            Time.timeScale = 1;                                                                                
            float menorDistancia = 1e9f;
            print("-- OBJETIVO: GIRAR HACIA EL OBJETIVO SITUADO A UNA DISTANCIA DE " + distanciaObjetivo + " m.");
       
            //Si usa dos bucles Fx y Fy con "modelo fisico aproximado", complejidad n^2
            //Reduce la complejidad con un solo bucle FOR, así

            for (float Fy = 1; Fy < valorMaximoFy; Fy = Fy + pasoY)                                            //Bucle FOR con fuerza Fy, deduce Fx = f (Fy, distancia) y escoge mejor combinacion         
            {
                Instance casoPrueba = new Instance(casosEntrenamiento.numAttributes());
                casoPrueba.setDataset(casosEntrenamiento);
                casoPrueba.setValue(1, Fy);                                                                   //crea un registro con una Fy
                casoPrueba.setValue(2, distanciaObjetivo);                                                    //y la distancia
                float Fx = (float)saberPredecirFuerzaX.classifyInstance(casoPrueba);                          //Predice Fx a partir de la distancia y una Fy 
                if ((Fx >= 1) && (Fx <= valorMaximoFx))
                {
                    Instance casoPrueba2 = new Instance(casosEntrenamiento.numAttributes());
                    casoPrueba2.setDataset(casosEntrenamiento);                                                  //Utiliza el "modelo fisico aproximado" con Fx y Fy                 
                    casoPrueba2.setValue(0, Fx);                                                                 //Crea una registro con una Fx
                    casoPrueba2.setValue(1, Fy);                                                                 //Crea una registro con una Fy
                    float prediccionDistancia = (float)saberPredecirDistancia.classifyInstance(casoPrueba2);     //Predice la distancia dada Fx y Fy
                    if (Mathf.Abs(prediccionDistancia - distanciaObjetivo) < menorDistancia)                     //Busca la Fy con una distancia más cercana al objetivo
                    {
                        menorDistancia = Mathf.Abs(prediccionDistancia - distanciaObjetivo);                     //si encuentra una buena toma nota de esta distancia
                        mejorFuerzaX = Fx;                                                                       //de la fuerzas que uso, Fx
                        mejorFuerzaY = Fy;                                                                       //tambien Fy
                        print("RAZONAMIENTO: Una posible acción es ejercer una fuerza Fx=" + mejorFuerzaX + " y Fy= " + mejorFuerzaY + " se alcanzaría una distancia de " + prediccionDistancia);
                    }
                }
            }                                                                                                     //FIN DEL RAZONAMIENTO PREVIO
            if ((mejorFuerzaX == 0) && (mejorFuerzaY == 0)) { 
                print("NO SE LANZO");
            }
            else
            {
                InstanciaPelota = Instantiate(pelota) as GameObject;
                r = InstanciaPelota.GetComponent<Rigidbody>();                                                        //EN EL JUEGO: utiliza la pelota física del juego (si no existe la crea)
                r.AddForce(new Vector3(mejorFuerzaX, 0, 0), ForceMode.Impulse);
                r.AddTorque(new Vector3(0,0,mejorFuerzaY));                            //la lanza en el videojuego con la fuerza encontrada
                print("DECISION REALIZADA: Se lanzó pelota con fuerza Fx =" + mejorFuerzaX + " y Fy= " + mejorFuerzaY);
                ESTADO = "Acción realizada";
            }
         }
        if (ESTADO == "Acción realizada")
        {
            if (r.transform.position.y < 0)                                            //cuando la pelota cae por debajo de 0 m
            {                                                                          //escribe la distancia en x alcanzada
                print("La canasta está a una distancia de " + distanciaObjetivo + " m");
                print("La pelota lanzada llegó a " + r.transform.position.x + ". El error fue de " + (r.transform.position.x - distanciaObjetivo).ToString("0.000000") + " m");
                r.isKinematic = true;
                ESTADO = "FIN";
            }
        }
    }
}
