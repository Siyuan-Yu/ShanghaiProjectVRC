using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    public GameObject cokePrefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 randomSpawnPosition = new Vector3(Random.Range(120, 170), 10, Random.Range(-15, 15));
            Instantiate(cokePrefab, randomSpawnPosition, Quaternion.identity);
        }
    }
}