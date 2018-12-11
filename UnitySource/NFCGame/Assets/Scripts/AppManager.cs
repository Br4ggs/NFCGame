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
    public ScannerManager scannerManager { get; private set; }

    private readonly object stateLock = new object();
    private bool stateChanged = false;
    public event DeviceConnectionStatusChangedHandler OnSerialStateChanged;

    public event OnValidJsonRecievedHandler OnValidJsonRecieved;
    public delegate void OnValidJsonRecievedHandler(object sender, JObject e);

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
            //lock (stateLock)
            //{
                stateChanged = false;
            //}

            ConnectionState newState = scannerManager.State;
            OnSerialStateChanged(this, newState);
        }
    }

    //this should be moved to scannermanager
    //with appmanager offering subscribe and unsubscribe methods
    public void DataRecieved(string data)
    {
        try
        {
            JObject o = JObject.Parse(data);
            OnValidJsonRecieved(this, o);
        }
        catch(JsonReaderException)
        {
            OnDataRecieved(this, data);
        }
    }

    public void SwitchScene(int index)
    {
        StartCoroutine(SwitchSceneRoutine(index));
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

    private void ConnectionStateHandler(object sender, ConnectionState e)
    {
        //lock (stateLock)
        //{
            stateChanged = true;
        //}
    }

    private void ShowPopup(object sender, ConnectionState e)
    {
        if(scannerManager.SerialConnectionenabled)
            scannerPopup.SetActive((e != ConnectionState.CONNECTED));
    }
}
