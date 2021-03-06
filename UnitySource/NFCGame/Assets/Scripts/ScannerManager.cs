﻿using System;
using System.Threading;
using System.IO;
using System.IO.Ports;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Wrapper-class which takes care of serial communication for different platforms
/// </summary>
public class ScannerManager : IDisposable
{
    /// <summary>
    /// Event that gets triggered when the serialController changes state
    /// NOTE: since this event is often called from the reading thread, IT IS NOT THREAD SAFE!
    /// callback methods that subscribe to this should folow a dispatcher pattern.
    /// </summary>
    public event DeviceConnectionStatusChangedHandler OnConnectionStateChanged;

    private ISerialController serialController;

    private ConnectionState state = ConnectionState.DISCONNECTED;
    public ConnectionState State
    {
        get
        {
            return state;
        }
        set
        {
            if(state != value)
            {
                state = value;
                OnConnectionStateChanged(this, state);
            }
        }
    }

    private bool serialConnectionEnabled = false;
    public bool SerialConnectionenabled
    {
        get
        {
            return serialConnectionEnabled;
        }
        set
        {
            if (serialConnectionEnabled != value)
            {
                serialConnectionEnabled = value;

                if(serialConnectionEnabled)
                    serialController.Connect();

                else
                    serialController.Disconnect();
            }
        }
    }

    public ScannerManager()
    {

#if UNITY_ANDROID
        serialController = new NativeUart();
#endif

#if UNITY_STANDALONE_WIN
        serialController = new WindowsSerialController();
#endif
        serialController.OnDeviceConnectionStatusChanged += OnDeviceStatusChangedHandler;
        serialController.AutoReconnect = true;
        SerialConnectionenabled = true;
    }

    /// <summary>
    /// Enqueues a string of data to be send as one line via serial
    /// </summary>
    /// <param name="data">The data to send</param>
    public void WriteLine(string data)
    {
        serialController.SendLine(data);
    }

    /// <summary>
    /// Reads and dequeues a line of data that was recieved via serial
    /// </summary>
    /// <returns>the recieved line of data, returns null if the queue is empty</returns>
    public string ReadLine()
    {
        return serialController.ReadLine();
    }

    public void Dispose()
    {
        Debug.Log("disposing");
        serialController.Dispose();
    }

    private void OnDeviceStatusChangedHandler(object sender, ConnectionState newState)
    {
        State = newState;
    }
}

public enum ConnectionState
{
    SEARCHING,
    DEVICEFOUND,
    CONNECTED,
    DISCONNECTED,
    DISPOSING
}

public delegate void DeviceConnectionStatusChangedHandler(object sender, ConnectionState newState);
public delegate void UartDataReceivedEventHandler(string message);