using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    public static AudioManager SharedInstance;    

    void Awake()
    {
        SharedInstance = this;
    }

    public void TestLog()
    {
        Debug.Log("Test from AudioManager");
    }
}
