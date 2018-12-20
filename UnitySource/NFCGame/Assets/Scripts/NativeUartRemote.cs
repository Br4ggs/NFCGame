using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NativeUartRemote : MonoBehaviour
{
    private bool callOnUartCallBackState;
    private string messageOnUartCallbackState;
    public event UartDataReceivedEventHandler OnUartCallbackState;

    private bool callOnUartMessageReceived;
    private string messageOnUartMessageReceived;
    public event UartDataReceivedEventHandler OnUartMessageReceived;

    private bool callOnUpdateWatchDog;
    public event UartDataReceivedEventHandler OnUpdateWatchDog;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        
    }

    public void UartCallbackState(string msg)
    {
        if (OnUartCallbackState != null)
            OnUartCallbackState(msg);
    }

    public void UartMessageReceived(string msg)
    {
        if (OnUartMessageReceived != null)
            OnUartMessageReceived(msg);
    }

    public void UpdateWatchDog(string msg)
    {
        if(OnUpdateWatchDog != null)
            OnUpdateWatchDog(msg);
    }
}
