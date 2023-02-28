using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    bool reached;
    public Vector3 randomWP{get; private set;}

    // Start is called before the first frame update
    void Start()
    {
        randomWP = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRandomWP();
    }

    // Get Random position in NavMesh area that be inside specified center and range
    bool GetRandomPosition(Vector3 center, float range, out Vector3 resultPos)
    {
        resultPos = center;
        bool res = false;
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                resultPos = hit.position;
                center = resultPos;
                res = true;
            }
        }
        return res;
    }

    // Get random waypoint inside NavMesh.
    // Hold state as below
    //  - Be reached waypoint ? 
    //  - Current waypoint
    //      If reach WP, then generate new WP
    void UpdateRandomWP()
    {
        // reached check
        bool reached = false;
        if ((transform.position - randomWP).magnitude < 5){
            reached = true;
            Debug.Log("Reached WP");
        }
        if (reached){
            var res = GetRandomPosition(transform.position, 20, out Vector3 resultPos);
            if (res){
                // success for generate new WP
                reached = false;
                randomWP = resultPos;
                Debug.Log(string.Format("New Waypoint : [{0}, {1}, {2}", randomWP[0], randomWP[1], randomWP[2]));
            }
        }
    }
}
