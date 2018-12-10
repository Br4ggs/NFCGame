using System;
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
    private const string waitForConnectionKey = "setConnection";
    private const int baudRate = 9600;
    private const int watchDogTimeoutRate = 5;

    public event DeviceConnectionStatusChangedHandler OnDeviceConnectionStatusChanged;

    private Queue<string> linesToWrite = new Queue<string>();
    private Queue<string> linesRecieved = new Queue<string>();

    private Thread watchDogThread;
    int watchDogTimer = 0;

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


	NativeUart()
    {
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

        lock (linesRecieved)
        {
            string data = linesRecieved.Dequeue();
            return data;
        }
    }

    public void DiscardRecievedQueue()
    {
        lock (linesRecieved)
        {
            linesRecieved.Clear();
        }
    }

    public void DiscardToSendQueue()
    {
        throw new System.NotImplementedException();
    }

    public void Dispose()
    {
        Disconnect();
	}

	/*public void Init(){
#if UNITY_ANDROID
		context.Call ("runOnUiThread", new AndroidJavaRunnable(() => {
			nu.CallStatic("initialize", context);
		}));
#endif
	}*/


	/*public void Connection(int boud){
#if UNITY_ANDROID
		context.Call ("runOnUiThread", new AndroidJavaRunnable(() => {
			nu.CallStatic ("connection", boud);
		}));

        //start up watchdog thread
        //if this thread counts to zero android reader thread should be declared disconnected
#endif
	}*/

	/*public void Send(string msg){
#if UNITY_ANDROID
		nu.CallStatic ("send", msg);
#endif
	}*/

	public void UartCallbackState(string msg)
    {
		//OnUartState(msg);

        //set connectionstate enum
        
	}

	public void UartCallbackDeviceList(string msg)
    {
		//OnUartDeviceList(msg);

        //set connectionstate enum
	}

	public void UartMessageReceived(string msg)
    {
        lock (linesRecieved)
        {
            linesRecieved.Enqueue(msg);
        }
        /*readData = readData + msg;
		if (msg.IndexOf ('\n') > -1) {
			OnUartMessageReadLine(readData);
			readData = null;
		} 
		OnUartMessageRead (msg);*/

        //add messages to queue

	}

    public void UpdateWatchDog(string msg)
    {
        //reset watchdog timer
    }
}
