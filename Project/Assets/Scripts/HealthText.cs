using Pixelbyte;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HealthText : MonoBehaviour {

    public TMP_Text healthTextPrefab;
    public TMP_Text myText;
    public Canvas worldSpaceCanvas;
    EnemyAI eai;
    Crystal crystal;
    public bool isEnemy;
    public bool isCrystal;

	void Awake () {
        myText = Pool.Spawn(healthTextPrefab);

        if (isEnemy)
        {
            eai = GetComponent<EnemyAI>();
        }
        else if (isCrystal)
        {
            crystal = GetComponent<Crystal>();
        }

    }

    void Update () {
        if (isEnemy)
        {
            PlaceTxts(myText, eai.transform, worldSpaceCanvas, new Vector2(.4f, 1));

            if (eai.myStats.health <= 0)
            {
                Pool.Recycle(gameObject);
            }
        }
        else if (isCrystal)
        {
            PlaceTxts(myText, crystal.transform, worldSpaceCanvas, new Vector2(.25f, 2.3f));

            if(crystal.currentHealth <= 0 && !crystal.isEnemySpawner)
            {
                // * Change scene
                // * You lost
                // * Break animation
                print("You lost!");
            }
        }

        
	}

    void PlaceTxts(TMP_Text txt, Transform objectTrans, Canvas worldCanvas, Vector2 offset)
    {
        if(txt == null) { Debug.LogError("Could not find txt"); return; }
        if (txt.transform.parent == null)
        {
            txt.transform.SetParent(worldCanvas.transform);
        }

        txt.transform.position = objectTrans.position + new Vector3(offset.x, offset.y, 0);
    }

}
