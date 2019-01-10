using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectDisplay : MonoBehaviour
{
    public Text varTypeIndicator;
    public Text changeCounter;
    public Text turnsCounter;
    public Text offsetCounter;
    public Image backgroundImage;

    void Awake()
    {
        varTypeIndicator = transform.Find("VariableCounter").GetComponent<Text>();
        changeCounter = transform.Find("ChangeCounter").GetComponent<Text>();
        turnsCounter = transform.Find("TurnsCounter").GetComponent<Text>();
        offsetCounter = transform.Find("OffsetCounter").GetComponent<Text>();
        backgroundImage = GetComponent<Image>();
    }

    public void ShowEffect(VariableChange change)
    {
        gameObject.SetActive(true);

        varTypeIndicator.text = change.variable.ToString();
        changeCounter.text = (change.change < 0) ? change.change.ToString("D2") : "+" + change.change.ToString("D2");
        turnsCounter.text = change.turns.ToString("D2");
        offsetCounter.text = change.offset.ToString("D2");
        backgroundImage.color = (change.change < 0) ? Color.red : Color.green;
    }
}
