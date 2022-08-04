using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;

public class Bar : MonoBehaviour {

    public bool moves;
    public Image bar;
    public bool attachedToSelf;

	void Awake () {

        if (attachedToSelf)
        {
            bar.transform.position = new Vector2(transform.position.x, bar.transform.position.y);
        }

        bar.fillAmount = 1;
	}
	
	void Update () {
        if (attachedToSelf && moves)
        {
            bar.transform.position = new Vector2(transform.position.x, bar.transform.position.y);
        }
    }

    public void GotHurt(int damageDone, int currentHealth)
    {
        currentHealth -= damageDone;
        float fillmount = currentHealth / 100f;
        bar.fillAmount = fillmount;
        print(bar.fillAmount);
    }
}
