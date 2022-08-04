using Pixelbyte;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    static readonly Vector2 THROW_VELOCITY = new Vector2(6, 3);
    static readonly float SPIN = 500;

    Animator animator;
    SpriteRenderer sr;
    Rigidbody2D rb;
    public float speed;
    public Transform holdingPlace;
    public bool autoPickup;
    public float jumpHeight;
    int walkingHash = Animator.StringToHash("Walking");
    int walkingWithGunHash = Animator.StringToHash("Walking With Gun");
    int jumpingHash = Animator.StringToHash("Jumped");
    int landHash = Animator.StringToHash("Landed");
    float colTimer = 0;
    public int startingHealth = 5;
    public TMP_Text healthTextPrefab;

    TMP_Text proText;
    public Canvas canvas;
    public Canvas worldSpaceCanvas;
    public TMP_Text ammoTextPrefab;
    public TMP_Text ammoAmountText;
    public TMP_Text AtkDamageTxt;
    TMP_Text myHealthText;

    public bool isWalking;

    public bool canJump;
    public int startingBullets;
    public GameObject bulletPrefab;
    public GameObject healthPackIconPrefab;
    public bool shot = false;
    public GameObject panel;

    void Start()
    {

        myHand.healthPackGobjs = new GameObject[6];

        AtkDamageTxt.text = "Atk: " + myStats.atkDmg;
        myStats.health = startingHealth;
        myStats.atkDmg = 0;

        myHand.bulletAmount = startingBullets;

        if (ammoAmountText)
        {
            ammoAmountText.text = "Ammo: " + myHand.bulletAmount;
        }

        myHealthText = Pool.Spawn(healthTextPrefab);
        myHealthText.transform.SetParent(worldSpaceCanvas.transform);
        UpdateHealthText();

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    public class Stats
    {
        public int health;
        public int atkDmg;
    }

    public class Inventory
    {
        public GameObject currentWeapon;
        public int bulletAmount;
        public GameObject[] healthPackGobjs = null;
        public int healthPackAmount;
    }

    public Inventory myHand = new Inventory();
    public Stats myStats = new Stats();

    void Update()
    {
        HealthTextFollowMe();
        GetKeys();

        if (myStats.health <= 0)
        {
            print("YOU HAVE DIED!");
            SceneManager.LoadScene(1);
        }

        if (rb.velocity.y <= .001f && rb.velocity.y >= 0)
        {
            canJump = true;
        }
        else
        {
            canJump = false;
        }

        if (landed && canJump)
        {
            landed = false;
            animator.SetTrigger(landHash);
            animator.ResetTrigger(jumpingHash);
        }

        if (shot && !isWalking && myHand.currentWeapon)
        {
            holdingPlace.transform.localPosition = new Vector2(.03f * (sr.flipX ? -1 : 1), 0);
            holdingPlace.transform.rotation = Quaternion.identity;
            var gunSpriteRend = myHand.currentWeapon.GetComponent<SpriteRenderer>();
            if (sr.flipX == true)
            {
                gunSpriteRend.flipX = true;
            }
            else
            {
                gunSpriteRend.flipX = false;
            }
        }
        else if (shot && isWalking)
        {
            shot = false;
        }

        if (lift)
        {
            proText.alpha -= Time.deltaTime;

            if (proText.alpha <= 0)
            {
                lift = false;
                proText.color = new Color(proText.color.r, proText.color.g, proText.color.b, 255);
                Destroy(proText);
                proText = null;
            }
        }
    }

    bool lift;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!autoPickup)
        {
            if (collision.gameObject.CompareTag("Gun") && myHand.currentWeapon == null && Input.GetKeyDown(KeyCode.E))
            {
                myHand.currentWeapon = collision.gameObject;
                var gun = collision.gameObject.GetComponent<PickupAble>();
                if (gun != null)
                    gun.Pickup(holdingPlace, sr);
            }
        }

        else if (autoPickup)
        {
            if (collision.gameObject.CompareTag("Gun") && myHand.currentWeapon == null)
            {
                myHand.currentWeapon = collision.gameObject;
                var gun = collision.gameObject.GetComponent<PickupAble>();
                if (gun != null)
                    gun.Pickup(holdingPlace, sr);

                myStats.atkDmg = gun.damage;
                AtkDamageTxt.text = "Atk: " + myStats.atkDmg;

                //switch (gun.gunType)
                //{
                //    case "Shotgun":
                //        myStats.atkDmg = 5;
                //        AtkDamageTxt.text = "Atk: " + myStats.atkDmg;
                //        break;
                //    case "Pistol":
                //        myStats.atkDmg = 3;
                //        AtkDamageTxt.text = "Atk: " + myStats.atkDmg;
                //        break;
                //}
            }
            else if (collision.gameObject.CompareTag("Ammo"))
            {
                int ammoAmount = Random.Range(3, 16);

                if (ammoAmount > 10)
                {
                    ammoAmount = Random.Range(3, 16);
                }


                if (proText != null)
                {
                    proText.text = $"+ {ammoAmount} Ammo";
                }
                else
                {
                    proText = Pool.Spawn(ammoTextPrefab);
                    proText.transform.SetParent(worldSpaceCanvas.transform);
                    proText.text = $"+ {ammoAmount} Ammo";
                }
                print($"+ {ammoAmount} Ammo");

                myHand.bulletAmount += ammoAmount;

                proText.transform.position = collision.gameObject.transform.localPosition;

                Destroy(collision.gameObject);

                lift = true;
                ammoAmountText.text = "Ammo: " + myHand.bulletAmount;

                ammoAmountText.color = new Color(255, 255, 255);
            }

        }
    }

    void Heal()
    { 
        myStats.health += 2;
        if (myStats.health > startingHealth)
        {
            myStats.health = startingHealth;
        }

        //Remove it
        Destroy(myHand.healthPackGobjs[myHand.healthPackAmount]);
        myHand.healthPackAmount--;
        UpdateHealthText();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            landed = false;
            animator.ResetTrigger(landHash);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            landed = true;
            rb.velocity = new Vector2(0, 0);
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            myStats.health -= collision.gameObject.GetComponent<EnemyAI>().myStats.atkDmg;

            Vector2 force = new Vector2(150 * (collision.gameObject.transform.position.x < transform.position.x ? 5 : -5), 150);
            rb.AddForce(force);
            UpdateHealthText();
        }

        else if (collision.gameObject.CompareTag("Health") && myHand.healthPackAmount < 5)
        {
            Destroy(collision.gameObject);
            myHand.healthPackAmount++;
            var newPack = Pool.Spawn(healthPackIconPrefab);
            newPack.transform.SetParent(panel.transform);
            myHand.healthPackGobjs[myHand.healthPackAmount] = newPack;
            //var healthpackCol = collision.gameObject.GetComponent<Collider2D>();
            //var healthpackRb = collision.gameObject.GetComponent<Rigidbody2D>();
            //healthpackCol.enabled = false;
            //healthpackRb.simulated = false;
            //healthpackRb.velocity = Vector2.zero;
        }

    }

    bool landed;

    void GetKeys()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (myHand.healthPackAmount > 0 && myStats.health < startingHealth)
            {
                Heal();
                animator.SetTrigger(landHash);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            animator.SetTrigger(jumpingHash);

            Vector3 force = new Vector3(0, jumpHeight, 0);
            rb.AddForce(force);
            canJump = false;
            //fall animation trigger

        }

        if (Input.GetKey(KeyCode.D))
        {
            if (canJump)
            {
                isWalking = true;
            }
            if (canJump && myHand.currentWeapon == null)
            {
                animator.SetBool(walkingHash, true);
                animator.SetBool(walkingWithGunHash, false);
            }
            else if (canJump && myHand.currentWeapon != null)
            {
                animator.SetBool(walkingHash, false);
                animator.SetBool(walkingWithGunHash, true);
                holdingPlace.transform.localPosition = new Vector2(.03f * (sr.flipX ? -1 : 1), 0);
                holdingPlace.transform.rotation = Quaternion.identity;
            }
            else if (!canJump && myHand.currentWeapon == null)
            {
                animator.SetBool(walkingHash, false);
                animator.SetBool(walkingWithGunHash, false);
                //myHand.current.GetComponent<SpriteRenderer>().flipX = true;
            }
            else if (!canJump && myHand.currentWeapon != null)
            {
                animator.SetBool(walkingHash, false);
                animator.SetBool(walkingWithGunHash, true);
                holdingPlace.transform.localPosition = new Vector2(.03f * (sr.flipX ? -1 : 1), 0);
                holdingPlace.transform.rotation = Quaternion.identity;
            }

            sr.flipX = false;
            //rb.velocity = new Vector2(speed * 80, rb.velocity.y);
            transform.Translate(speed, 0, 0);
        }

        else if (Input.GetKey(KeyCode.A))
        {
            HealthTextFollowMe();
            if (canJump)
            {
                isWalking = true;
            }

            if (canJump && myHand.currentWeapon == null)
            {
                animator.SetBool(walkingHash, true);
                animator.SetBool(walkingWithGunHash, false);
            }
            else if (canJump && myHand.currentWeapon != null)
            {
                animator.SetBool(walkingHash, false);
                animator.SetBool(walkingWithGunHash, true);
                holdingPlace.transform.localPosition = new Vector2(.03f * (sr.flipX ? -1 : 1), 0);
                holdingPlace.transform.rotation = Quaternion.identity;
            }
            else if (!canJump && myHand.currentWeapon == null)
            {
                animator.SetBool(walkingHash, false);
                animator.SetBool(walkingWithGunHash, false);
                //myHand.current.GetComponent<SpriteRenderer>().flipX = true;
            }
            else if (!canJump && myHand.currentWeapon != null)
            {
                animator.SetBool(walkingHash, false);
                animator.SetBool(walkingWithGunHash, true);
                holdingPlace.transform.localPosition = new Vector2(.03f * (sr.flipX ? -1 : 1), 0);
                holdingPlace.transform.rotation = Quaternion.identity;
            }

            sr.flipX = true;
            //rb.velocity = new Vector2(-speed * 80, rb.velocity.y);
            transform.Translate(-speed, 0, 0);
        }
        else
        {
            //idle
            isWalking = false;

            animator.SetBool(walkingHash, false);
            animator.SetBool(walkingWithGunHash, false);

            holdingPlace.transform.localPosition = new Vector2(0, -.06f);
            holdingPlace.transform.rotation = Quaternion.Euler(0, 0, 90);
        }


        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (myHand.currentWeapon != null)
            {
                var oldGun = myHand.currentWeapon.GetComponent<PickupAble>();
                if (oldGun)
                {
                    oldGun.Drop(rb.velocity + new Vector2(THROW_VELOCITY.x * (sr.flipX ? -1 : 1), THROW_VELOCITY.y) * 1.5f, SPIN * (sr.flipX ? 1 : -1), .35f);
                }

                myStats.atkDmg = 0;

                AtkDamageTxt.text = "Atk: " + myStats.atkDmg;

                myHand.currentWeapon = null;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (myHand.currentWeapon != null)
            {
                if (myHand.bulletAmount > 0)
                {
                    myHand.bulletAmount--;

                    if (myHand.bulletAmount == 0)
                    {
                        ammoAmountText.color = new Color(255, 0, 0);
                    }
                    
                    var newBullet = Pool.Spawn(bulletPrefab);

                    var bulletScript = newBullet.GetComponent<Bullet>();
                    //change according to gun
                    bulletScript.lifeSpan = 1;
                    bulletScript.dmg = myStats.atkDmg;

                    newBullet.transform.position = new Vector3(holdingPlace.transform.position.x + (sr.flipX ? -.05f : .05f), holdingPlace.transform.position.y + (isWalking ? .05f : .5f));
                    Vector2 force = new Vector2(1000 * (sr.flipX ? -1 : 1), 10);
                    newBullet.GetComponent<Rigidbody2D>().AddForce(force);
                    shot = true;

                    if (ammoAmountText)
                    {
                        ammoAmountText.text = "Ammo: " + myHand.bulletAmount;
                    }
                }
                else
                {
                    //you have no more ammo!
                }
            }
        }
    }

    void HealthTextFollowMe()
    {
        myHealthText.transform.position = transform.position + new Vector3(.1f, 1.3f, 0);
    }

    void UpdateHealthText()
    {
        myHealthText.text = myStats.health + "/" + startingHealth;
    }

    void ChangeTextColor(TMP_Text text, Color col)
    {
        text.color = new Color(col.r, col.b, col.g);
    }
}
