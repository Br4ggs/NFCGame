using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

/// <summary>
/// Main class for enyclopedia module/scene
/// </summary>
public class EncyclopediaController : MonoBehaviour
{
    public Text nameTxt;
    public Text descriptionTxt;
    public Text typeTxt;

    void OnEnable()
    {
        AppManager.INSTANCE.OnValidJsonRecieved += OnDataRecievedHandler;
    }

    void OnDisable()
    {
        AppManager.INSTANCE.OnValidJsonRecieved -= OnDataRecievedHandler;
    }

    public void OnDataRecievedHandler(object sender, JObject e)
    {
        nameTxt.text = e.GetValue("name").ToString();
        descriptionTxt.text = e.GetValue("description").ToString();
        typeTxt.text = e.GetValue("typeOf").ToString();
    }
}
