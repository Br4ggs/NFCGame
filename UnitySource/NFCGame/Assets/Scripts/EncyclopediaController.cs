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
    public GameObject messageBox;

    public Vector3 offScreenPos;
    private Vector3 onScreenPos;

    private bool inAnimation;

    void OnEnable()
    {
        AppManager.INSTANCE.OnValidJsonRecieved += OnDataRecievedHandler;
        onScreenPos = messageBox.transform.localPosition;

        messageBox.transform.localPosition = offScreenPos;
    }

    void OnDisable()
    {
        AppManager.INSTANCE.OnValidJsonRecieved -= OnDataRecievedHandler;
    }

    public void OnDataRecievedHandler(object sender, JObject e)
    {
        if (inAnimation)
            return;

        StartCoroutine(AnimationCoRoutine(e));
        inAnimation = true;
    }

    public IEnumerator AnimationCoRoutine(JObject e)
    {
        while (Vector3.Distance(messageBox.transform.localPosition, offScreenPos) > 0.05f)
        {
            messageBox.transform.localPosition = Vector3.Lerp(messageBox.transform.localPosition, offScreenPos, 0.2f);
            yield return null;
        }
        messageBox.transform.localPosition = offScreenPos;

        nameTxt.text = e.GetValue("name").ToString();

        JToken desc = e.GetValue("description");
        if (desc == null)
            desc = e.GetValue("desc");
        descriptionTxt.text = desc.ToString();

        JToken typ = e.GetValue("typeOf");
        if (typ == null)
            typ = e.GetValue("type");
        typeTxt.text = typ.ToString();

        while (Vector3.Distance(messageBox.transform.localPosition, onScreenPos) > 0.05f)
        {
            messageBox.transform.localPosition = Vector3.Lerp(messageBox.transform.localPosition, onScreenPos, 0.2f);
            yield return null;
        }
        messageBox.transform.localPosition = onScreenPos;
        inAnimation = false;
    }
}
