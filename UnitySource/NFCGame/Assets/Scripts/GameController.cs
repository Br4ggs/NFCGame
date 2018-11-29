using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GameController : MonoBehaviour
{
    public GameObject damageDialog;
    public FlatCharUIController[] characterUIControllers;

    private int currentPlayer;

    void Start()
    {
        AppManager.INSTANCE.OnValidJsonRecieved += OnDataRecievedHandler;
        currentPlayer = 0;
        damageDialog.SetActive(false);
        for(int i = 0; i < characterUIControllers.Length; i++)
        {
            bool enabled = i < AppManager.INSTANCE.characterData.Count;
            characterUIControllers[i].gameObject.SetActive(enabled);

            if (enabled)
            {
                characterUIControllers[i].setup(AppManager.INSTANCE.characterData[i]);
            }
        }
    }

    void OnDisable()
    {
        AppManager.INSTANCE.OnValidJsonRecieved -= OnDataRecievedHandler;
    }

    void NextTurn()
    {
        currentPlayer = 0;
        //check how many players are left
        //if one game is over
    }

    void NextPlayer()
    {
        currentPlayer++;
    }

    void OnDataRecievedHandler(object sender, JObject e)
    {
        Debug.Log("a card was played");

        if (e.GetValue("typeOf").ToString() != "Ability")
        {
            Debug.LogAssertion("incorrect card type was played");
            return;
        }

        AbilityData data = e.ToObject<AbilityData>();

        Debug.Log("name: " + data.name);
        Debug.Log("description: " + data.description);
        Debug.Log("damage: " + data.damage);
        Debug.Log("canDamageMultiplePeople: " + data.canDamageMultiple);
        Debug.Log("heals: " + data.heals);
        Debug.Log("pointCost: " + data.pointCost);

        int currentPlayerAbilityPoints = AppManager.INSTANCE.characterData[currentPlayer].currentAbilityPoints;
        if(currentPlayerAbilityPoints < data.pointCost)
        {
            Debug.LogWarning("you do not have enough points left to play this card");
            return;
        }

        if (data.damage > 0)
            DisplayDamageDialog(data.canDamageMultiple);
        //if card does damage display damage dialog
        //register all changes of card
        //update ui

        AppManager.INSTANCE.characterData[currentPlayer].currentAbilityPoints -= data.pointCost;
        if(AppManager.INSTANCE.characterData.Count - 1 > currentPlayer)
        {
            NextPlayer();
        }
        else
        {
            NextTurn();
        }
    }

    private void DisplayDamageDialog(bool canDamageMultipleTargets)
    {
        damageDialog.SetActive(true);

        foreach(Transform child in damageDialog.transform)
        {
            if (child.name == currentPlayer.ToString())
                child.gameObject.SetActive(false);

            else
                child.gameObject.SetActive(true);
        }
    }

    public void CloseDamageDialog()
    {

    }
}
