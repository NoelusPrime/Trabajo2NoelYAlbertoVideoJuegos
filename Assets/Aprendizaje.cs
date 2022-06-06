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
    public float valorMaximoA, pasoA, Velocidad_Simulacion=1, valorMaximoG, pasoG;
    float mejorFuerzaX, mejorFuerzaY, distanciaObjetivo, anguloObjetivo, mejorAcelerar, mejorGiro;
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
                for(float g = 0; g <= valorMaximoG; g = g + pasoG){                 //Bucle de planificacion de los grados de giro
                    for (float a = 1; a <= valorMaximoA; a = a +  pasoA){               //Bucle de planificacion de la aceleracion 

                            //aaaaaaaaaaaaaaaaaaaaaaaa
                            InstanciaPelota = Instantiate(pelota) as GameObject;
                            Rigidbody rb = InstanciaPelota.GetComponent<Rigidbody>();
                            ps = InstanciaPelota.GetComponent<pruebaScript>();


                            //Gira, acelera y frena a la vez, el freno solo lo hace la mitad inicial de la curva (tiempo/2)
                            StartCoroutine("enumGiro", (int)g);
                            StartCoroutine("enumAcel", (int)a);



                            yield return new WaitUntil(() => (rb.transform.rotation.eulerAngles.y > 180));

                            Instance casoAaprender = new Instance(casosEntrenamiento.numAttributes());
                            print("ENTRENAMIENTO: con velocidad " + rb.velocity.sqrMagnitude + " con giro " + g + " con aceleracion " + a +
                            " Distancia recorrida y angulo final " + (InstanciaPelota.transform.position - transform.position).sqrMagnitude + ", " + Vector3.Angle(posIni.forward, InstanciaPelota.transform.position));
                            casoAaprender.setDataset(casosEntrenamiento);
                            casoAaprender.setValue(0, g);
                            casoAaprender.setValue(1, a);
                            casoAaprender.setValue(2, (rb.transform.position - transform.position).sqrMagnitude);        //Distancia alcanzada

                            casosEntrenamiento.add(casoAaprender);




                            Destroy(InstanciaPelota, 0);
                                                                    
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

        saberPredecirAceleracion = new M5P();                                                
        casosEntrenamiento.setClassIndex(1);                                             
        saberPredecirAceleracion.buildClassifier(casosEntrenamiento);  

        saberPredecirDistancia = new M5P();                                                
        casosEntrenamiento.setClassIndex(2);                                             
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
            
            casosEntrenamiento.setClassIndex(1);
            evaluador.crossValidateModel(saberPredecirAceleracion, casosEntrenamiento, 10, new java.util.Random(1));

            casosEntrenamiento.setClassIndex(2);
            evaluador.crossValidateModel(saberPredecirDistancia, casosEntrenamiento, 10, new java.util.Random(1));
          

        }
        Velocidad_Simulacion = 2; 
    }

    

    public float prediccionDistancia;
    
    void Calcular(){
        distanciaObjetivo = (posIni.transform.position - posTarget.position).sqrMagnitude;  
        float menorDistancia =  1e20f;
        print("-- OBJETIVO: GIRAR HACIA EL OBJETIVO SITUADO A UNA DISTANCIA DE " + distanciaObjetivo + " m.");
       
        //Si usa dos bucles Fx y Fy con "modelo fisico aproximado", complejidad n^2
        //Reduce la complejidad con un solo bucle FOR, así

        for(float a = 1; a <= valorMaximoA; a = a + pasoA*2){                 //Bucle de planificación de la velocidad a la que va
            Instance casoPruebaAngulo = new Instance(casosEntrenamiento.numAttributes());
            casoPruebaAngulo.setDataset(casosEntrenamiento);
            casoPruebaAngulo.setValue(1,a);
            casoPruebaAngulo.setValue(2, distanciaObjetivo);
            float giro = (float)saberPredecirGiro.classifyInstance(casoPruebaAngulo);
            print("GIRO" + giro);
            if(giro >= 1){
                Instance casoPruebaAngulo2 = new Instance(casosEntrenamiento.numAttributes());
                casoPruebaAngulo2.setDataset(casosEntrenamiento);
                casoPruebaAngulo2.setValue(0, giro);
                casoPruebaAngulo2.setValue(2, distanciaObjetivo);
                float acel = (float)saberPredecirAceleracion.classifyInstance(casoPruebaAngulo2);
                print("ACEL" + acel);

                if(acel >= 1){
                    Instance casoPruebaAngulo4 = new Instance(casosEntrenamiento.numAttributes());
                    casoPruebaAngulo4.setDataset(casosEntrenamiento);
                    casoPruebaAngulo4.setValue(0, giro);
                    casoPruebaAngulo4.setValue(1, acel);
                    menorDistancia = (float)saberPredecirDistancia.classifyInstance(casoPruebaAngulo4);
                    print("PreDis" + prediccionDistancia);

                    if(prediccionDistancia < menorDistancia){
                        menorDistancia = prediccionDistancia;
                        mejorAcelerar = acel;
                        mejorGiro = giro;
                    }
                }            
            }   
                    
        }
    }

    void FixedUpdate()                                                                                 
    {
        Time.timeScale = Velocidad_Simulacion;
        if(ESTADO == "Con conocimiento"){
            objetoEstudiante.SetActive(true);
            Calcular();

            if ((mejorAcelerar == 0) && (mejorGiro == 0)) { 
                print("NO SE LANZO");
            } else {
                ps = objetoEstudiante.GetComponent<pruebaScript>();

                Vector3 posDirectionLocal = objetoEstudiante.transform.InverseTransformPoint(posTarget.transform.position);



                if(posDirectionLocal.x > 1){
                    ps.girarDerecha((int)mejorGiro);
                    
                } else if(posDirectionLocal.x < -1){
                    ps.girarIzquierda((int)mejorGiro);
                    
                } else {
                    ps.enderezar();
                }
                //ps.pisarAcelerador((int)mejorAcelerar);

                //print("distancia: "+Vector3.Distance(objetoEstudiante.transform.position, pelota.transform.position));
                //print("cosa rara: " + posDirectionLocal);
                //print("objeto estudiante: "+objetoEstudiante.transform.position);
                //print("pelota: "+ GameObject.Find("Agent").transform.position);
                print(objetoEstudiante.GetComponent<Rigidbody>().velocity.sqrMagnitude);


                //if (objetoEstudiante.GetComponent<Rigidbody>().velocity.sqrMagnitude > 2000 || Vector3.Distance(objetoEstudiante.transform.position,GameObject.Find("Agent").transform.position) < 10)
                if (Vector3.Distance(objetoEstudiante.transform.position, GameObject.Find("Agent").transform.position) < 10)
                {
                    ps.pisarFreno(3);
                    ps.pisarAcelerador(0);
                }
                else
                {
                    ps.pisarFreno(0);
                    ps.pisarAcelerador((int)mejorAcelerar);
                }

                print("DECISION REALIZADA: Giro= " + mejorGiro + ", aceleracion= " + mejorAcelerar);
            }  

            if(posTarget.name != "Agent"){
                if((objetoEstudiante.transform.position - posTarget.transform.position).sqrMagnitude < 5){
                    print("POSICION ALCANZADA");
                    posIni.position = posTarget.position;
                    objetoEstudiante.GetComponent<Rigidbody>().isKinematic = true;
                } else {
                    objetoEstudiante.GetComponent<Rigidbody>().isKinematic = false;
                }
            }
            //Si se tumba, se resetea, no lo he probado
            if(objetoEstudiante.transform.rotation.z > 40){
                objetoEstudiante.transform.position = new Vector3(0,0,0);
                objetoEstudiante.transform.rotation = new Quaternion(0,0,0,0);
            }
        }
    }
}

