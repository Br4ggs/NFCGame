using System;
using System.Threading;
using System.IO;
using System.IO.Ports;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScannerManager : IDisposable
{
    //constants
    const string establishedConnectionKey = "setup";
    const string waitForConnectionKey = "setConnection";

    private string portname;
    private int baudRate = 9600;
    private int timeOutRate = 1000;

    //main thread for writing/reading data to serialport
    private Thread readingThread;
    private SerialPort serialPort;
    private Queue<string> linesToWrite = new Queue<string>();
    private Queue<string> linesRecieved = new Queue<string>();

    //create property for this
    //and create event for when this changes
    public delegate void OnConnectionStateChangedHandler(object sender, ConnectionState newState);
    public event OnConnectionStateChangedHandler OnConnectionStateChanged;

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

    private bool active = false;
    public bool Active
    {
        get
        {
            return active;
        }
        set
        {
            active = value;

            if (active)
            {
                if (serialPort == null)
                {
                    state = ConnectionState.CONNECTED;
                    readingThread = new Thread(ThreadLoop);
                    readingThread.Start();
                }
            }
            else
            {
                state = ConnectionState.DISCONNECTED;
                serialPort = null;
                DiscardRecievedQueue();
            }
        }
    }

    public ScannerManager()
    {
        //readingThread = new Thread(ThreadLoop);
        //readingThread.Start();
    }

    /// <summary>
    /// Enqueues a string of data to be send as one line via serial
    /// </summary>
    /// <param name="data">The data to send</param>
    public void WriteLine(string data)
    {
        lock (linesToWrite)
        {
            linesToWrite.Enqueue(data);
        }
    }

    /// <summary>
    /// Reads and dequeues a line of data that was recieved via serial
    /// </summary>
    /// <returns>the recieved line of data, returns null if the queue is empty</returns>
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

    public bool DataInRecievedQueue()
    {
        return (linesRecieved.Count > 0);
    }

    public void DiscardRecievedQueue()
    {
        lock (linesRecieved)
        {
            linesRecieved.Clear();
        }
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
                        portname = port;
                        serialPort = stream;
                        return;
                    }
                }
                catch(IOException)
                {
                    Debug.LogWarning("The arduino was unplugged/already in use while trying to establish a connection");
                    stream.Dispose();
                }
                catch (TimeoutException e)
                {
                    Debug.LogWarning(e);
                    stream.Dispose();
                }      
                catch(Exception e)
                {
                    Debug.LogAssertion(e);
                }
            }

            Debug.LogWarning("the arduino could not be found, retrying...");
        }
    }

    private void ThreadLoop()
    {
        if (serialPort == null)
            FindPort();

        while (state == ConnectionState.CONNECTED)
        {
            try
            {
                while(linesToWrite.Count > 0)
                {
                    lock (linesToWrite)
                    {

                        string message = linesToWrite.Dequeue();
                        serialPort.WriteLine(message);
                    }
                }

                //cannot use bytestoread so we're just gonna have to expect an exception each time
                string data = serialPort.ReadLine();

                //in case arduino gets accidentally reset on use
                if(data == waitForConnectionKey)
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
                if(state != ConnectionState.DISPOSING)
                {
                    //something went wrong with the port, like an unplug
                    //try to refind port
                    Debug.LogWarning(e);
                    FindPort();
                }
            }
            catch(Exception e)
            {
                Debug.LogAssertion(e);
            }
        }
    }

    public void Dispose()
    {
        Debug.Log("disposing");

        State = ConnectionState.DISPOSING;

        if(readingThread != null)
            readingThread.Abort();

        if(serialPort != null)
            serialPort.Dispose();
    }
}

public enum ConnectionState
{
    SEARCHING,
    CONNECTED,
    DISCONNECTED,
    DISPOSING
}
