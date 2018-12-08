using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NativeUart : ISerialController
{
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

    const int watchDogTimeOut = 5;

    public event DeviceConnectionStatusChangedHandler OnDeviceConnectionStatusChanged;
    public event UartDataReceivedEventHandler OnUartMessageRead;
    public event UartDataReceivedEventHandler OnUartMessageReadLine;

    string readData = null;
    int watchDogTimer = 0;
    int maxTimer;

	#if UNITY_ANDROID
	AndroidJavaClass nu;
	AndroidJavaObject context;
	AndroidJavaClass unityPlayer;
	#endif

	NativeUart(){
		Debug.Log("Create NativeUart instance.");

#if UNITY_ANDROID
		nu = new AndroidJavaClass ("jp.co.satoshi.uart_plugin.NativeUart");
		unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
		context  = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
#endif			
	}

    public void Connect()
    {
        throw new System.NotImplementedException();
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
        throw new System.NotImplementedException();
    }

    public void DiscardRecievedQueue()
    {
        throw new System.NotImplementedException();
    }

    public void DiscardToSendQueue()
    {
        throw new System.NotImplementedException();
    }

    public void Dispose()
    {
#if UNITY_ANDROID
		nu.CallStatic ("disconnect");
#endif
	}

	public void Init(){
#if UNITY_ANDROID
		context.Call ("runOnUiThread", new AndroidJavaRunnable(() => {
			nu.CallStatic("initialize", context);
		}));
#endif
	}


	public void Connection(int boud){
#if UNITY_ANDROID
		context.Call ("runOnUiThread", new AndroidJavaRunnable(() => {
			nu.CallStatic ("connection", boud);
		}));

        //start up watchdog thread
        //if this thread counts to zero android reader thread should be declared disconnected
#endif
	}

	public void Send(string msg){
#if UNITY_ANDROID
		nu.CallStatic ("send", msg);
#endif
	}

	public void UartCallbackState(string msg){
		//OnUartState(msg);
	}
	public void UartCallbackDeviceList(string msg){
		//OnUartDeviceList(msg);
	}
	public void UartMessageReceived(string msg){

		readData = readData + msg;
		if (msg.IndexOf ('\n') > -1) {
			OnUartMessageReadLine(readData);
			readData = null;
		} 
		OnUartMessageRead (msg);
	}

    public void UpdateWatchDog(string msg)
    {
        //reset watchdog timer
    }
}
