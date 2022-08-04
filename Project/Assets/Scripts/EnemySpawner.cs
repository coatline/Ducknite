using Pixelbyte;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {

    public Canvas worldSpaceCanvas;
    public GameObject[] enemies;
    public float timeTilSpawn = 1;
    private float timer;
    GameObject oldEnemy;
    string sideOfSpawnerFacingCrystal;
    public Transform enemyHolder;

    void Start () {
        timer = timeTilSpawn;
	}
	
	void Update () {
		if(timer <= 0)
        {
            timer = timeTilSpawn;
            var newEnemy = Pool.Spawn(enemies[0]);
            newEnemy.transform.SetParent(enemyHolder, true);
            newEnemy.GetComponent<HealthText>().worldSpaceCanvas = worldSpaceCanvas;
            newEnemy.transform.position = transform.position;
            
            //if (oldEnemy)
            //{
            //    newEnemy.transform.position -= new Vector3((sideOfSpawnerFacingCrystal == "right" ? 5 : -5), 0, 0);
            //}
            //oldEnemy = newEnemy;
        }
        else
        {
            timer -= Time.deltaTime;
        }
	}
}
