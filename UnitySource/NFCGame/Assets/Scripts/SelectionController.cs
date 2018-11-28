using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class SelectionController : MonoBehaviour
{
    private int currentSelectingPlayer;

	void OnEnable ()
    {
        AppManager.INSTANCE.OnValidJsonRecieved += OnDataRecievedHandler;
        currentSelectingPlayer = 1;
	}

    void OnDisable()
    {
        AppManager.INSTANCE.OnValidJsonRecieved -= OnDataRecievedHandler;
    }

    void OnDataRecievedHandler(object sender, JObject e)
    {
        PlayerData data = new PlayerData();

        data.name = e.GetValue("name").Value<string>();
        AddCharacter(data);
    }

    void AddCharacter(PlayerData data)
    {
        AppManager.INSTANCE.characterData.Add(currentSelectingPlayer, data);
        currentSelectingPlayer++;
    }

    void RemoveCharacter(int player)
    {

    }

    void UpdateUI()
    {

    }
}
