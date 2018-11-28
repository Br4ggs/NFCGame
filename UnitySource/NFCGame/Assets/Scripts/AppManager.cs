using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class AppManager : MonoBehaviour
{
    [Header("Resource settings")]
    public string characterDataPath;
    public string abilityDataPath;

    [Header("Scene transition settings")]
    public float animationTime;
    public GameObject transitionPrefab;
    private GameObject transitionInstance;
    private Animator animator;

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
            scannerManager = new ScannerManager();
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
    }

    public void DataRecieved(string data)
    {
        try
        {
            JObject o = JObject.Parse(data);
            Debug.Log("json was fully parsed");
            OnValidJsonRecieved(this, o);
        }
        catch(JsonReaderException)
        {
            Debug.Log("string was something else");
            Debug.Log(data);
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
            Debug.LogAssertion("the transition prefab must have an animator attached to it");

        DontDestroyOnLoad(transitionInstance);
        transitionInstance.SetActive(false);
    }

    public IEnumerator SwitchSceneRoutine(int index)
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
}

public struct PlayerData
{
    public string name;
    public string description;
    public int maxHealth;
    public int currentHealth;
    public int maxAbilityPoints;
    public int currentAbilityPoints;
    public int victoryPoints;
}
