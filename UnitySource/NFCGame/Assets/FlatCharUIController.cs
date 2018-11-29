using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlatCharUIController : MonoBehaviour
{
    Text charName;
    Text hitPoints;
    Text abilityPoints;
    Text victoryPoints;

	void Awake ()
    {
        charName = transform.Find("Name").GetComponent<Text>();
        hitPoints = transform.Find("HealthCounter").GetComponent<Text>();
        abilityPoints = transform.Find("AbilityCounter").GetComponent<Text>();
        victoryPoints = transform.Find("VictoryCounter").GetComponent<Text>();
    }

    public void setup(PlayerData data)
    {
        charName.text = data.name;
        hitPoints.text = data.maxHealth.ToString("D2");
        abilityPoints.text = data.maxAbilityPoints.ToString("D2");
        victoryPoints.text = 0.ToString("D2");
    }
}
