using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    float prevTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        prevTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        float currentTime = Time.time; 
        if (currentTime - prevTime > 1){
            Debug.Log("Spawn a New Enemy");
            GameObject enemy = Instantiate(enemyPrefab);
            enemy.transform.position = gameObject.transform.position;
            prevTime = currentTime;
        }
    }
}
