using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{

    public NavMeshAgent agent;

    public GameObject pointFather;
    public Transform[] points;

    public int i = 0;
    // Start is called before the first frame update
    void Start()
    {
        points = pointFather.GetComponentsInChildren<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Mathf.Abs(agent.remainingDistance - agent.stoppingDistance) < 10){
            //i = ((i + 1) % points.Length);
            i = (i % points.Length) + 1;
            
            agent.SetDestination(points[i].position);
        }
    }
}
