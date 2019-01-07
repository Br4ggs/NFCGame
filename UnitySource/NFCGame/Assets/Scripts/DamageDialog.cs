using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageDialog : MonoBehaviour
{
    public Text abilityName;
    public Text abilityDescription;
    public Toggle[] toggles;

    private bool multipleChoice;
    private Action callBack;
    public List<int> affectedPlayers { get; private set; }

    public void ActivateDialogBox(bool multipleChoice, int[] targetPlayers, string name, string description, Action callback)
    {
        affectedPlayers = new List<int>();
        affectedPlayers.Clear();
        gameObject.SetActive(true);

        abilityName.text = name;
        abilityDescription.text = description;

        this.multipleChoice = multipleChoice;

        for(int i = 0; i < toggles.Length; i++)
        {
            toggles[i].isOn = false;

            if(Array.Exists(targetPlayers, j => j == i))
            {
                toggles[i].gameObject.SetActive(true);
            }
            else
            {
                toggles[i].gameObject.SetActive(false);
            }
        }

        callBack = callback;
    }

    public void OnToggleUpdate(int player)
    {
        if (!toggles[player].isOn)
        {
            affectedPlayers.Remove(player);
            return;
        }

        if (!multipleChoice)
        {
            for(int i = 0; i < affectedPlayers.Count; i++)
            {
                toggles[affectedPlayers[i]].isOn = false;
            }
            affectedPlayers.Clear();
        }
        affectedPlayers.Add(player);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);

        if(callBack != null)
            callBack();
    }
}
