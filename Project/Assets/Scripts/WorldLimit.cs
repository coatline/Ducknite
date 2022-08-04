using Pixelbyte;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldLimit : MonoBehaviour {

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            var script = collision.gameObject.GetComponent<EnemyAI>();
            Pool.Recycle(script.EnemyHealthText);
            Pool.Recycle(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene(1);
            
        }
    }
}
