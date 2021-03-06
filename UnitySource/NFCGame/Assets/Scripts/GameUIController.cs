﻿using System;
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
        Array.ForEach(characterUIControllers, controller => controller.RemoveEffects());
        for(int i = 0; i < AppManager.INSTANCE.characterData.Count; i++)
        {
            PlayerData data = AppManager.INSTANCE.characterData[i];
            characterUIControllers[i].UpdateUI(data);
        }
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
        popupMessage.ShowDialog(msg, PopupCallBack);
        DialogUp = true;
    }

    public void DisplayChoiceBox(bool targetsMultiple, int[] targetPlayers, string name, string desc)
    {
        choiceDialog.ActivateDialogBox(targetsMultiple, targetPlayers, name, desc, PopupCallBack);
        DialogUp = true;
    }

    public void PopupCallBack()
    {
        DialogUp = false;
    }

    public int[] GetChoiceBoxResult()
    {
        return choiceDialog.affectedPlayers.ToArray();
    }

    public void SetRound(int round)
    {
        roundText.text = round.ToString("D2");
    }

    public void ShowVarChanges(List<VariableChange> changes)
    {
        StartCoroutine(ShowVarChangeCoRoutine(changes, 0.2f));
    }

    private IEnumerator ShowVarChangeCoRoutine(List<VariableChange> changes, float waitPeriod)
    {
        foreach (VariableChange change in changes)
        {
            //update ui profile
            PlayerData data = AppManager.INSTANCE.characterData[change.player];
            //apply visual effect
            characterUIControllers[change.player].ShowVarChange(change.variable, change.change, data);

            yield return new WaitForSeconds(waitPeriod);
        }

        UpdatePlayerUI();
    }

    public void UpdateStatusEffects(List<VariableChange> statusEffects)
    {
        Array.ForEach(characterUIControllers, controller => controller.RemoveEffects());

        foreach(VariableChange effect in statusEffects)
        {
            characterUIControllers[effect.player].RegisterEffect(effect);
        }
    }

    public void UpdatePlayerUI()
    {
        for (int i = 0; i < AppManager.INSTANCE.characterData.Count; i++)
        {
            PlayerData data = AppManager.INSTANCE.characterData[i];
            characterUIControllers[i].UpdateUI(data);
        }
    }
}
