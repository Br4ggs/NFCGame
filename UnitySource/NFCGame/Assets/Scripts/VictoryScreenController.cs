using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class for controlling victory scene
/// </summary>
public class VictoryScreenController : MonoBehaviour
{
    public Text playerNumber;
    public Text playerHeroName;

	void Start ()
    {
        Debug.Log(AppManager.INSTANCE.characterData.Count);
        for (int i = 0; i < AppManager.INSTANCE.characterData.Count; i++)
        {
            if (AppManager.INSTANCE.characterData[i].isAlive)
            {
                playerNumber.text = "Player " + (i + 1) + " as:";
                playerHeroName.text = AppManager.INSTANCE.characterData[i].name;
                return;
            }
        }
	}

    void OnDisable()
    {
        AppManager.INSTANCE.characterData.Clear();
    }
}
