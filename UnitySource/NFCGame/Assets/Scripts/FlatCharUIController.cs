using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlatCharUIController : MonoBehaviour
{
    public GameObject VarChangeEffect;
    private Image background;
    private Color defaultBackgroundColor;
    private Text charName;
    private Text lives;
    private Text hitPoints;
    private Text abilityPoints;
    private Text victoryPoints;

    private RectTransform canvas;

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
        canvas = GameObject.Find("Canvas").GetComponent<RectTransform>();
        charName = transform.Find("Name").GetComponent<Text>();
        lives = transform.Find("Lives").GetComponent<Text>();
        hitPoints = transform.Find("HealthCounter").GetComponent<Text>();
        abilityPoints = transform.Find("AbilityCounter").GetComponent<Text>();
        victoryPoints = transform.Find("VictoryCounter").GetComponent<Text>();
    }

    public void UpdateUI(PlayerData data)
    {
        charName.text = data.name;
        lives.text = data.lives.ToString("D2");
        hitPoints.text = data.currentHealth.ToString("D2");
        abilityPoints.text = data.currentAbilityPoints.ToString("D2");
        victoryPoints.text = data.victoryPoints.ToString("D2");

        if (!data.isAlive)
        {
            background.color = Color.red;
        }
    }

    private void UpdateHighLight()
    {
        background.color = highlighted ? Color.blue : defaultBackgroundColor;
    }

    public void ShowVarChange(VarType variable, int change, PlayerData newData)
    {
        //show effect on corresponding counter
        //can be different depending on if change is negative
        StartCoroutine(ShowVarChangeCoRoutine(0.5f, variable, change, newData));
    }

    public void RegisterEffect(VariableChange change)
    {
        //add an effect visualiser
    }

    public void RemoveEffects()
    {
        //remove registered effects
    }

    public IEnumerator ShowVarChangeCoRoutine(float duration, VarType variable, int change, PlayerData newData)
    {
        Vector3 position = Vector3.zero;
        switch (variable)
        {
            case VarType.health:
                position = hitPoints.transform.position;
                hitPoints.text = newData.currentHealth.ToString("D2");
                break;
            case VarType.ability:
                position = abilityPoints.transform.position;
                abilityPoints.text = newData.currentAbilityPoints.ToString("D2");
                break;
            case VarType.victory:
                position = victoryPoints.transform.position;
                victoryPoints.text = newData.victoryPoints.ToString("D2");
                break;
        }
        GameObject instance = Instantiate(VarChangeEffect, position, Quaternion.identity, canvas);
        instance.transform.Find("Text").GetComponent<Text>().text = (change > 0) ? "+" + change.ToString() : change.ToString();
        instance.GetComponent<Image>().color = (change > 0) ? Color.green : Color.red;

        yield return new WaitForSeconds(duration);

        Destroy(instance);
    }
}
