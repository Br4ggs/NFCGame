using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Handles the logic of the game scene
/// uses the UI controllers to manipulate the UI depending on the games state
/// </summary>
public class GameController : MonoBehaviour
{
    public GameUIController UIController;

    private int currentPlayer;
    private int currentRound;

    public List<VariableChange> registeredEffects = new List<VariableChange>();

    /// <summary>
    /// setup
    /// </summary>
    void Start()
    {
        UIController = GetComponent<GameUIController>();

        AppManager.INSTANCE.OnValidJsonRecieved += OnDataRecievedHandler;

        currentPlayer = 0;

        for(int i = 0; i < AppManager.INSTANCE.characterData.Count; i++)
        {
            PlayerData data = AppManager.INSTANCE.characterData[i];
            data.currentHealth = data.maxHealth;
            data.currentAbilityPoints = data.maxAbilityPoints;
            data.victoryPoints = 0;
            data.lives = 3;
            data.isAlive = true;
            AppManager.INSTANCE.characterData[i] = data;
        }

        UIController.GenerateUIProfiles();
        UIController.SetHighLightedPlayer(currentPlayer);
        UIController.StartGame();
    }

    /// <summary>
    /// destructor for class which removes methods for event
    /// </summary>
    void OnDisable()
    {
        AppManager.INSTANCE.OnValidJsonRecieved -= OnDataRecievedHandler;
    }

    private bool ShouldGameEnd()
    {
        int numOfActivePlayers = 0;
        foreach (PlayerData player in AppManager.INSTANCE.characterData)
        {
            if (player.isAlive)
                numOfActivePlayers++;
        }

        return (numOfActivePlayers < 2);
    }

    /// <summary>
    /// called when all players have had their turn and the game advances to the next round
    /// </summary>
    private void NextRound()
    {
        currentPlayer = 0;
        currentRound++;
        UIController.SetRound(currentRound);

        foreach(PlayerData character in AppManager.INSTANCE.characterData)
        {
            if(character.isAlive)
                character.currentAbilityPoints = character.maxAbilityPoints;
        }
    }

    /// <summary>
    /// called when the current player finishes their turn
    /// </summary>
    private void NextPlayer()
    {
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

        UIController.SetHighLightedPlayer(currentPlayer);
        ApplyRegisteredEffects(currentPlayer);

        if (!AppManager.INSTANCE.characterData[currentPlayer].isAlive)
        {
            NextPlayer();
            return;
        }
    }

    /// <summary>
    /// this gets called when a player uses a card on the scanner, this is what advances the player's turn
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDataRecievedHandler(object sender, JObject e)
    {
        if (UIController.DialogUp)
            return;

        JToken token = e.GetValue("type");
        if (token == null || token.ToString() != "Ability")
        {
            UIController.DisplayMessageBox("An incorrect card type was played.");
            return;
        }

        int ptCost = int.Parse(e.GetValue("ptCst").ToString());
        if(AppManager.INSTANCE.characterData[currentPlayer].currentAbilityPoints < ptCost)
        {
            UIController.DisplayMessageBox("You do not have enough ability points left for this move.");
            return;
        }
        AppManager.INSTANCE.characterData[currentPlayer].currentAbilityPoints -= ptCost;

        JArray fxArray = (JArray)e.GetValue("fx");
        StartCoroutine(ConvertToStructRoutine(fxArray));
    }

    /// <summary>
    /// Called after a user has played a card or when it's the next players turn
    /// </summary>
    /// <param name="changes">List of effects that were applied this update, used for UI</param>
    private void EndUpdate(List<VariableChange> changes)
    {
        UIController.ShowVarChanges(changes);
        UIController.UpdateStatusEffects(registeredEffects);

        if (ShouldGameEnd())
            AppManager.INSTANCE.SwitchScene(4);
    }

    private IEnumerator ConvertToStructRoutine(JArray effects)
    {
        List<VariableChange> variableChanges = new List<VariableChange>();
        List<int> affectedPlayers = new List<int>();

        foreach (JToken effect in effects)
        {
            affectedPlayers.Clear();
            JObject effectObj = effect.ToObject<JObject>();
            string target = effectObj.GetValue("trgts").ToString();
            JArray varChanges = (JArray)effectObj.GetValue("varchng");

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

                    UIController.DisplayChoiceBox((targetType == "multiple"), targetPlayers.ToArray(), "test", "this is a test");

                    while (UIController.DialogUp)
                    {
                        Debug.Log("waiting...");
                        yield return null;
                    }
                    Debug.Log("moving on...");
                    affectedPlayers.AddRange(UIController.GetChoiceBoxResult());
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
        List<VariableChange> changesToSendToUI = new List<VariableChange>();
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
            {
                registeredEffects.Add(result.Value);

                if (result.Value.offset <= 0)
                    changesToSendToUI.Add(result.Value);
            }
            else
                changesToSendToUI.Add(change);
        }
        EndUpdate(changesToSendToUI);
    }

    /// <summary>
    /// This is called when its a players turn, and looks through/applies the registerd effects for any buffs/debuffs that apply
    /// to this player
    /// </summary>
    private void ApplyRegisteredEffects(int currentPlayer)
    {
        List<VariableChange> changesToSendToUI = new List<VariableChange>();
        for (int i = registeredEffects.Count - 1; i >= 0; i--)
        {
            VariableChange change = registeredEffects[i];

            if ((change.variable == VarType.ability || change.variable == VarType.damage) && change.player != currentPlayer)
                continue;

            if (!AppManager.INSTANCE.characterData[change.player].isAlive)
            {
                registeredEffects.RemoveAt(i);
                continue;
            }

            VariableChange? result = ApplyVarChange(change);
            if (result.HasValue)
            {
                registeredEffects[i] = result.Value;

                if (result.Value.offset <= 0 && change.offset <= 0)
                {
                    changesToSendToUI.Add(result.Value);
                }
            }
            else
            {
                registeredEffects.RemoveAt(i);
                changesToSendToUI.Add(change);

                Debug.Log("remove it");

            }
        }
        EndUpdate(changesToSendToUI);
    }

    public VariableChange? ApplyVarChange(VariableChange varChange)
    {
        if (!AppManager.INSTANCE.characterData[varChange.player].isAlive)
            return null;

        if (varChange.offset > 0)
        {
            varChange.offset--;
            return varChange;
        }

        switch (varChange.variable)
        {
            case VarType.health:
                AppManager.INSTANCE.characterData[varChange.player].AddToHealth(varChange.change, false);
                //send something back to check player state
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
                currentHealth = 0;
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
