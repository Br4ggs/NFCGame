using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NFCSpoofer : MonoBehaviour
{
    public string dataToSend;

    public void SpoofNFCData()
    {
        AppManager.INSTANCE.scannerManager.TriggerOnDataRecieved(dataToSend);
    }
}
