using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    public DamageDialog choiceDialog;
    public PopupDialog popupMessage;
    public Text roundText;
    public FlatCharUIController[] characterUIControllers;

    public bool DialogUp { get; private set; }

    void Start ()
    {
        choiceDialog.Deactivate();
        popupMessage.gameObject.SetActive(false);
        DialogUp = false;
    }

    public void StartGame()
    {
        roundText.text = 0.ToString("D2");
    }

    public void SetHighLightedPlayer(int player)
    {
        Array.ForEach(characterUIControllers, controller => controller.HighLighted = false);
        characterUIControllers[player].HighLighted = true;
    }

    public void GenerateUIProfiles()
    {
        for (int i = 0; i < characterUIControllers.Length; i++)
        {
            bool enabled = i < AppManager.INSTANCE.characterData.Count;
            characterUIControllers[i].gameObject.SetActive(enabled);
        }
    }

    public void DisplayMessageBox(string msg)
    {
        popupMessage.ShowDialog(msg);
        DialogUp = true;
    }

    public void DisplayChoiceBox(bool targetsMultiple, int[] targetPlayers, string name, string desc)
    {
        choiceDialog.ActivateDialogBox(targetsMultiple, targetPlayers, name, desc, ChoiceBoxCallBack);
        DialogUp = true;
    }

    public void ChoiceBoxCallBack()
    {
        DialogUp = false;
    }

    public int[] GetChoiceBoxResult()
    {
        return choiceDialog.affectedPlayers.ToArray();
    }
}
