using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class ScannerTest : MonoBehaviour
{
    Text connectionStatusText;
    Text outputTxt;
    InputField input;
    NFCSpoofer spoofer;

    void Start()
    {
        connectionStatusText = GameObject.Find("ConnectionStatusText").GetComponent<Text>();
        outputTxt = GameObject.Find("OutputText").GetComponent<Text>();
        input = GameObject.Find("InputField").GetComponent<InputField>();
        spoofer = GameObject.Find("Manager").GetComponent<NFCSpoofer>();

        AppManager.INSTANCE.OnValidJsonRecieved += OnJsonRecievedHandler;
        AppManager.INSTANCE.OnSerialStateChanged += OnConnectionStatusChangedHandler;
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

    public void OnJsonRecievedHandler(object sender, JObject e)
    {
        outputTxt.text = e.ToString();
    }

    public void OnConnectionStatusChangedHandler(object sender, ConnectionState e)
    {
        connectionStatusText.text = e.ToString();
    }

    public void OnToggle(bool value)
    {
        if (value)
        {
            //AppManager.INSTANCE.OnDataRecieved += OnDataRecievedHandler;
            //AppManager.INSTANCE.OnValidJsonRecieved += OnJsonRecievedHandler;
            //AppManager.INSTANCE.OnSerialStateChanged += OnConnectionStatusChangedHandler;
        }
        else
        {
            //AppManager.INSTANCE.OnDataRecieved -= OnDataRecievedHandler;
            //AppManager.INSTANCE.OnValidJsonRecieved -= OnJsonRecievedHandler;
            //AppManager.INSTANCE.OnSerialStateChanged -= OnConnectionStatusChangedHandler;
        }
    }
}
