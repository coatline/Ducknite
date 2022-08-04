using Pixelbyte;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    Animator anim;
    public GameObject[] contents;
    bool opened;
    bool finished;

    private void Awake()
    {
        //have seperate spawnpoint script that spawns chests
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        i = Random.Range(0, contents.Length);
        var script = contents[i].GetComponent<PickupAble>();
        if (script.type == "Shotgun" || script.type == "Health")
        {
            i = Random.Range(0, contents.Length);
        }

        myContents.content = contents[i];
    }

    int i = 0;
    int openHash = Animator.StringToHash("open");

    public class Contents
    {
        public GameObject content;
    }

    public Contents myContents = new Contents();

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player") && !opened)
        {
            var weapon = Pool.Spawn(myContents.content);
            weapon.transform.position = transform.position + new Vector3(0, 0f, 0);
            var weaponRB = weapon.GetComponent<Rigidbody2D>();
            Vector2 force = new Vector2(Random.Range(-200, 200), 330);
            weaponRB.AddForce(force);

            opened = true;
            //anim.SetTrigger("open");
            anim.SetTrigger(openHash);
        }
    }


}
