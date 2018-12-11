using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class counter : MonoBehaviour
{
    int count;

	void Start ()
    {
        count = 0;
	}
	

	void Update ()
    {
        GetComponentInChildren<Text>().text = count.ToString();
        count++;
	}
}
