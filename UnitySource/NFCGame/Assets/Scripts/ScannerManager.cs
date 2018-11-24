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
    string establishedConnectionKey = "setup";
    string waitForConnectionKey = "setConnection";

    string portname;
    int baudRate = 9600;
    int timeOutRate = 3000;
    SerialPort serialPort;


    //searching and reading and (re)searching can be efficiently done on one thread instead of 2!
    private Thread readingThread;
    private Thread searchingThread;

    ConnectionState state;
    bool readData;

    public ScannerManager()
    {
        readData = false;
        state = ConnectionState.SEARCHING;
        searchingThread = new Thread(FindPort);
        searchingThread.Start();
    }

    public void StartReading()
    {
        readData = true;
        readingThread = new Thread(ThreadLoop);
        readingThread.Start();
    }

    public void StopReading()
    {
        readData = false;
    }

    private void OnDataRecieved(string Json)
    {
        //convert data and call event here
        Debug.Log("DATA WAS RECIEVED FROM SERIAL");
        Debug.Log(Json);
    }

    private void OnConnectionEstablished()
    {
        readData = true;
        readingThread = new Thread(ThreadLoop);
        readingThread.Start();
    }

    private void FindPort()
    {
        while (state == ConnectionState.SEARCHING)
        {
            string[] ports = SerialPort.GetPortNames();
            Debug.Log(ports.Length);

            if (ports.Length < 1)
            {
                Debug.LogWarning("no open ports were found, make sure the arduino is plugged in");
                Thread.Sleep(1000);
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
                        stream.WriteLine(establishedConnectionKey);
                        portname = port;
                        serialPort = stream;
                        state = ConnectionState.CONNECTED;
                        OnConnectionEstablished();
                        Debug.Log("the correct port was found");
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
            }

            Debug.LogWarning("the arduino could not be found, retrying...");
        }
    }

    private void ThreadLoop()
    {
        //check if serial port has been defined
        //if not call FindPort()

        while (state == ConnectionState.CONNECTED && readData)
        {
            string data = null;
            try
            {
                //maybe its better to put this data in a queue
                data = serialPort.ReadLine();

                //in case arduino gets accidentally reset on use
                if(data == waitForConnectionKey)
                {
                    Debug.Log("resetti");
                    serialPort.WriteLine(establishedConnectionKey);
                }

                OnDataRecieved(data);
            }
            catch (TimeoutException e)
            {
                Debug.LogWarning(e);
            }
            catch (IOException e)
            {
                if(state != ConnectionState.DISPOSING)
                {
                    Debug.LogWarning(e);
                    //something went wrong with the port, like an unplug
                    //try to refind port;
                }
            }
        }
    }

    public void Dispose()
    {
        Debug.Log("disposing");

        state = ConnectionState.DISPOSING;
        readData = false;

        if(readingThread != null)
            readingThread.Abort();
        
        if(searchingThread != null)
            searchingThread.Abort();

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
