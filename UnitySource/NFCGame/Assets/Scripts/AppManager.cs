using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private Dictionary<int, CharacterData> characterData;

    void Awake()
    {
        if (INSTANCE == null)
        {
            DontDestroyOnLoad(this);

            INSTANCE = this;

            SetupTransition();
            //scannerManager = new ScannerManager();
            //scannerManager.OnDataRecieved += DataRecieved;

            //LoadResourceData();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void DataRecieved(string data)
    {
        Debug.Log("DATA WAS RECIEVED FROM SERIAL");
        Debug.Log(data);
    }

    public void SwitchScene(int index)
    {
        StartCoroutine(SwitchSceneRoutine(index));
    }

    private void OnDestroy()
    {
        scannerManager.Dispose();
    }

    private void LoadResourceData()
    {
        CharacterData[] data = Resources.LoadAll<CharacterData>(characterDataPath);
        
        foreach(CharacterData obj in data)
        {
            characterData.Add(obj.dataID, obj);
        }
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
    public int MaxHealth;
    public int CurrentHealth;
    public int CurrentActionPoints;
    public int VictoryPoints;
}
