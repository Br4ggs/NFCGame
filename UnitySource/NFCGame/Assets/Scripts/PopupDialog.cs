using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupDialog : MonoBehaviour
{
    public Text message;
    public Action callBack;

    public void ShowDialog(string dialog, Action callback)
    {
        gameObject.SetActive(true);
        message.text = dialog;
        callBack = callback;
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
        callBack();
    }
}
