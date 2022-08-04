using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupAble : MonoBehaviour
{
    Collider2D col;
    Rigidbody2D rb;
    SpriteRenderer mySR;
    SpriteRenderer rootSr;
    Player player;
    bool isBeingHeld;
    float colTimer;
    public bool isgun;
    public int damage = 0;
    public string type;

    private void Awake()
    {
        mySR = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        col.enabled = false;
        colTimer = .35f;
    }

    public void Pickup(Transform parent, SpriteRenderer playerSr)
    {
        transform.SetParent(parent);
        player = transform.GetComponentInParent<Player>();
        isBeingHeld = true;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        col.enabled = false;
        rb.velocity = Vector2.zero;
        rb.simulated = false;
        rootSr = playerSr;
    }

    public void Drop(Vector2 dropVel, float angularVel, float colliderTimer)
    {
        isBeingHeld = false;
        rb.simulated = true;
        rb.angularVelocity = angularVel;
        transform.SetParent(null);
        rb.velocity = dropVel;
        rootSr = null;
        colTimer = colliderTimer;
    }


    void Update()
    {
        if (colTimer > 0)
        {
            colTimer -= Time.deltaTime;
            if (colTimer <= 0)
            {
                col.enabled = true;
            }
        }

        if (!isgun) { return; }

        if (isBeingHeld)
        {

            if (player.shot && !player.isWalking)
            {
                return;
            }

            //if the player is walking 
            if (rootSr && player.isWalking && player.canJump || rootSr && player.isWalking && !player.canJump)
            {
                mySR.flipX = rootSr.flipX;
            }

            //if the player is idle 
            else if (player.canJump && !player.isWalking || !player.canJump && !player.isWalking)
            {
                mySR.flipX = true;
            }

        }


    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Enemy"))&& colTimer > 0)
        {
            colTimer = 0;
            col.enabled = true;
        }
    }
}
