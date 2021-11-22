using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Movement : MonoBehaviour
{

    UnityEngine.AI.NavMeshAgent agent;

    void Start()
    {
        agent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
        //Start the coroutine we define below named ExampleCoroutine.
        StartCoroutine(StopHammerTime());
    }

    IEnumerator StopHammerTime()
    {
        //Print the time of when the function is first called.
        Debug.Log("Started Coroutine at timestamp : " + Time.time);

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(5);
        Debug.Log("Stopping agent at : " + Time.time);
        agent.isStopped = true;

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(5);
        agent.isStopped = false;

        //After we have waited 5 seconds print the time again.
        Debug.Log("Finished Coroutine at timestamp : " + Time.time);
    }
}
