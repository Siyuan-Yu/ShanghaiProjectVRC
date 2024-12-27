using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [InfoBox("Use Keyboard G to generate coke")]
    public GameObject cokePrefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Vector3 randomSpawnPosition = new Vector3(Random.Range(120, 170), 10, Random.Range(-15, 15));
            Instantiate(cokePrefab, randomSpawnPosition, Quaternion.identity);
        }
    }
}