using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OurHome : MonoBehaviour {

    public void Send() {
        Application.OpenURL("https://app.ourhomeapp.com/#auth/login");
    }
}
