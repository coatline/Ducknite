using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Pixelbyte;

public class Gamemanager : MonoBehaviour
{
    public int enemyAmount;
    public Canvas worldSpaceCanvas;
    public TMP_Text healthTextPrefab;
    public GameObject enemySlime;
    public Player playerPrefab;
    //for the player
    public TMP_Text ammoTextPrefab;
    public TMP_Text ammoAmountText;
    public TMP_Text AtkDamageTxt;
    public int startingBullets;
    public int startingHealth;
    public bool autoPickup;
    public float jumpHeight;
    public GameObject bulletPrefab;
    public GameObject healthIconPrefab;
    public Canvas canvas;
    public GameObject panel;

    Player me;



    GameObject[] badGuys;

    void Awake()
    {
        var newPlayer = Pool.Spawn(playerPrefab);
        me = newPlayer;
        newPlayer.ammoAmountText = ammoAmountText;
        newPlayer.ammoTextPrefab = ammoTextPrefab;
        newPlayer.AtkDamageTxt = AtkDamageTxt;
        newPlayer.bulletPrefab = bulletPrefab;
        newPlayer.worldSpaceCanvas = worldSpaceCanvas;
        newPlayer.startingBullets = startingBullets + 0;
        newPlayer.startingHealth = startingHealth + 0;
        newPlayer.jumpHeight = 350;
        newPlayer.healthTextPrefab = healthTextPrefab;
        newPlayer.healthPackIconPrefab = healthIconPrefab;
        newPlayer.canvas = canvas;
        newPlayer.panel = panel;

        Camera.main.GetComponent<Cam>().player = me;

        badGuys = new GameObject[enemyAmount];
    }

}
