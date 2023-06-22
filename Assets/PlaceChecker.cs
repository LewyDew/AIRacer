using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlaceChecker : MonoBehaviour
{
    public GameObject[] waypoints;
    public int leadingWP = 0;
    public GameObject[] racers;
    public GameObject learner;

    private void Update()
    {
        racers = racers.OrderBy(x => Vector3.Distance(x.transform.position, waypoints[leadingWP].transform.position)).ToArray();
        for (int i =0; i < racers.Length; i ++)
        {
           // Debug.Log("Racer in place " + (i + 1) + " is " + racers[i].GetInstanceID());
        }
        if (racers[0] == learner)
        {
            Debug.Log("Leaner in 1st place ");
        }
        if (racers[1] == learner)
        {
            Debug.Log("Leaner in 2nd place ");
        }
        if (racers[2] == learner)
        {
            Debug.Log("Leaner in 3rd place ");
        }
        if (racers[3] == learner)
        {
            Debug.Log("Leaner in 4th place ");
        }
        if (Vector3.Distance(racers[0].transform.position, waypoints[leadingWP].transform.position) < 5)
        {
            leadingWP++;
            
        }
        if (Vector3.Distance(racers[1].transform.position, waypoints[leadingWP].transform.position) < 5)
        {
            leadingWP++;
        }
        if (Vector3.Distance(racers[2].transform.position, waypoints[leadingWP].transform.position) < 5)
        {
            leadingWP++;
        }
        if (Vector3.Distance(racers[3].transform.position, waypoints[leadingWP].transform.position) < 5)
        {
            leadingWP++;
        }
        if (leadingWP >= 12)
        {
           
            
            leadingWP = 0;
            racers[0].GetComponent<CarControllerAI>().EndEpisode();
            racers[1].GetComponent<CarControllerAI>().EndEpisode();
            racers[2].GetComponent<CarControllerAI>().EndEpisode();
            racers[3].GetComponent<CarControllerHuman>().EndEpisode();
        }

    }
}
