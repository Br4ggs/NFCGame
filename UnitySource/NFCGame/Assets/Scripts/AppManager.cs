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

    private bool showPopup;
    private GameObject scannerPopup;

    public static AppManager INSTANCE { get; private set; }
    public ScannerManager scannerManager { get; private set; }

    public event OnValidJsonRecievedHandler OnValidJsonRecieved;
    public delegate void OnValidJsonRecievedHandler(object sender, JObject e);

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
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void Update()
    {
        if (scannerManager.DataInRecievedQueue())
            DataRecieved(scannerManager.ReadLine());

        scannerPopup.SetActive(showPopup);
    }

    public void DataRecieved(string data)
    {
        try
        {
            JObject o = JObject.Parse(data);
            OnValidJsonRecieved(this, o);
        }
        catch(JsonReaderException)
        {
            //do nothing for now
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

    public IEnumerator SwitchSceneRoutine(int index)
    {
        scannerManager.Active = false;
        scannerManager.DiscardRecievedQueue();

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

    public void ConnectionStateHandler(object sender, ConnectionState e)
    {
        if(e == ConnectionState.SEARCHING)
        {
            showPopup = true;
        }
        else
        {
            showPopup = false;
        }
    }
}
