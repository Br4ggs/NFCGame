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

    private ConnectionState state;

    public event OnDataRecievedHandler OnDataRecieved;
    public delegate void OnDataRecievedHandler(string data);

    public ScannerManager()
    {
        readingThread = new Thread(ThreadLoop);
        readingThread.Start();
    }

    /// <summary>
    /// Debug Method for spoofing incoming NFC data
    /// </summary>
    /// <param name="data">the data to spoof</param>
    public void TriggerOnDataRecieved(string data)
    {
        //can be done on separate thread
        lock (linesToWrite)
        {
            linesToWrite.Enqueue(data);
        }
    }

    private void FindPort()
    {
        state = ConnectionState.SEARCHING;

        while (state == ConnectionState.SEARCHING)
        {
            string[] ports = SerialPort.GetPortNames();
            Debug.Log(ports.Length);

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
                        state = ConnectionState.CONNECTED;
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
        //check if serial port has been defined
        //if not call FindPort()

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
                //maybe its better to put this data in a queue
                string data = serialPort.ReadLine();
                Debug.Log(data);

                //in case arduino gets accidentally reset on use
                /*if(data == waitForConnectionKey)
                {
                    Debug.Log("resetti");
                    serialPort.WriteLine(establishedConnectionKey);
                }
                OnDataRecieved(data.ToString());*/
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

        state = ConnectionState.DISPOSING;

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
    DISPOSING
}
