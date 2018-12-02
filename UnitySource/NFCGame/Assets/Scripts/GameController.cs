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

    /// <summary>
    /// setup
    /// </summary>
    void Start()
    {
        AppManager.INSTANCE.scannerManager.Active = true;
        AppManager.INSTANCE.OnValidJsonRecieved += OnDataRecievedHandler;

        choiceDialog.DeactivateDialogBox();

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

                characterUIControllers[i].UpdateUI(data);
                AppManager.INSTANCE.characterData[i] = data;
            }
        }

        roundText.text = currentRound.ToString("D2");
    }

    /// <summary>
    /// destructor for class which removes methods for event
    /// </summary>
    void OnDisable()
    {
        AppManager.INSTANCE.OnValidJsonRecieved -= OnDataRecievedHandler;
    }

    /// <summary>
    /// called when all players have had their turn and the game advances to the next round
    /// </summary>
    public void NextRound()
    {
        characterUIControllers[currentPlayer].HighLighted = false;
        currentPlayer = 0;
        characterUIControllers[currentPlayer].HighLighted = true;

        currentRound++;
        roundText.text = currentRound.ToString("D2");

        //update player values

        //update ui
        EndUpdate();

        int numOfActivePlayers = 0;
        foreach(PlayerData player in AppManager.INSTANCE.characterData)
        {
            if (player.isAlive)
                numOfActivePlayers++;
        }

        if (numOfActivePlayers < 2)
            Debug.Log("GAME IS OVER");
    }

    /// <summary>
    /// called when the current player finishes their turn
    /// </summary>
    public void NextPlayer()
    {
        //check if current player has no health left and is on their last life
        //if so mark them for death
        //else mark them knocked down

        characterUIControllers[currentPlayer].HighLighted = false;

        if(currentPlayer + 1 >= AppManager.INSTANCE.characterData.Count)
        {
            NextRound();
        }
        else
        {
            currentPlayer++;
            characterUIControllers[currentPlayer].HighLighted = true;
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

        if (e.GetValue("typeOf").ToString() != "Ability")
        {
            Debug.LogAssertion("incorrect card type was played");
            return;
        }

        currentData = e.ToObject<AbilityData>();

        if(AppManager.INSTANCE.characterData[currentPlayer].currentAbilityPoints < currentData.pointCost)
        {
            Debug.LogWarning("you do not have enough points left to play this card");
            return;
        }

        AppManager.INSTANCE.characterData[currentPlayer].currentAbilityPoints -= currentData.pointCost;

        //move to endaction
        characterUIControllers[currentPlayer].UpdateUI(AppManager.INSTANCE.characterData[currentPlayer]);
        
        //register other changes of card via parser?

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
            return;
        }

        //update ui of players
        EndUpdate();

        if(AppManager.INSTANCE.characterData[currentPlayer].currentAbilityPoints <= 0)
            NextPlayer();
    }
    
    public void CloseDamageDialog()
    {
        int[] affectedPlayers = choiceDialog.DeactivateDialogBox();

        for (int i = 0; i < affectedPlayers.Length; i++)
        {
            DamagePlayer(affectedPlayers[i], currentData.damage);

            //move to endaction
            characterUIControllers[affectedPlayers[i]].UpdateUI(AppManager.INSTANCE.characterData[affectedPlayers[i]]);
        }

        //update ui of players
        EndUpdate();

        if (AppManager.INSTANCE.characterData[currentPlayer].currentAbilityPoints <= 0)
            NextPlayer();
    }


    /// <summary>
    /// called after a user played a card or when it's the next players turn
    /// </summary>
    private void EndUpdate()
    {

    }


    private void DamagePlayer(int player, int damage)
    {
        PlayerData data = AppManager.INSTANCE.characterData[player];
        data.currentHealth -= damage;

        if(data.currentHealth <= 0)
        {
            data.lives--;
            if(data.lives <= 0)
            {
                data.isAlive = false;
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
