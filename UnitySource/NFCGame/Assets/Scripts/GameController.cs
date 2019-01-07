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
    public PopupDialog popupMessage;
    public FlatCharUIController[] characterUIControllers;
    public Text roundText;

    private int currentPlayer;
    private int currentRound;
    private bool dialogUp;

    public List<VariableChange> registeredEffects = new List<VariableChange>();
    private List<int> affectedPlayers = new List<int>();

    /// <summary>
    /// setup
    /// </summary>
    void Start()
    {
        AppManager.INSTANCE.OnValidJsonRecieved += OnDataRecievedHandler;

        choiceDialog.DeactivateDialogBox();
        popupMessage.gameObject.SetActive(false);
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
    }

    /// <summary>
    /// called when all players have had their turn and the game advances to the next round
    /// </summary>
    public void NextRound()
    {
        //check if game ends
        int numOfActivePlayers = 0;
        foreach (PlayerData player in AppManager.INSTANCE.characterData)
        {
            if (player.isAlive)
                numOfActivePlayers++;
        }
        if (numOfActivePlayers < 2)
        {
            Debug.Log("GAME IS OVER");
            AppManager.INSTANCE.SwitchScene(4);
        }

        characterUIControllers[currentPlayer].HighLighted = false;
        currentPlayer = 0;

        characterUIControllers[currentPlayer].HighLighted = true;

        currentRound++;
        roundText.text = currentRound.ToString("D2");

        //update player values
        //calculate player damage and ability point values

        foreach(PlayerData character in AppManager.INSTANCE.characterData)
        {
            if(character.isAlive)
                character.currentAbilityPoints = character.maxAbilityPoints;
        }
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
        }

        if (!AppManager.INSTANCE.characterData[currentPlayer].isAlive)
        {
            NextPlayer();
            return;
        }
        characterUIControllers[currentPlayer].HighLighted = true;

        ApplyRegisteredEffects(currentPlayer);
        EndUpdate();
    }

    /// <summary>
    /// this gets called when a player uses a card on the scanner, this is what advances the player's turn
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnDataRecievedHandler(object sender, JObject e)
    {
        if (dialogUp)
            return;

        if (e.GetValue("type").ToString() != "Ability")
        {
            popupMessage.ShowDialog("An incorrect card type was played.");
            dialogUp = true;
            return;
        }
        JArray fxArray = (JArray)e.GetValue("fx");
        StartCoroutine(ConvertToStructRoutine(fxArray));
    }

    /// <summary>
    /// Called after a user has played a card or when it's the next players turn
    /// </summary>
    /// <param name="changes">List of effects that were applied this update, used for UI</param>
    private void EndUpdate(List<VariableChange> changes)
    {
        //changes is the full list of applied variable changes this turn
        //like attacks or maybe debuffs for the current player
        //this is passed to the ui manager

        for(int i = 0; i < AppManager.INSTANCE.characterData.Count; i++)
        {
            characterUIControllers[i].UpdateUI(AppManager.INSTANCE.characterData[i]);
        }

        //check if final player
        //update ui
    }
    
    public void CloseDamageDialog()
    {
        dialogUp = false;
        affectedPlayers.AddRange(choiceDialog.DeactivateDialogBox());
    }

    public void ClosePopupDialog()
    {
        dialogUp = false;
        popupMessage.gameObject.SetActive(false);
    }

    private IEnumerator ConvertToStructRoutine(JArray effects)
    {
        List<VariableChange> variableChanges = new List<VariableChange>();
        foreach (JToken effect in effects)
        {
            JObject effectObj = effect.ToObject<JObject>();
            string target = effectObj.GetValue("trgts").ToString();
            JArray varChanges = (JArray)effectObj.GetValue("varchng");
            affectedPlayers.Clear();

            // get specified targets for this effect
            if (target == "user") //skip the targeting step
            {
                affectedPlayers.Add(currentPlayer);
            }
            else //allies won't be implemented for next build
            {
                string targetType = effectObj.GetValue("trgtType").ToString();
                if(targetType == "multiple" || targetType == "one")
                {
                    List<int> targetPlayers = new List<int>();
                    for (int i = 0; i < AppManager.INSTANCE.characterData.Count; i++)
                    {
                        if (i == currentPlayer)
                            continue;
                        if (AppManager.INSTANCE.characterData[i].isAlive && AppManager.INSTANCE.characterData[i].currentHealth > 0)
                            targetPlayers.Add(i);
                    }

                    choiceDialog.ActivateDialogBox((targetType == "multiple"), targetPlayers.ToArray(), "test", "this is a test");
                    dialogUp = true;

                    while (dialogUp)
                    {
                        Debug.Log("waiting...");
                        yield return null;
                    }
                    Debug.Log("moving on...");
                }
                else
                {
                    affectedPlayers.AddRange(new int[] { 0, 1, 2, 3 });
                    affectedPlayers.Remove(currentPlayer);
                }
            }

            // parse variable changes to more manageable struct form
            foreach (JToken varChange in varChanges)
            {
                JObject varChangeObj = varChange.ToObject<JObject>();
                VariableChange changeData = new VariableChange();

                changeData.additive = (varChangeObj.GetValue("type").ToString() == "additive");
                changeData.change = int.Parse(varChangeObj.GetValue("chng").ToString());
                changeData.offset = int.Parse(varChangeObj.GetValue("offst").ToString());
                changeData.turns = int.Parse(varChangeObj.GetValue("trns").ToString());

                string var = varChangeObj.GetValue("var").ToString();
                if (var == "health") changeData.variable = VarType.health;
                if (var == "ability") changeData.variable = VarType.ability;
                if (var == "victory") changeData.variable = VarType.victory;
                if (var == "damage") changeData.variable = VarType.damage;

                // add to list for each player
                foreach(int player in affectedPlayers)
                {
                    changeData.player = player;
                    variableChanges.Add(changeData);
                }            
            }
        }
        ParseNewVarChanges(variableChanges);
    }

    /// <summary>
    /// Parses a list of VariableChanges and applies the varchange or stores it in the register for future parse
    /// </summary>
    /// <param name="changes">The list of new VariableChanges</param>
    public void ParseNewVarChanges(List<VariableChange> changes)
    {
        for(int i = 0; i < changes.Count; i++)
        {
            VariableChange change = changes[i];
            if((change.variable == VarType.ability || change.variable == VarType.damage) && change.player != currentPlayer)
            {
                registeredEffects.Add(change);
                continue;
            }

            VariableChange? result = ApplyVarChange(change);
            if (result.HasValue)
                registeredEffects.Add(result.Value);
        }
        EndUpdate();
    }

    /// <summary>
    /// This is called when its a players turn, and looks through/applies the registerd effects for any buffs/debuffs that apply
    /// to this player
    /// </summary>
    private void ApplyRegisteredEffects(int currentPlayer)
    {
        for (int i = registeredEffects.Count - 1; i >= 0; i--)
        {
            VariableChange varChange = registeredEffects[i];

            if ((varChange.variable == VarType.ability || varChange.variable == VarType.damage) && varChange.player != currentPlayer)
                continue;

            VariableChange? result = ApplyVarChange(varChange);
            if (result.HasValue)
                registeredEffects[i] = result.Value;
            else
                registeredEffects.RemoveAt(i);
        }
    }

    public VariableChange? ApplyVarChange(VariableChange varChange)
    {
        //check if this varchange first needs to be skipped
        if (varChange.offset > 0)
        {
            varChange.offset--;
            return varChange;
        }


        Debug.Log("applying damage");
        switch (varChange.variable)
        {
            case VarType.health:
                AppManager.INSTANCE.characterData[varChange.player].AddToHealth(varChange.change, false);
                break;
            case VarType.victory:
                AppManager.INSTANCE.characterData[varChange.player].AddToVictoryPoints(varChange.change);
                break;
            case VarType.ability:
                AppManager.INSTANCE.characterData[varChange.player].AddToAbilityPoints(varChange.change);
                break;
            case VarType.damage:
                AppManager.INSTANCE.characterData[varChange.player].AddToDamage(varChange.change);
                break;
        }

        varChange.turns--;
        Debug.Log(varChange.turns);
        if (varChange.turns <= 0)
        {
            return null;
        }

        return varChange;
    }
}

