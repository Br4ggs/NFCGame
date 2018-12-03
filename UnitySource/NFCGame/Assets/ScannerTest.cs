using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ScannerTest : MonoBehaviour
{
    public void OnDataRecievedHandler(object sender, JObject e)
    {
        Debug.Log("data was recieved");
    }

    public void OnToggle(bool value)
    {
        if (value)
        {
            AppManager.INSTANCE.scannerManager.Active = true;
            AppManager.INSTANCE.OnValidJsonRecieved += OnDataRecievedHandler;
        }
        else
        {
            AppManager.INSTANCE.scannerManager.Active = false;
            AppManager.INSTANCE.OnValidJsonRecieved -= OnDataRecievedHandler;
        }
    }
}
