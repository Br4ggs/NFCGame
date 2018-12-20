using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

//usefull aditions
//-separate parsing class for handling data on the cards
// usefull when implementing more advanced mechanics such as poisoning etc.

/// <summary>
/// Handles the logic of the game scene
/// uses the UI controllers to manipulate the UI depending on the games state
/// </summary>
public class GameController : MonoBehaviour
{
    public DamageDialog choiceDialog;
    public FlatCharUIController[] characterUIControllers;
    public Text roundText;

    private int currentPlayer;
    private int currentRound;
    private AbilityData currentData;
    private bool dialogUp;

    /// <summary>
    /// setup
    /// </summary>
    void Start()
    {
        Debug.Log("test");
        AppManager.INSTANCE.OnValidJsonRecieved += OnDataRecievedHandler;

        choiceDialog.DeactivateDialogBox();
        dialogUp = false;

        currentPlayer = 0;
        characterUIControllers[currentPlayer].HighLighted = true;

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
                data.lives = 3;
                data.isAlive = true;
                AppManager.INSTANCE.characterData[i] = data;

                characterUIControllers[i].UpdateUI(data);
            }
        }

        roundText.text = currentRound.ToString("D2");
        EndUpdate();
    }

    /// <summary>
    /// destructor for class which removes methods for event
    /// </summary>
    void OnDisable()
    {
        AppManager.INSTANCE.OnValidJsonRecieved -= OnDataRecievedHandler;
        AppManager.INSTANCE.characterData.Clear();
    }

    /// <summary>
    /// called when all players have had their turn and the game advances to the next round
    /// </summary>
    public void NextRound()
    {
        int numOfActivePlayers = 0;
        foreach (PlayerData player in AppManager.INSTANCE.characterData)
        {
            if (player.isAlive)
                numOfActivePlayers++;
        }

        if (numOfActivePlayers < 2)
        {
            Debug.Log("GAME IS OVER");
            AppManager.INSTANCE.SwitchScene(0);
        }

        characterUIControllers[currentPlayer].HighLighted = false;
        currentPlayer = 0;
        if (!AppManager.INSTANCE.characterData[currentPlayer].isAlive)
            NextPlayer();

        characterUIControllers[currentPlayer].HighLighted = true;

        currentRound++;
        roundText.text = currentRound.ToString("D2");

        //update player values
        foreach(PlayerData character in AppManager.INSTANCE.characterData)
        {
            if(character.isAlive)
                character.currentAbilityPoints = character.maxAbilityPoints;
        }

        //update ui
        EndUpdate();
    }

    /// <summary>
    /// called when the current player finishes their turn
    /// </summary>
    public void NextPlayer()
    {
        characterUIControllers[currentPlayer].HighLighted = false;

        if(currentPlayer + 1 >= AppManager.INSTANCE.characterData.Count)
        {
            NextRound();
        }
        else
        {
            currentPlayer++;
            if (!AppManager.INSTANCE.characterData[currentPlayer].isAlive)
            {
                NextPlayer();
                return;
            }

            characterUIControllers[currentPlayer].HighLighted = true;
            EndUpdate();
        }
    }

    /// <summary>
    /// this gets called when a player uses a card on the scanner, this is what advances the player's turn
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnDataRecievedHandler(object sender, JObject e)
    {
        Debug.Log("a card was played");
        if (dialogUp)
            return;

        if (e.GetValue("typeOf").ToString() != "Ability")
        {
            Debug.LogAssertion("incorrect card type was played");
            return;
        }

        currentData = e.ToObject<AbilityData>();
        PlayerData currentPlayerData = AppManager.INSTANCE.characterData[currentPlayer];

        if (currentPlayerData.currentAbilityPoints < currentData.pointCost)
        {
            Debug.LogWarning("you do not have enough points left to play this card");
            return;
        }

        currentPlayerData.currentAbilityPoints -= currentData.pointCost;
        
        if(currentData.heals > 0)
        {

            if (currentPlayerData.currentHealth + currentData.heals > currentPlayerData.maxHealth)
                currentPlayerData.currentHealth = currentPlayerData.maxHealth;
            else
                currentPlayerData.currentHealth += currentData.heals;
        }

        AppManager.INSTANCE.characterData[currentPlayer] = currentPlayerData;

        if (currentData.damage > 0)
        {
            List<int> targetPlayers = new List<int>();
            for(int i = 0; i < AppManager.INSTANCE.characterData.Count; i++)
            {
                if (i == currentPlayer)
                    continue;
                if (AppManager.INSTANCE.characterData[i].isAlive && AppManager.INSTANCE.characterData[i].currentHealth > 0)
                    targetPlayers.Add(i);
            }

            choiceDialog.ActivateDialogBox(currentData.canDamageMultiple, targetPlayers.ToArray());
            dialogUp = true;
            return;
        }

        //update ui of players
        EndUpdate();

        if(AppManager.INSTANCE.characterData[currentPlayer].currentAbilityPoints <= 0)
            NextPlayer();
    }

    /// <summary>
    /// called after a user played a card or when it's the next players turn
    /// </summary>
    private void EndUpdate()
    {
        for(int i = 0; i < AppManager.INSTANCE.characterData.Count; i++)
        {
            characterUIControllers[i].UpdateUI(AppManager.INSTANCE.characterData[i]);
        }

        //check if final player
    }
    
    public void CloseDamageDialog()
    {
        dialogUp = false;
        int[] affectedPlayers = choiceDialog.DeactivateDialogBox();

        for (int i = 0; i < affectedPlayers.Length; i++)
        {
            DamagePlayer(affectedPlayers[i], currentData.damage);
        }

        //update ui of players
        EndUpdate();

        if (AppManager.INSTANCE.characterData[currentPlayer].currentAbilityPoints <= 0)
            NextPlayer();
    }

    //change this to damage/heal players?
    private void DamagePlayer(int player, int damage)
    {
        PlayerData data = AppManager.INSTANCE.characterData[player];
        data.currentHealth -= damage;

        if(data.currentHealth <= 0)
        {
            AppManager.INSTANCE.characterData[currentPlayer].victoryPoints++;

            if (data.lives - 1 < 0)
                data.isAlive = false;
            else
            {
                data.lives--;
                data.currentHealth = data.maxHealth;
            }
        }

        AppManager.INSTANCE.characterData[player] = data;
    }
}

public class PlayerData
{
    public string name;
    public string description;
    public int maxHealth;
    public int currentHealth;
    public int maxAbilityPoints;
    public int currentAbilityPoints;
    public int victoryPoints;

    public int lives;
    public bool isAlive;
}

public struct AbilityData
{
    public string name;
    public string description;
    public int damage;
    public bool canDamageMultiple;
    public int heals;
    public int pointCost;
}
