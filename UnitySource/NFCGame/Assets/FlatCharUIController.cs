﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlatCharUIController : MonoBehaviour
{
    Image background;
    Color defaultBackgroundColor;
    Text charName;
    Text hitPoints;
    Text abilityPoints;
    Text victoryPoints;

    private bool highlighted;
    public bool HighLighted
    {
        get
        {
            return highlighted;
        }
        set
        {
            highlighted = value;
            UpdateHighLight();
        }
    }

	void Awake ()
    {
        background = GetComponent<Image>();
        defaultBackgroundColor = background.color;
        charName = transform.Find("Name").GetComponent<Text>();
        hitPoints = transform.Find("HealthCounter").GetComponent<Text>();
        abilityPoints = transform.Find("AbilityCounter").GetComponent<Text>();
        victoryPoints = transform.Find("VictoryCounter").GetComponent<Text>();
    }

    public void UpdateUI(PlayerData data)
    {
        charName.text = data.name;
        hitPoints.text = data.currentHealth.ToString("D2");
        abilityPoints.text = data.currentAbilityPoints.ToString("D2");
        victoryPoints.text = data.victoryPoints.ToString("D2");
    }

    private void UpdateHighLight()
    {
        background.color = highlighted ? Color.blue : defaultBackgroundColor;
    }
}
