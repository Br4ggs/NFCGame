﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Unity compatible android serial controller
/// Requireds external serial AAR for usage
/// </summary>
public class NativeUart : ISerialController
{
    //constants
    private const string establishedConnectionKey = "setup";
    private const int baudRate = 9600;
    private const int watchDogTimeoutRate = 5;

    public event DeviceConnectionStatusChangedHandler OnDeviceConnectionStatusChanged;

    private Queue<string> linesToWrite = new Queue<string>();
    private Queue<string> linesRecieved = new Queue<string>();

    private string readData = null;
    private Thread watchDogThread;
    private int watchDogTimer = 0;

    private NativeUartRemote remote;

#if UNITY_ANDROID
	AndroidJavaClass nu;
	AndroidJavaObject context;
	AndroidJavaClass unityPlayer;
#endif

    private bool autoReconnect = false;
    public bool AutoReconnect
    {
        get
        {
            return autoReconnect;
        }
        set
        {
            autoReconnect = value;
        }
    }

    private ConnectionState state;
    public ConnectionState State
    {
        get
        {
            return state;
        }
        set
        {
            if (state != value)
            {
                state = value;
                OnDeviceConnectionStatusChanged(this, state);
            }
        }
    }


	public NativeUart()
    {
        GameObject instance = new GameObject();
        instance.name = "NativeUart";
        remote = instance.AddComponent<NativeUartRemote>();

        remote.OnUartCallbackState += UartCallbackState;
        remote.OnUartMessageReceived += UartMessageReceived;
        remote.OnUpdateWatchDog += UpdateWatchDog;

#if UNITY_ANDROID
		nu = new AndroidJavaClass ("jp.co.satoshi.uart_plugin.NativeUart");
		unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
		context  = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
#endif			
	}

    public void Connect()
    {
#if UNITY_ANDROID
        context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
            nu.CallStatic("initialize", context);
        }));
#endif
    }

    public void OnDeviceFound()
    {
#if UNITY_ANDROID
        context.Call("runOnUiThread", new AndroidJavaRunnable(() => {
            nu.CallStatic("connection", baudRate);
        }));
#endif
    }

    public void Disconnect()
    {
#if UNITY_ANDROID
		nu.CallStatic ("disconnect");
#endif
    }

    public void SendLine(string msg)
    {
#if UNITY_ANDROID
		nu.CallStatic ("send", msg + "\r\n");
#endif
    }

    public string ReadLine()
    {
        if (linesRecieved.Count < 1)
            return null;

        //lock (linesRecieved)
        //{
            string data = linesRecieved.Dequeue();
            return data;
        //}
    }

    public void DiscardRecievedQueue()
    {
        //lock (linesRecieved)
        //{
            linesRecieved.Clear();
        //}
    }

    public void DiscardToSendQueue()
    {
        throw new System.NotImplementedException();
    }

    public void Dispose()
    {
        State = ConnectionState.DISPOSING;
        Disconnect();
	}

    /// <summary>
    /// Called by the java side to signify what state the code is in
    /// </summary>
    /// <param name="msg"></param>
	public void UartCallbackState(string msg)
    {
        if (state == ConnectionState.DISPOSING)
            return;

        switch (msg)
        {
            case "SEARCHING":
                State = ConnectionState.SEARCHING;
                break;

            case "DEVICEFOUND":
                State = ConnectionState.DEVICEFOUND;
                OnDeviceFound();
                break;

            case "CONNECTED":
                State = ConnectionState.CONNECTED;
                SendLine(establishedConnectionKey);
                DiscardRecievedQueue();
                readData = null;
                break;

            case "DISCONNECTED":
                State = ConnectionState.DISCONNECTED;
                break;
        }
	}

    /// <summary>
    /// Called by the java side when the serial port has recieved data
    /// </summary>
    /// <param name="msg">The data that was recieved</param>
	public void UartMessageReceived(string msg)
    {
        readData = readData + msg;
        if (msg.IndexOf('\n') > -1)
        {
            //lock (linesRecieved)
            //{
                linesRecieved.Enqueue(readData);
            //}
            readData = null;
        }
	}

    public void UpdateWatchDog(string msg)
    {
        //reset watchdog timer
    }
}