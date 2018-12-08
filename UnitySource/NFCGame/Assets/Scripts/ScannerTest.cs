using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ScannerTest : MonoBehaviour
{
    Text outputTxt;
    InputField input;
    NFCSpoofer spoofer;

    void Start()
    {
        outputTxt = GameObject.Find("OutputText").GetComponent<Text>();
        input = GameObject.Find("InputField").GetComponent<InputField>();
        spoofer = GameObject.Find("Manager").GetComponent<NFCSpoofer>();
    }

    public void SendText()
    {
        spoofer.dataToSend = input.text;
        spoofer.SpoofNFCData();
    }

    public void OnDataRecievedHandler(object sender, string e)
    {
        outputTxt.text = e;
    }

    public void OnToggle(bool value)
    {
        if (value)
        {
            AppManager.INSTANCE.scannerManager.SerialConnectionenabled = true;
            AppManager.INSTANCE.OnDataRecieved += OnDataRecievedHandler;
        }
        else
        {
            AppManager.INSTANCE.scannerManager.SerialConnectionenabled = false;
            AppManager.INSTANCE.OnDataRecieved -= OnDataRecievedHandler;
        }
    }
}
