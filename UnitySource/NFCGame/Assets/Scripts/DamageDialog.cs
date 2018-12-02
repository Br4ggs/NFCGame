using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageDialog : MonoBehaviour
{
    public Text dialog;
    public Toggle[] toggles;

    private bool multipleChoice;
    public List<int> affectedPlayers;

    public void ActivateDialogBox(bool multipleChoice, int[] targetPlayers)
    {
        gameObject.SetActive(true);
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
    }

    public int[] DeactivateDialogBox()
    {
        gameObject.SetActive(false);
        return affectedPlayers.ToArray();
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
}
