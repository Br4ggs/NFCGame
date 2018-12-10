using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.IO.Ports;
using UnityEngine;

/// <summary>
/// Unity-compatible serial controller for windows devices
/// Currently only usable with arduino with proper software
/// </summary>
public class WindowsSerialController : ISerialController
{
    //constants
    private const string establishedConnectionKey = "setup";
    private const string waitForConnectionKey = "setConnection";
    private const int baudRate = 9600;
    private const int timeOutRate = 1000;

    public event DeviceConnectionStatusChangedHandler OnDeviceConnectionStatusChanged;

    private Queue<string> linesToWrite = new Queue<string>();
    private Queue<string> linesRecieved = new Queue<string>();

    private Thread managerThread;
    private SerialPort serialPort;

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

    private ConnectionState state = ConnectionState.DISCONNECTED;
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

    public void Connect()
    {
        managerThread = new Thread(ThreadLoop);
        managerThread.Start();
    }

    public void Disconnect()
    {
        State = ConnectionState.DISCONNECTED;
    }

    public void SendLine(string message)
    {
        lock (linesToWrite)
        {
            linesToWrite.Enqueue(message);
        }
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
        lock (linesToWrite)
        {
            linesToWrite.Clear();
        }
    }

    public void Dispose()
    {
        Debug.Log("disposing windowsSerialController");
        State = ConnectionState.DISPOSING;
        if(managerThread != null)
            managerThread.Abort();

        if (serialPort != null)
            serialPort.Dispose();
    }

    private void FindPort()
    {
        State = ConnectionState.SEARCHING;

        while (state == ConnectionState.SEARCHING)
        {
            string[] ports = SerialPort.GetPortNames();

            if (ports.Length < 1)
            {
                Debug.LogWarning("no open ports were found, make sure the arduino is plugged in");
                Thread.Sleep(2000);
                continue;
            }

            foreach (string port in ports)
            {
                Debug.Log("attempting connection...");
                SerialPort stream = new SerialPort();
                stream.ReadTimeout = timeOutRate;
                stream.BaudRate = baudRate;

                try
                {
                    stream.PortName = port;
                    stream.Open();

                    string data = stream.ReadLine();
                    if (data == waitForConnectionKey)
                    {
                        Debug.Log("the correct port was found");
                        stream.WriteLine(establishedConnectionKey);
                        State = ConnectionState.CONNECTED;
                        serialPort = stream;
                        return;
                    }
                }
                catch (IOException)
                {
                    Debug.LogWarning("The arduino was unplugged/already in use while trying to establish a connection");
                    stream.Dispose();
                }
                catch (TimeoutException e)
                {
                    Debug.LogWarning(e);
                    stream.Dispose();
                }
                catch (Exception e)
                {
                    Debug.LogAssertion(e);
                }
            }

            Debug.LogWarning("the arduino could not be found, retrying...");
        }
    }

    private void ThreadLoop()
    {
        //get a port if it is not yet instantiated
        if (serialPort == null)
            FindPort();

        //discards the buffer and is also a good way to check if the serial port is still valid
        try
        {
            serialPort.DiscardInBuffer();
        }
        catch(IOException e)
        {
            FindPort();
        }

        State = ConnectionState.CONNECTED;
        while (state == ConnectionState.CONNECTED)
        {
            try
            {
                while (linesToWrite.Count > 0)
                {
                    lock (linesToWrite)
                    {
                        string message = linesToWrite.Dequeue();
                        serialPort.WriteLine(message);
                    }
                }

                //cannot use bytestoread so we're just going to have to expect an exception each time
                string data = serialPort.ReadLine();
                Debug.Log("data is: " + data);

                //in case arduino gets accidentally reset on use
                if (data == waitForConnectionKey)
                {
                    Debug.Log("resetti");
                    serialPort.WriteLine(establishedConnectionKey);
                    continue;
                }

                lock (linesRecieved)
                {
                    linesRecieved.Enqueue(data);
                }
            }
            catch (TimeoutException)
            {
                //silently catch the error and do nothing
            }
            catch (IOException e)
            {
                if (state != ConnectionState.DISPOSING)
                {
                    if (autoReconnect && state != ConnectionState.DISCONNECTED)
                    {
                        //something went wrong with the port, like an unplug
                        //try to refind port
                        Debug.LogWarning(e);
                        FindPort();
                    }
                    else
                    {
                        State = ConnectionState.DISCONNECTED;
                        return;
                    }

                }
            }
            catch (Exception e)
            {
                Debug.LogAssertion(e);
            }
        }
    }
}
