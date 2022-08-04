using Pixelbyte;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public float lifeSpan = .5f;
    public int dmg;

	void Start () {
        Invoke("Die", lifeSpan);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Platform")
        {
            Die();
        }
        else if (collision.gameObject.tag == "Enemy") {
            Die();
        }
        else if (collision.gameObject.tag != "Turret") { Die(); }
    }

    void Die()
    {
        Pool.Recycle(gameObject,0);
    }
}
