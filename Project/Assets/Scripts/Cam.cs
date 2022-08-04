using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam : MonoBehaviour {

    public Player player;

	void LateUpdate () {
        transform.position = player.transform.position + new Vector3(0,0, -10);
	}
}
