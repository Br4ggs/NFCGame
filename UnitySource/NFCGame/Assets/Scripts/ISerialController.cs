using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for platform specific serial connection controllers
/// </summary>
public interface ISerialController : IDisposable
{
    /// <summary>
    /// Should the serial controller automatically try to reconnect in case of a disconnect
    /// </summary>
    bool AutoReconnect
    {
        get;
        set;
    }

    /// <summary>
    /// Called when the status of the serial controller changes
    /// </summary>
    event DeviceConnectionStatusChangedHandler OnDeviceConnectionStatusChanged;

    /// <summary>
    /// Establish a connection with the serial device, this will be managed on a newly created thread.
    /// </summary>
    /// <param name="autoConnect">Should the serial controller try to re-initialize and auto connect if connection is lost?</param>
    void Connect();

    /// <summary>
    /// De-establish the connection with the serial device, also finishes the managing thread
    /// </summary>
    void Disconnect();

    /// <summary>
    /// Send a message ending in a newline symbol to the serial device
    /// </summary>
    void SendLine(string message);

    /// <summary>
    /// Reads a message from the serial device
    /// </summary>
    /// <returns>The message recieved from the serial device</returns>
    string ReadLine();

    void DiscardRecievedQueue();

    void DiscardToSendQueue();
}