/// <summary>
/// Container class which holds all player-related variables and methods of changing them
/// </summary>
[System.Serializable]
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

    public int damageAdditive;

    /// <summary>
    /// Changes the players health by the specified amount
    /// </summary>
    /// <param name="change">The amount with which the players health changes</param>
    /// <param name="allowOverFlow"></param>
    /// <returns>Int signifying if the player state, where 1 is still alive, 0 is a life lost and -1 is dead</returns>
    public int AddToHealth(int change, bool allowOverFlow)
    {
        currentHealth += change;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        else if (currentHealth <= 0)
        {
            if (lives - 1 < 0)
            {
                isAlive = false;
                return -1;
            }
            else
            {
                lives--;
                currentHealth = maxHealth;
                return 0;
            }
        }
        return 1;
    }

    /// <summary>
    /// Changes the ability point additive on the player
    /// </summary>
    /// <param name="change"></param>
    public void AddToAbilityPoints(int change)
    {
        currentAbilityPoints += change;
        if (currentAbilityPoints < 0)
            currentAbilityPoints = 0;
    }

    /// <summary>
    /// Changes the players victory points by the specified amount
    /// </summary>
    /// <param name="change"></param>
    /// <param name="allowOverFlow"></param>
    public void AddToVictoryPoints(int change)
    {
        victoryPoints += change;
        if (victoryPoints < 0)
            victoryPoints = 0;
    }

    /// <summary>
    /// Changes the damage point additive on the player
    /// </summary>
    /// <param name="change"></param>
    public void AddToDamage(int change)
    {
        damageAdditive += change;
    }
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

[System.Serializable]
public struct VariableChange
{
    public int player;
    public VarType variable;
    public bool additive;
    public int change;
    public int offset;
    public int turns;
}

public enum VarType
{
    health,
    ability,
    victory,
    damage
}
