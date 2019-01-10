using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectIcon : MonoBehaviour
{
    public EffectDisplay DisplayBox;
    private VariableChange varChange;

    public void ShowEffect(VariableChange change)
    {
        varChange = change;
        Button btn = GetComponent<Button>();
        btn.image.color = (change.change < 0) ? Color.red : Color.green;
        btn.transform.Find("Text").GetComponent<Text>().text = change.offset.ToString("D2");
        gameObject.SetActive(true);
    }

    public void DisplayEffect()
    {
        Debug.Log("displaying effect");
        DisplayBox.ShowEffect(varChange);
    }
}
