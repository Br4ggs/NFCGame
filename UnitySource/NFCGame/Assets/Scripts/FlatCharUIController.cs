using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlatCharUIController : MonoBehaviour
{
    public EffectIcon[] effectIcons;
    public EffectDisplay effectDisplay;
    public GameObject VarChangeEffect;
    private Image background;
    private Color defaultBackgroundColor;
    private Text charName;
    private Text lives;
    private Text hitPoints;
    private Text abilityPoints;
    private Text victoryPoints;

    private RectTransform canvas;

    private int visualisedDebufs;

    private bool highlighted;
    public bool HighLighted
    {
        get
        {
            return highlighted;
        }
        set
        {
            bool prevValue = highlighted;
            highlighted = value;
            if(prevValue != highlighted)
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

        effectDisplay = transform.Find("EffectDisplay").GetComponent<EffectDisplay>();
        effectDisplay.gameObject.SetActive(false);
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
        StartCoroutine(ShowVarChangeCoRoutine(0.5f, variable, change, newData));
    }

    public void RegisterEffect(VariableChange change)
    {
        if (visualisedDebufs > effectIcons.Length - 1)
            return;

        EffectIcon icon = effectIcons[visualisedDebufs];

        icon.ShowEffect(change);
        visualisedDebufs++;
    }

    public void RemoveEffects()
    {
        effectDisplay.gameObject.SetActive(false);

        foreach(EffectIcon effect in effectIcons)
        {
            effect.gameObject.SetActive(false);
        }
        visualisedDebufs = 0;
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
