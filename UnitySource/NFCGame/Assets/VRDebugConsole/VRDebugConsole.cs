using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Debug helper class which controls an in game UI-panel which shows incoming log data from Debug
/// </summary>
public class VRDebugConsole : MonoBehaviour
{
    public Color logColor;
    public Color warningColor;
    public Color assertionColor;

    public GameObject logMessage;
    public RectTransform messageBox;

    public List<GameObject> displayedMessages;

    private bool isQuitting = false;

    private Queue<KeyValuePair<string, LogType>> messagesToWrite = new Queue<KeyValuePair<string, LogType>>();

	void Awake ()
    {
        Application.logMessageReceivedThreaded += LogCallback;
        Application.quitting += () => isQuitting = true;
        Debug.Log("ui logger is initialized");
	}

    void Update()
    {
        if(messagesToWrite.Count > 0)
        {
            DisplayNewMessage();
        }
    }

    public void LogCallback(string condition, string stackTrace, LogType type)
    {
        messagesToWrite.Enqueue(new KeyValuePair<string, LogType>(condition, type));

        /*if (isQuitting)
            return;

        displayedMessages.Insert(0, InstantiateNewLogMessage(condition, type));
        ShiftMessages();
        DestroyOutOfBoundMessages();*/
    }

    private void DisplayNewMessage()
    {
        if (isQuitting)
            return;

        KeyValuePair<string, LogType> message = messagesToWrite.Dequeue();

        displayedMessages.Insert(0, InstantiateNewLogMessage(message.Key, message.Value));
        ShiftMessages();
        DestroyOutOfBoundMessages();
    }

    private GameObject InstantiateNewLogMessage(string condition, LogType type)
    {
        GameObject instance = Instantiate(logMessage, messageBox);

        instance.GetComponentInChildren<Text>().text = condition;
        switch (type)
        {
            case LogType.Assert:
            case LogType.Error:
            case LogType.Exception:
                instance.GetComponentInChildren<Image>().color = assertionColor;
                break;
            case LogType.Warning:
                instance.GetComponentInChildren<Image>().color = warningColor;
                break;
            default:
                instance.GetComponentInChildren<Image>().color = logColor;
                break;
        }

        RectTransform instanceRect = instance.GetComponent<RectTransform>();
        Vector3 position = Vector3.zero;

        position.y = (messageBox.rect.height / 2) - (instanceRect.rect.height / 2);
        instance.transform.localPosition = position;

        return instance;
    }

    private void ShiftMessages()
    {
        if(displayedMessages.Count > 1)
        {
            for(int i = 1; i < displayedMessages.Count; i++)
            {
                Vector3 position = displayedMessages[i].transform.localPosition;
                position.y -= displayedMessages[i].GetComponent<RectTransform>().rect.height;
                displayedMessages[i].transform.localPosition = position;
            }
        }
    }

    private void DestroyOutOfBoundMessages()
    {
        for(int i = displayedMessages.Count - 1; i >= 0; i--)
        {
            Vector3 position = displayedMessages[i].transform.localPosition;
            if ((position.y * 2) * -1 >= messageBox.rect.height)
            {
                Destroy(displayedMessages[i]);
                displayedMessages.RemoveAt(i);
            }
        }
    }
}
