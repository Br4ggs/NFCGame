using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class SelectionController : MonoBehaviour
{
    public GameObject[] playerProfiles;

	void OnEnable ()
    {
        foreach(GameObject playerProfile in playerProfiles)
        {
            playerProfile.SetActive(false);
        }
        AppManager.INSTANCE.scannerManager.Active = true;
        AppManager.INSTANCE.OnValidJsonRecieved += OnDataRecievedHandler;
	}

    void OnDisable()
    {
        AppManager.INSTANCE.OnValidJsonRecieved -= OnDataRecievedHandler;
    }

    void OnDataRecievedHandler(object sender, JObject e)
    {
        if (e.GetValue("typeOf").ToString() != "Character")
        {
            Debug.LogAssertion("incorrect card type was played");
            return;
        }

        PlayerData data = e.ToObject<PlayerData>();
        AddCharacter(data);
    }

    void AddCharacter(PlayerData data)
    {
        AppManager.INSTANCE.characterData.Add(data);
        UpdateUI();
    }

    public void RemoveCharacter(int index)
    {
        AppManager.INSTANCE.characterData.RemoveAt(index);
        UpdateUI();
    }

    void UpdateUI()
    {
        for(int i = 0; i < playerProfiles.Length; i++)
        {
            if(AppManager.INSTANCE.characterData.Count > i)
            {
                playerProfiles[i].SetActive(true);
                PlayerData data = AppManager.INSTANCE.characterData[i];
                GameObject profile = playerProfiles[i];
                profile.transform.Find("Name").GetComponent<Text>().text = data.name;
            }
            else
            {
                playerProfiles[i].SetActive(false);
            }
        }
    }
}
