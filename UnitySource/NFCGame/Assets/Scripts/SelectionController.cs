using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// not gonna lie, this is hacky. I'll come back to this later to fix this
public class SelectionController : MonoBehaviour
{
    public PopupDialog messageBox;
    bool dialogUp;
    public GameObject[] playerProfiles;
    public float offScreenOffset;
    public float defaultYPos;

    private bool allowedToMove = true;
    private bool deleteAble = true;

	void OnEnable ()
    {
        foreach (GameObject playerProfile in playerProfiles)
        {
            StartCoroutine(LerpToCoRoutine(playerProfile, new Vector3(playerProfile.transform.localPosition.x, offScreenOffset, playerProfile.transform.localPosition.z), false, false, false));
        }
        AppManager.INSTANCE.characterData.Clear();
        AppManager.INSTANCE.OnValidJsonRecieved += OnDataRecievedHandler;
	}

    void OnDisable()
    {
        AppManager.INSTANCE.OnValidJsonRecieved -= OnDataRecievedHandler;
    }

    public void OnDataRecievedHandler(object sender, JObject e)
    {
        if (dialogUp)
            return;

        JToken token = e.GetValue("typeOf");
        if (token == null || token.ToString() != "Character")
        {
            messageBox.ShowDialog("incorrect card type was played", MessageBoxCallBack);
            dialogUp = true;
            return;
        }

        PlayerData data = e.ToObject<PlayerData>();

        if(AppManager.INSTANCE.characterData.Count < 4)
            AddCharacter(data);
    }

    public void MessageBoxCallBack()
    {
        dialogUp = false;
    }

    public void AddCharacter(PlayerData data)
    {
        AppManager.INSTANCE.characterData.Add(data);
        GameObject profile = playerProfiles[AppManager.INSTANCE.characterData.Count - 1];
        profile.transform.Find("Name").GetComponent<Text>().text = data.name;
        StartCoroutine(LerpToCoRoutine(profile, new Vector3(profile.transform.localPosition.x, 0, profile.transform.localPosition.z), true, true, false));
    }

    public void RemoveCharacter(int index)
    {
        deleteAble = false;
        SetDeleteAble();

        AppManager.INSTANCE.characterData.RemoveAt(index);
        GameObject profile = playerProfiles[index];
        StartCoroutine(LerpToCoRoutine(profile, new Vector3(profile.transform.localPosition.x, offScreenOffset, profile.transform.localPosition.z), true, false, true));

        deleteAble = true;
        Invoke("SetDeleteAble", 1.5f);
    }

    private void ReorderUI()
    {
        allowedToMove = false;
        for (int i = 0; i < playerProfiles.Length; i++)
        {
            GameObject profile = playerProfiles[i];
            if (i < AppManager.INSTANCE.characterData.Count)
            {
                Debug.Log("this was reached");
                GameObject nextProfile = playerProfiles[i + 1];

                if (!profile.activeSelf)
                {
                    Vector3 destination = profile.transform.localPosition;
                    destination.y = 0;

                    profile.transform.localPosition = nextProfile.transform.localPosition;
                    profile.transform.Find("Name").GetComponent<Text>().text = nextProfile.transform.Find("Name").GetComponent<Text>().text;

                    StartCoroutine(LerpToCoRoutine(profile, destination, true, true, false));
                    nextProfile.SetActive(false);
                }
            }
            else
            {
                StartCoroutine(LerpToCoRoutine(profile, new Vector3(profile.transform.localPosition.x, offScreenOffset, profile.transform.localPosition.z), false, false, false));
            }
        }
        allowedToMove = true;
    }

    private IEnumerator LerpToCoRoutine(GameObject profile, Vector3 destination, bool activeOnBegin, bool activeOnEnd, bool reorderOnEnd)
    {
        profile.SetActive(activeOnBegin);

        while (!allowedToMove)
            yield return null;

        while(Vector3.Distance(profile.transform.localPosition, destination) > 0.05f)
        {
            profile.transform.localPosition = Vector3.Lerp(profile.transform.localPosition, destination, 0.2f);
            yield return null;
        }

        profile.transform.localPosition = destination;
        profile.SetActive(activeOnEnd);

        if (reorderOnEnd)
            ReorderUI();
    }

    public void SetDeleteAble()
    {
        foreach (GameObject playerProfile in playerProfiles)
        {
            playerProfile.transform.Find("Button").GetComponent<Button>().interactable = deleteAble;
        }
    }
}
