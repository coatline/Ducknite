using Pixelbyte;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnChests : MonoBehaviour
{

    GameObject[] spawnPoints;
    public GameObject chestPrefab;
    public int chestAmount;

    void Start()
    {

        for (int i = chestAmount; i > 0; i--)
        {
            spawnPoints = GameObject.FindGameObjectsWithTag("ChestSpawnPoint");
            int j = Random.Range(0, spawnPoints.Length);

            if (spawnPoints[j] != null && (spawnPoints.Length >= 0))
            {
                var newChest = Pool.Spawn(chestPrefab);
                newChest.transform.position = spawnPoints[j].transform.position;
                Pool.Recycle(spawnPoints[j]);
            }

        }
    }

    //void Update()
    //{

    //}
}
