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
    weka.core.Instances casosEntrenamiento;//, casosEntrenamientoDistancia, casosEntrenamientoAngulo, casosEntrenamientoGastoGoma;
    private string ESTADO = "Sin conocimiento";
    public GameObject pelota;
    public pruebaScript ps, trueps;
    GameObject InstanciaPelota, PuntoObjetivo;
    public float valorMaximoA, valorMaximoF, pasoA, pasoF, Velocidad_Simulacion=1, valorMaximoG, pasoG;
    float mejorFuerzaX, mejorFuerzaY, distanciaObjetivo, anguloObjetivo, mejorAcelerar, mejorFrenar, mejorGiro;
    public Rigidbody trueBody;

    public GameObject objetoEstudiante;

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
        
        casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Iniciales_Experiencias.arff"));

        if (casosEntrenamiento.numInstances() < 10)
        {
            print("Datos de entrada: valorMaximoFx=" + valorMaximoA + " valorMaximoFy="+ valorMaximoF + "  " + ((valorMaximoA == 0 || valorMaximoF == 0) ? " ERROR: alguna fuerza es siempre 0" : ""));
            for(float f = 1; f <= valorMaximoF; f = f + pasoF){                 //Bucle de planificación de la velocidad a la que va
                for(float g = 1; g <= valorMaximoG; g = g + pasoG){                 //Bucle de planificacion de los grados de giro
                    for (float a = 1; a <= valorMaximoA; a = a +  pasoA){               //Bucle de planificacion de la aceleracion 
                        

                        //aaaaaaaaaaaaaaaaaaaaaaaa
                        InstanciaPelota = Instantiate(pelota) as GameObject;
                        Rigidbody rb = InstanciaPelota.GetComponent<Rigidbody>(); 
                        ps = InstanciaPelota.GetComponent<pruebaScript>();


                        //Gira, acelera y frena a la vez, el freno solo lo hace la mitad inicial de la curva (tiempo/2)
                        StartCoroutine("enumGiro", (int)g);
                        StartCoroutine("enumAcel",(int)a);
                        StartCoroutine("enumFre", (int)f);


                            
                        yield return new WaitUntil(() => (rb.transform.rotation.eulerAngles.y > 90));

                        Instance casoAaprender = new Instance(casosEntrenamiento.numAttributes());
                        print("ENTRENAMIENTO: con velocidad " + rb.velocity.sqrMagnitude + " con giro " + g + " con aceleracion " + a + " y frenado " + f + 
                        " Distancia recorrida y angulo final " + (InstanciaPelota.transform.position - posIni.position).sqrMagnitude + ", " + Vector3.Angle(posIni.forward, InstanciaPelota.transform.position));
                        casoAaprender.setDataset(casosEntrenamiento);                          
                        casoAaprender.setValue(0, g);
                        casoAaprender.setValue(1, a);
                        casoAaprender.setValue(2, f);
                        casoAaprender.setValue(3, Mathf.Sqrt((rb.transform.position - posTarget.position).sqrMagnitude));        //Distancia alcanzada
                        //casoAaprender.setValue(4, Vector3.Angle(posIni.forward, (InstanciaPelota.transform.position-posIni.position)));

                        casosEntrenamiento.add(casoAaprender);




                        Destroy(InstanciaPelota,0);                                             
                                                                                               
                    }
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
        //Predicciones
        saberPredecirGiro = new M5P();                                                
        casosEntrenamiento.setClassIndex(0);                                             
        saberPredecirGiro.buildClassifier(casosEntrenamiento); 

      

        saberPredecirDistancia = new M5P();                                                
        casosEntrenamiento.setClassIndex(3);                                             
        saberPredecirDistancia.buildClassifier(casosEntrenamiento);  

        
           

        ESTADO = "Con conocimiento";

        print(casosEntrenamiento.numInstances() +" espers "+ saberPredecirDistancia.toString());

        //EVALUACION DEL CONOCIMIENTO APRENDIDO: 
        //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA partir de aqui para abajo, corregir todo
        print("Etapa de crossevaluation ");
        if (casosEntrenamiento.numInstances() >= 10){
            Evaluation evaluador = new Evaluation(casosEntrenamiento);       
              
            
            casosEntrenamiento.setClassIndex(0);
            evaluador.crossValidateModel(saberPredecirGiro, casosEntrenamiento, 10, new java.util.Random(1));
            
            casosEntrenamiento.setClassIndex(3);
            evaluador.crossValidateModel(saberPredecirDistancia, casosEntrenamiento, 10, new java.util.Random(1));
          

        }
        distanciaObjetivo = (objetoEstudiante.transform.position - posTarget.position).sqrMagnitude;   
    }

    

    public float prediccionDistancia;
    //FALTA POR ENTENDER
    void FixedUpdate()                                                                                 
    {
        if ((ESTADO == "Con conocimiento") && (distanciaObjetivo > 0))
        {
            Time.timeScale = 1;                                                                                
            float menorDistancia =  1e20f;
            print("-- OBJETIVO: GIRAR HACIA EL OBJETIVO SITUADO A UNA DISTANCIA DE " + distanciaObjetivo + " m.");
       
            //Si usa dos bucles Fx y Fy con "modelo fisico aproximado", complejidad n^2
            //Reduce la complejidad con un solo bucle FOR, así

            for(float a = 1; a <= valorMaximoA; a = a + pasoA*2){                 //Bucle de planificación de la velocidad a la que va
                 for(float f = 1; f <= valorMaximoF; f = f + pasoF*2){                 //Bucle de planificacion de los grados de giro
                    Instance casoPruebaAngulo = new Instance(casosEntrenamiento.numAttributes());
                    casoPruebaAngulo.setDataset(casosEntrenamiento);
                    casoPruebaAngulo.setValue(1,a);
                    casoPruebaAngulo.setValue(2,f);
                    casoPruebaAngulo.setValue(3, distanciaObjetivo);
                    float giro = (float)saberPredecirGiro.classifyInstance(casoPruebaAngulo)%45;
                    print("GIRO" + giro);
                    if(giro >= 1){
                        Instance casoPruebaAngulo2 = new Instance(casosEntrenamiento.numAttributes());
                        casoPruebaAngulo2.setDataset(casosEntrenamiento);
                        casoPruebaAngulo2.setValue(0, giro);
                        casoPruebaAngulo2.setValue(1,a);
                        casoPruebaAngulo2.setValue(2,f);
                        
                        //ERROR AQUI SEGURAMENTE
                        print(menorDistancia);
                        
                        prediccionDistancia = (float)saberPredecirDistancia.classifyInstance(casoPruebaAngulo2);
                        print("PreDis" + prediccionDistancia);

                        if(prediccionDistancia < menorDistancia){
                            menorDistancia = prediccionDistancia;
                            mejorAcelerar = 5;
                            mejorFrenar = 1;
                            mejorGiro = giro;
                        }   
                    }
                }
            }                                                                                         //FIN DEL RAZONAMIENTO PREVIO
            if ((mejorAcelerar == 0) && (mejorFrenar == 0) && (mejorGiro == 0)) { 
                print("NO SE LANZO");
            }
            else
            {
                float givalue  =mejorGiro % 45;
                Velocidad_Simulacion = 1;
                ps = objetoEstudiante.GetComponent<pruebaScript>();
                
                StartCoroutine("enumGiro", (int)givalue);
                StartCoroutine("enumAcel",(int)mejorAcelerar);
                StartCoroutine("enumFre", (int)mejorFrenar);
                                         
                print("DECISION REALIZADA: Giro= " + givalue + ", aceleracion= " + mejorAcelerar + ", frenada= " + mejorFrenar);
            }
         } 
        
    }
}
