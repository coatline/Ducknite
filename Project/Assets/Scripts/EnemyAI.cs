using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Pixelbyte;

public class EnemyAI : MonoBehaviour
{

    HealthText myHealthTextScript;
    public int damage;
    public int startingHealth;
    TMP_Text healthtext;
    public float movementSpeed = 1;
    GameObject crystal;
    SpriteRenderer sr;

    public class Stats
    {
        public int health;
        public int atkDmg;
    }

    public Stats myStats = new Stats();

    public TMP_Text EnemyHealthText
    {
        get
        {

            return healthtext;
        }
        set
        {

        }
    }

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        startingHealth = Random.Range(6,16);
        if(startingHealth >= 12)
        {
            sr.color = new Color(255, 100, 100);
        }
        crystal = GameObject.FindGameObjectWithTag("Crystal");
        myStats.health = startingHealth;
        myStats.atkDmg = damage;
        myHealthTextScript = GetComponent<HealthText>();
        healthtext = myHealthTextScript.myText;
        healthtext.text = myStats.health + "/" + startingHealth;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.tag == "Bullet")
        {
            var bulletScript = collision.gameObject.GetComponent<Bullet>();
            myStats.health -= bulletScript.dmg;
            healthtext.text = myStats.health + "/" + startingHealth;

            if (myStats.health <= 0)
            {
                Invoke("Die", .05f);
            }
        }
    }

    private void Update()
    {
        GoAfterTarget(crystal.transform);
        transform.rotation = Quaternion.identity;
    }

    void Die()
    {
        Pool.Recycle(gameObject);
        Pool.Recycle(healthtext);
    }

    void GoAfterTarget(Transform target)
    {
        var dir = Vector3.MoveTowards(transform.position, target.position, Time.deltaTime * movementSpeed);
        transform.position = dir;
    }

    private void OnEnable()
    {
        myStats.health = startingHealth;
        if (healthtext)
            healthtext.text = myStats.health + "/" + startingHealth;

    }
}
