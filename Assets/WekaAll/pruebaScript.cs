using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pruebaScript : MonoBehaviour
{
    const int MAXIMO_GIRO = 25;
    // Start is called before the first frame update
    void Start()
    {
        /*foreach (WheelCollider w in gameObject.GetComponentsInChildren<WheelCollider>()) {
            w.motorTorque = 1000;
        }
        foreach (WheelCollider w in gameObject.GetComponentsInChildren<WheelCollider>())
        {
            if (w.CompareTag("RuedaDelantera"))
            {
               // w.steerAngle = 20;
            }
        }*/
        
    }

    // Update is called once per frame
    void Update()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        rb.AddForce(transform.up*-1*rb.velocity.sqrMagnitude*5, ForceMode.Force);
        // if (Time.time < 2.5)
        // {
        //     acelerar();
        //     enderezar();
        // }else if (Time.time < 4)
        // {
        //     frenar();
        //     girarDerecha(45);
        // }
        // else if (Time.time < 6)
        // {
        //     acelerar();
        //     girarDerecha(45);
        // }
        // else if (Time.time < 8)
        // {
        //     acelerar();
        //     enderezar();
        // }
        // else if (Time.time < 15)
        // {
        //     frenar();
        //     girarIzquierda(45);
        // }


    }

    //angulos entre 0 y 45
    public void girarDerecha(int angulos)
    {
        foreach (WheelCollider w in gameObject.GetComponentsInChildren<WheelCollider>())
        {
            if (w.CompareTag("RuedaDelantera"))
            {
                if (w.steerAngle < MAXIMO_GIRO && w.steerAngle < angulos)
                {
                    w.steerAngle++;
                }
            }
        }
    }

    //angulos entre 0 y 45
    public void girarIzquierda(int angulos)
    {
        foreach (WheelCollider w in gameObject.GetComponentsInChildren<WheelCollider>())
        {
            if (w.CompareTag("RuedaDelantera"))
            {
                if (w.steerAngle > -MAXIMO_GIRO && w.steerAngle > -angulos)
                {
                    w.steerAngle--;
                }
            }
        }
    }

    public void enderezar()
    {
        foreach (WheelCollider w in gameObject.GetComponentsInChildren<WheelCollider>())
        {
            if (w.CompareTag("RuedaDelantera"))
            {
                /*if (w.steerAngle > -1)
                {
                    w.steerAngle--;
                }else if (w.steerAngle < 1)
                {
                    w.steerAngle++;
                }
                else
                {
                    w.steerAngle = 0;
                }*/
                w.steerAngle = 0;
            }
        }
    }

    public void frenar()
    {
        foreach (WheelCollider w in gameObject.GetComponentsInChildren<WheelCollider>())
        {
            w.motorTorque = 1000;
            w.brakeTorque = 500;
        }
    }

    //intensidad de 1 a 10
    public void frenar(int intensidad)
    {
        foreach (WheelCollider w in gameObject.GetComponentsInChildren<WheelCollider>())
        {
            w.motorTorque = 100 * intensidad;
            w.brakeTorque = 50 * intensidad;
        }
    }

    public void acelerar()
    {
        foreach (WheelCollider w in gameObject.GetComponentsInChildren<WheelCollider>())
        {
            w.motorTorque = 1000;
            w.brakeTorque = 0;
        }
    }

    //intensidad de 1 a 10
    public void acelerar(int intensidad)
    {
        foreach (WheelCollider w in gameObject.GetComponentsInChildren<WheelCollider>())
        {
            w.motorTorque = 100 * intensidad;
            w.brakeTorque = 0;
        }
    }

    public void pisarAcelerador(int intensidad)
    {
        if (gameObject.GetComponent<Rigidbody>().velocity.sqrMagnitude < 300)
        {
            foreach (WheelCollider w in gameObject.GetComponentsInChildren<WheelCollider>())
            {
                w.motorTorque = 100 * intensidad;
            }
        }
        else
        {
            foreach (WheelCollider w in gameObject.GetComponentsInChildren<WheelCollider>())
            {
                w.motorTorque = 0;
            }
        }

    }

    public void pisarFreno(int intensidad)
    {
        foreach (WheelCollider w in gameObject.GetComponentsInChildren<WheelCollider>())
        {
            w.brakeTorque = 10 * intensidad;
        }
    }
}
