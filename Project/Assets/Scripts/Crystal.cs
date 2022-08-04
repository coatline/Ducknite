using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour {

    public int startingHealth;
    public int currentHealth;
    public bool isEnemySpawner;
    public Bar healthBar;
    HealthText mytext;

    private void Start()
    {
        currentHealth = startingHealth;

        mytext = GetComponent<HealthText>();

        mytext.myText.text = currentHealth + "/" + startingHealth;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (collision.gameObject.CompareTag("Bullet") && isEnemySpawner)
        {
            var dmg = collision.gameObject.GetComponent<Bullet>().dmg;
            healthBar.GotHurt(dmg, currentHealth);
            currentHealth -= dmg;
            mytext.myText.text = currentHealth + "/" + startingHealth;
        }
        else if (collision.gameObject.CompareTag("Enemy") && !isEnemySpawner)
        {
            print("Alert!!! The Crystal is getting attacked!");
            var dmg = collision.gameObject.GetComponent<EnemyAI>().damage;
            healthBar.GotHurt(dmg, currentHealth);
            currentHealth -= dmg;
            mytext.myText.text = currentHealth + "/" + startingHealth;
        }
    }
}
