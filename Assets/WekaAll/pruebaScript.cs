using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pruebaScript : MonoBehaviour
{
    const int MAXIMO_GIRO = 25;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        rb.AddForce(transform.up*-1*rb.velocity.sqrMagnitude*5, ForceMode.Force);
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
                w.steerAngle = 0;
            }
        }
    }
    public void pisarAcelerador(int intensidad)
    {
        if (gameObject.GetComponent<Rigidbody>().velocity.sqrMagnitude < 900)
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
            w.brakeTorque = 100 * intensidad;
        }
    }
}
