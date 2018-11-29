using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GameController : MonoBehaviour
{
    public GameObject damageDialog;
    public FlatCharUIController[] characterUIControllers;
    public Text roundText;

    public Toggle[] damageToggle;

    private int currentPlayer;
    private int currentRound;


    AbilityData currentData;

    void Start()
    {
        AppManager.INSTANCE.scannerManager.Active = true;
        AppManager.INSTANCE.OnValidJsonRecieved += OnDataRecievedHandler;

        currentPlayer = 0;
        characterUIControllers[currentPlayer].HighLighted = true;

        damageDialog.SetActive(false);

        for(int i = 0; i < characterUIControllers.Length; i++)
        {
            bool enabled = i < AppManager.INSTANCE.characterData.Count;
            characterUIControllers[i].gameObject.SetActive(enabled);

            if (enabled)
            {
                PlayerData data = AppManager.INSTANCE.characterData[i];
                data.currentHealth = data.maxHealth;
                data.currentAbilityPoints = data.maxAbilityPoints;
                data.victoryPoints = 0;
                characterUIControllers[i].UpdateUI(data);
                AppManager.INSTANCE.characterData[i] = data;
            }
        }

        roundText.text = currentRound.ToString("D2");
    }

    private void SetupGame()
    {
        //set up game here
    }

    void OnDisable()
    {
        AppManager.INSTANCE.OnValidJsonRecieved -= OnDataRecievedHandler;
    }

    public void NextTurn()
    {
        characterUIControllers[currentPlayer].HighLighted = false;
        currentPlayer = 0;
        characterUIControllers[currentPlayer].HighLighted = true;

        currentRound++;

        roundText.text = currentRound.ToString("D2");

        int numOfActivePlayers = 0;
        foreach(PlayerData player in AppManager.INSTANCE.characterData)
        {
            if (player.currentHealth < 1)
                numOfActivePlayers++;
        }

        if (numOfActivePlayers < 2)
            Debug.Log("GAME IS OVER");
    }

    public void NextPlayer()
    {
        characterUIControllers[currentPlayer].HighLighted = false;

        if(currentPlayer + 1 >= AppManager.INSTANCE.characterData.Count)
        {
            NextTurn();
        }
        else
        {
            currentPlayer++;
            characterUIControllers[currentPlayer].HighLighted = true;
        }
    }

    void OnDataRecievedHandler(object sender, JObject e)
    {
        Debug.Log("a card was played");

        if (e.GetValue("typeOf").ToString() != "Ability")
        {
            Debug.LogAssertion("incorrect card type was played");
            return;
        }

        currentData = e.ToObject<AbilityData>();

        int currentPlayerAbilityPoints = AppManager.INSTANCE.characterData[currentPlayer].currentAbilityPoints;
        if(currentPlayerAbilityPoints < currentData.pointCost)
        {
            Debug.LogWarning("you do not have enough points left to play this card");
            return;
        }

        AppManager.INSTANCE.characterData[currentPlayer].currentAbilityPoints -= currentData.pointCost;
        
        //register other changes of card
        //update ui

        if (currentData.damage > 0)
        {
            DisplayDamageDialog();
            return;
        }

        NextPlayer();
    }

    //move to separate ui manager
    private void DisplayDamageDialog()
    {
        damageDialog.SetActive(true);

        for(int i = 0; i < damageToggle.Length; i++)
        {
            bool enabled = i < AppManager.INSTANCE.characterData.Count && i != currentPlayer;
            damageToggle[i].gameObject.SetActive(enabled);
        }
    }

    //move to separate ui manager
    public void OnToggleUpdated(int player)
    {
        if (!currentData.canDamageMultiple)
        {
            for (int i = 0; i < damageToggle.Length; i++)
            {
                if (i == player)
                    continue;
                else
                    damageToggle[i].isOn = false;
            }
        }
    }

    //move to separate ui manager
    public void CloseDamageDialog()
    {
        damageDialog.SetActive(false);

        for (int i = 0; i < damageToggle.Length; i++)
        {
            if (damageToggle[i].isOn)
            {
                AppManager.INSTANCE.characterData[i].currentHealth -= currentData.damage;
                characterUIControllers[i].UpdateUI(AppManager.INSTANCE.characterData[i]);
            }
        }

        NextPlayer();
    }
}
