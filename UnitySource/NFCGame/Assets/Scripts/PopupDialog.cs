using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupDialog : MonoBehaviour
{
    public Text message;

    public void ShowDialog(string dialog)
    {
        gameObject.SetActive(true);
        message.text = dialog;
    }
}
