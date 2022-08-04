using Pixelbyte;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour {

    SpriteRenderer sr;
    public Bullet bulletPrefab;	
    float timer = 0;
    public float shotRate = .8f;
    public int damage = 2;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update () {
        if(timer <= 0)
        {
            //shoot
            var newBullet = Pool.Spawn(bulletPrefab);
            newBullet.transform.position = new Vector3(transform.position.x + (sr.flipX ? -.05f : .05f), transform.position.y + .4f);
            var bulletrb = newBullet.GetComponent<Rigidbody2D>();
            Vector2 force = new Vector2(1000 * (sr.flipX ? -1 : 1), Random.Range(-200, 150));
            bulletrb.AddForce(force);
            newBullet.dmg = damage;
            timer = shotRate;
        }
        else if(timer > 0)
        {
            timer -= Time.deltaTime;
        }
	}
}
