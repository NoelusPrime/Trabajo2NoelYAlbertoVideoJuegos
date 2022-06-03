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
    weka.classifiers.trees.M5P saberPredecirDistancia, saberPredecirFuerzaX, saberPredecirAceleracion, saberPredecirFrenado, 
                                saberPredecirGiro, saberPredecirAngulo, saberPredecirGastoGoma;
    weka.core.Instances casosEntrenamientoDistancia, casosEntrenamientoAngulo, casosEntrenamientoGastoGoma;
    private string ESTADO = "Sin conocimiento";
    public GameObject pelota;
    public pruebaScript ps;
    GameObject InstanciaPelota, PuntoObjetivo;
    public float valorMaximoA, valorMaximoF, pasoA, pasoF, Velocidad_Simulacion=1, valorMaximoV, pasoV, valorMaximoG, pasoG;
    float mejorFuerzaX, mejorFuerzaY, distanciaObjetivo;
    Rigidbody r;

    public Transform posTarget, posIni;
 
    public int tiempoHaciendoPrueba;
    void Start()
    {
        Time.timeScale = Velocidad_Simulacion;                                          
        if (ESTADO == "Sin conocimiento") StartCoroutine("Entrenamiento");              

    }

    IEnumerator enumGiro(int g){
        for(float i = tiempoHaciendoPrueba; i > 0; i -= Time.deltaTime){
            ps.girarDerecha(g);
        }
        yield return null;
    }

    IEnumerator enumAcel(int a){
        for(float i = tiempoHaciendoPrueba; i > 0; i -= Time.deltaTime){
            ps.pisarAcelerador(a); //Tiene que ser mucho mayor que el freno
        }
        yield return null;
    }

    IEnumerator enumFre(int f){
        for(float i = tiempoHaciendoPrueba/2; i > 0; i -= Time.deltaTime){
            ps.pisarFreno(f);
        }
        yield return null;
    }

    IEnumerator Entrenamiento()
    {

        casosEntrenamientoDistancia = new weka.core.Instances(new java.io.FileReader("Assets/WekaAll/Iniciales_ExperienciasDistancia.arff"));  
        casosEntrenamientoAngulo = new weka.core.Instances(new java.io.FileReader("Assets/WekaAll/Iniciales_ExperienciasAngulo.arff"));
        casosEntrenamientoGastoGoma = new weka.core.Instances(new java.io.FileReader("Assets/WekaAll/Iniciales_ExperienciasGastoGoma.arff"));

        //Uso de una tabla con los datos del último entrenamiento:
        //casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Finales_Experiencias.arff"));   

        if (casosEntrenamientoDistancia.numInstances() < 10 || casosEntrenamientoAngulo.numInstances() < 10 || casosEntrenamientoGastoGoma.numInstances() < 10)
        {
            print("Datos de entrada: valorMaximoFx=" + valorMaximoA + " valorMaximoFy="+ valorMaximoF + "  " + ((valorMaximoA == 0 || valorMaximoF == 0) ? " ERROR: alguna fuerza es siempre 0" : ""));
            for(float f = 1; f <= valorMaximoF; f = f + pasoF){                 //Bucle de planificación de la velocidad a la que va
                for(float g = 1; g <= valorMaximoG; g = g + pasoG){                 //Bucle de planificacion de los grados de giro
                    for (float a = 1; a <= valorMaximoA; a = a +  pasoA){               //Bucle de planificacion de la aceleracion 
                        

                        //aaaaaaaaaaaaaaaaaaaaaaaa
                        InstanciaPelota = Instantiate(pelota) as GameObject;
                        Rigidbody rb = InstanciaPelota.GetComponent<Rigidbody>(); 
                        ps = InstanciaPelota.GetComponent<pruebaScript>();

                        StartCoroutine("enumAcel", (int)a); //Acelera unos segundos
                        yield return new WaitForSeconds(2);

                        //Gira, acelera y frena a la vez, el freno solo lo hace la mitad inicial de la curva (tiempo/2)
                        StartCoroutine("enumGiro", (int)g);
                        StartCoroutine("enumAcel",(int)a);
                        StartCoroutine("enumFre", (int)f);


                            
                        yield return new WaitForSeconds(tiempoHaciendoPrueba);

                        Instance casoAaprenderDistancia = new Instance(casosEntrenamientoDistancia.numAttributes());
                        //Diferencia de distancia? Diferencia de angulos?
                        print("ENTRENAMIENTO: con velocidad " + rb.velocity.sqrMagnitude + " con giro " + g + " con aceleracion " + a + " y frenado " + f + 
                        " Distancia recorrida y angulo final " + (InstanciaPelota.transform.position - posIni.position).sqrMagnitude + ", " + Vector3.Angle(posIni.forward, InstanciaPelota.transform.position));
                        casoAaprenderDistancia.setDataset(casosEntrenamientoDistancia);                          
                        casoAaprenderDistancia.setValue(0, rb.velocity.sqrMagnitude);                                         
                        casoAaprenderDistancia.setValue(1, g);
                        casoAaprenderDistancia.setValue(2, a);
                        casoAaprenderDistancia.setValue(3, f);
                        //Diferencia de distancia? Diferencia de angulos? gasto de goma en linea recta?
                        casoAaprenderDistancia.setValue(4, (rb.transform.position - posTarget.position).sqrMagnitude);                    
                        casosEntrenamientoDistancia.add(casoAaprenderDistancia);     

                        //Documento de angulos
                        Instance casoAaprenderAngulo = new Instance(casosEntrenamientoAngulo.numAttributes());
                        casoAaprenderAngulo.setDataset(casosEntrenamientoAngulo);                          
                        casoAaprenderAngulo.setValue(0, rb.velocity.sqrMagnitude);                                         
                        casoAaprenderAngulo.setValue(1, g);
                        casoAaprenderAngulo.setValue(2, a);
                        casoAaprenderAngulo.setValue(3, f);
                        casoAaprenderAngulo.setValue(4, Vector3.Angle(posIni.forward, (InstanciaPelota.transform.position-posIni.position)));          //Angulos de dos vectores          
                        casosEntrenamientoAngulo.add(casoAaprenderAngulo);

                        //Documento gasto de goma
                        Instance casoAaprenderGastoGoma = new Instance(casosEntrenamientoGastoGoma.numAttributes());
                        //Diferencia de distancia? Diferencia de angulos?
                        print("ENTRENAMIENTO: con velocidad " + rb.velocity.sqrMagnitude + " con giro " + g + " con aceleracion " + a + " y frenado " + f + 
                        " Distancia recorrida y angulo final " + (InstanciaPelota.transform.position - posIni.position).sqrMagnitude + ", " + Vector3.Angle(posIni.forward, InstanciaPelota.transform.position));
                        casoAaprenderGastoGoma.setDataset(casosEntrenamientoGastoGoma);                          
                        casoAaprenderGastoGoma.setValue(0, rb.velocity.sqrMagnitude);                                         
                        casoAaprenderGastoGoma.setValue(1, g);
                        casoAaprenderGastoGoma.setValue(2, a);
                        casoAaprenderGastoGoma.setValue(3, f);
                        //Diferencia de distancia? Diferencia de angulos? gasto de goma en linea recta?
                        casoAaprenderGastoGoma.setValue(4, a-f);                    
                        casosEntrenamientoGastoGoma.add(casoAaprenderGastoGoma);




                        Destroy(InstanciaPelota,0);                                             
                                                                                               
                    }
                }
            }

            File salidaDistancia = new File("Assets/Finales_ExperienciasDistancia.arff");
            if (!salidaDistancia.exists())
                System.IO.File.Create(salidaDistancia.getAbsoluteFile().toString()).Dispose();
            ArffSaver saverDistancia = new ArffSaver();
            saverDistancia.setInstances(casosEntrenamientoDistancia);
            saverDistancia.setFile(salidaDistancia);
            saverDistancia.writeBatch();

            File salidaAngulos = new File("Assets/Finales_ExperienciasAngulos.arff");
            if (!salidaAngulos.exists())
                System.IO.File.Create(salidaAngulos.getAbsoluteFile().toString()).Dispose();
            ArffSaver saverAngulos = new ArffSaver();
            saverAngulos.setInstances(casosEntrenamientoAngulo);
            saverAngulos.setFile(salidaAngulos);
            saverAngulos.writeBatch();

            File salidaGastoGoma = new File("Assets/Finales_ExperienciasGastoGoma.arff");
            if (!salidaGastoGoma.exists())
                System.IO.File.Create(salidaGastoGoma.getAbsoluteFile().toString()).Dispose();
            ArffSaver saverGastoGoma = new ArffSaver();
            saverGastoGoma.setInstances(casosEntrenamientoGastoGoma);
            saverGastoGoma.setFile(salidaGastoGoma);
            saverGastoGoma.writeBatch();
        }

        //APRENDIZAJE CONOCIMIENTO: 

        saberPredecirGiro = new M5P();                                                
        casosEntrenamientoDistancia.setClassIndex(1);                                             
        saberPredecirGiro.buildClassifier(casosEntrenamientoDistancia); 

        saberPredecirAceleracion = new M5P();                                                
        casosEntrenamientoDistancia.setClassIndex(2);                                             
        saberPredecirAceleracion.buildClassifier(casosEntrenamientoDistancia);  

        saberPredecirFrenado = new M5P();                                                
        casosEntrenamientoDistancia.setClassIndex(3);                                             
        saberPredecirFrenado.buildClassifier(casosEntrenamientoDistancia);  

        saberPredecirDistancia = new M5P();                                                
        casosEntrenamientoDistancia.setClassIndex(4);                                             
        saberPredecirDistancia.buildClassifier(casosEntrenamientoDistancia);  

        saberPredecirAngulo = new M5P();                                                
        casosEntrenamientoAngulo.setClassIndex(4);                                             
        saberPredecirAngulo.buildClassifier(casosEntrenamientoAngulo); 


        saberPredecirGastoGoma = new M5P();                                                
        casosEntrenamientoGastoGoma.setClassIndex(4);                                             
        saberPredecirGastoGoma.buildClassifier(casosEntrenamientoGastoGoma);


        



        //Borrar luego, todavia no porque pondra todo lo de abajo en rojo y paso de rayarme con eso
        saberPredecirFuerzaX = new M5P();                                                
        casosEntrenamientoDistancia.setClassIndex(0);                                             
        saberPredecirFuerzaX.buildClassifier(casosEntrenamientoDistancia);                        

        saberPredecirDistancia = new M5P();                                              
        casosEntrenamientoDistancia.setClassIndex(2);                                                                                                                                   
        saberPredecirDistancia.buildClassifier(casosEntrenamientoDistancia);

        //Borrar hasta aqui                     

        ESTADO = "Con conocimiento";

        print(casosEntrenamientoDistancia.numInstances() +" espers "+ saberPredecirDistancia.toString());
        print(casosEntrenamientoAngulo.numInstances() +" espers "+ saberPredecirAngulo.toString());
        print(casosEntrenamientoGastoGoma.numInstances() +" espers "+ saberPredecirGastoGoma.toString());

        //EVALUACION DEL CONOCIMIENTO APRENDIDO: 
        //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA partir de aqui para abajo, corregir todo
        print("Etapa de crossevaluation ");
        if (casosEntrenamientoDistancia.numInstances() >= 10 && casosEntrenamientoAngulo.numInstances() >= 10 && casosEntrenamientoGastoGoma.numInstances() >= 10){
            casosEntrenamientoDistancia.setClassIndex(0);
            Evaluation evaluadorDistancia = new Evaluation(casosEntrenamientoDistancia);                   
            evaluadorDistancia.crossValidateModel(saberPredecirFuerzaX, casosEntrenamientoDistancia, 10, new java.util.Random(1));
            print("El Error Absoluto Promedio con Fx durante el entrenamiento fue de " + evaluadorDistancia.meanAbsoluteError().ToString("0.000000") + " N");
            casosEntrenamientoDistancia.setClassIndex(2);
            evaluadorDistancia.crossValidateModel(saberPredecirDistancia, casosEntrenamientoDistancia, 10, new java.util.Random(1));
            print("El Error Absoluto Promedio con Distancias durante el entrenamiento fue de " + evaluadorDistancia.meanAbsoluteError().ToString("0.000000") + " m");
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

            for(float v = 1; v < valorMaximoV; v = v + pasoV){

                for (float Fy = 1; Fy < valorMaximoF; Fy = Fy + pasoF)                                            //Bucle FOR con fuerza Fy, deduce Fx = f (Fy, distancia) y escoge mejor combinacion         
                {
                    Instance casoPrueba = new Instance(casosEntrenamientoDistancia.numAttributes());
                    casoPrueba.setDataset(casosEntrenamientoDistancia);
                    casoPrueba.setValue(1, Fy);                                                                   //crea un registro con una Fy
                    casoPrueba.setValue(2, distanciaObjetivo);                                                    //y la distancia
                    float Fx = (float)saberPredecirFuerzaX.classifyInstance(casoPrueba);                          //Predice Fx a partir de la distancia y una Fy 
                    if ((Fx >= 1) && (Fx <= valorMaximoA))
                    {
                        Instance casoPrueba2 = new Instance(casosEntrenamientoDistancia.numAttributes());
                        casoPrueba2.setDataset(casosEntrenamientoDistancia);                                                  //Utiliza el "modelo fisico aproximado" con Fx y Fy                 
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
                }     
            
            }                                                                                                //FIN DEL RAZONAMIENTO PREVIO
            if ((mejorFuerzaX == 0) && (mejorFuerzaY == 0)) { 
                print("NO SE LANZO");
            }
            else
            {
                InstanciaPelota = Instantiate(pelota) as GameObject;
                r = InstanciaPelota.GetComponent<Rigidbody>();                                                        //EN EL JUEGO: utiliza la pelota física del juego (si no existe la crea)
                //aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
                r.AddForce(new Vector3(mejorFuerzaX, 0, 0), ForceMode.Impulse);
                r.AddTorque(new Vector3(0,0,mejorFuerzaY));                            //la lanza en el videojuego con la fuerza encontrada
                print("DECISION REALIZADA: Se lanzó pelota con fuerza Fx =" + mejorFuerzaX + " y Fy= " + mejorFuerzaY);
                ESTADO = "Acción realizada";
            }
         } else {
             Time.timeScale = Velocidad_Simulacion;
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
