using System;
using System.Threading;
using System.IO;
using System.IO.Ports;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScannerManager
{
    //constants
    private string establishedConnectionKey = "setup";
    private string waitForConnectionKey = "setConnection";

    string portname;
    int baudRate = 9600;
    int timeOutRate = 5000;
    SerialPort serialPort;
    private Thread readingThread;
    private bool connected;
    private bool readData;

    public event OnScanEventHandler OnScanRecievedEvent;
    public delegate void OnScanEventHandler(string Json);

    public ScannerManager()
    {
        connected = false;
        Thread setupThread = new Thread(FindPort);
        setupThread.Start();
        serialPort = new SerialPort();
        //serialPort.PortName = portname;
        //serialPort.BaudRate = baudRate;
        //serialPort.Open();
    }

    private void OnDataRecieved(string Json)
    {
        //convert data and call event here
        OnScanRecievedEvent(Json);
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

    private void FindPort()
    {
        while (!connected)
        {
            Debug.Log("attempting conenction...");

            string[] ports = SerialPort.GetPortNames();

            if (ports.Length < 1)
            {
                Debug.LogWarning("no open ports were found, make sure the arduino is plugged in");
                Thread.Sleep(1000);
                continue;
            }

            foreach (string port in ports)
            {
                try
                {
                    using (SerialPort stream = new SerialPort(port, baudRate))
                    {
                        stream.ReadTimeout = timeOutRate;
                        stream.Open();

                        string data = stream.ReadLine();
                        if (data == waitForConnectionKey)
                        {
                            //correct port found;
                            Debug.Log("the correct port was found");
                            stream.WriteLine(establishedConnectionKey);
                            return;
                        }
                    }
                }
                catch(IOException e)
                {
                    Debug.LogWarning("The arduino was unplugged/already in use while trying to establish a connection");
                }
                catch (TimeoutException e)
                {
                    Debug.LogWarning(e);
                }
                
            }

            Debug.LogWarning("the arduino could not be found");
        }
    }

    private void ThreadLoop()
    {
        //serial stuff goes here;
        while (readData)
        {
            string data;
            try
            {
                data = serialPort.ReadLine();
            }
            catch (TimeoutException)
            {
                data = null;
            }

            if (data != null)
            {
                OnDataRecieved(data);
            }
        }
    }
}
