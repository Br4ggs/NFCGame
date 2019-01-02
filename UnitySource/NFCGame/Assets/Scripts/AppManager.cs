using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class AppManager : MonoBehaviour
{
    [Header("Scene transition settings")]
    public float animationTime;
    public GameObject transitionPrefab;
    private GameObject transitionInstance;
    private Animator animator;

    private GameObject scannerPopup;

    public static AppManager INSTANCE { get; private set; }
    private ScannerManager scannerManager;

    private bool stateChanged = false;
    /// <summary>
    /// Thread-safe event that gets called when the serial state has changed
    /// </summary>
    public event DeviceConnectionStatusChangedHandler OnSerialStateChanged;

    /// <summary>
    /// Event that gets called when recieved data was sucessfully parsed to a Json object
    /// </summary>
    public event OnValidJsonRecievedHandler OnValidJsonRecieved;
    public delegate void OnValidJsonRecievedHandler(object sender, JObject e);

    /// <summary>
    /// Event that gets called when recieved data could not be parsed to a Json object, such as error messages or status codes
    /// </summary>
    public event OnDataRecievedHandler OnDataRecieved;
    public delegate void OnDataRecievedHandler(object sender, string e);

    public List<PlayerData> characterData = new List<PlayerData>();

    void Awake()
    {
        if (INSTANCE == null)
        {
            DontDestroyOnLoad(this);

            INSTANCE = this;

            SetupTransition();
            scannerPopup = transform.Find("ScannerPopup").gameObject;

            scannerManager = new ScannerManager();
            scannerManager.OnConnectionStateChanged += ConnectionStateHandler;

            OnSerialStateChanged += ShowPopup;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void Update()
    {
        string data = scannerManager.ReadLine();
        if (data != null)
            DataRecieved(data);

        if (stateChanged)
        {
            stateChanged = false;

            ConnectionState newState = scannerManager.State;
            OnSerialStateChanged(this, newState);
        }
    }

    public void SwitchScene(int index)
    {
        StartCoroutine(SwitchSceneRoutine(index));
    }

    public void SendMessageToSerial(string msg)
    {
        scannerManager.WriteLine(msg);
    }

    private void DataRecieved(string data)
    {
        try
        {
            JObject o = JObject.Parse(data);
            if(OnValidJsonRecieved != null)
                OnValidJsonRecieved(this, o);
        }
        catch(JsonReaderException)
        {
            if(OnDataRecieved != null)
                OnDataRecieved(this, data);
        }
    }

    private void OnDestroy()
    {
        if (scannerManager != null)
            scannerManager.Dispose();
    }

    private void SetupTransition()
    {
        transitionInstance = Instantiate(transitionPrefab);
        animator = transitionInstance.GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogAssertion("the transition prefab must have an animator attached to it");
            return;
        }

        DontDestroyOnLoad(transitionInstance);
        transitionInstance.SetActive(false);
    }

    private IEnumerator SwitchSceneRoutine(int index)
    {
        transitionInstance.SetActive(true);
        animator.Play("TransitionOut");
        yield return new WaitForSeconds(animationTime);
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(index);
        while (!sceneLoad.isDone)
        {
            yield return null;
        }
        animator.Play("TransitionIn");
        yield return new WaitForSeconds(animationTime);
        transitionInstance.SetActive(false);
    }

    /// <summary>
    /// Thread-safe callback method that lets the main-unity thread know that the serial state has been changed
    /// </summary>
    private void ConnectionStateHandler(object sender, ConnectionState e)
    {   
        stateChanged = true;
    }

    private void ShowPopup(object sender, ConnectionState e)
    {
        scannerPopup.SetActive((e != ConnectionState.CONNECTED));
    }
}
