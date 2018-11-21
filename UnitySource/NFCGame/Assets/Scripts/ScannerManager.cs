using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScannerManager
{
    event OnScanEventHandler OnScanRecievedEvent;
    delegate void OnScanEventHandler();
}
