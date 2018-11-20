using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    public static AppManager ManagerInstance { get; private set; }
    private SceneSwitcher sceneSwitcher;
    private ScannerManager scannerManager;

    public PlayerData[] playerData;

    void Awake()
    {
        if (ManagerInstance == null)
        {
            DontDestroyOnLoad(this);

            ManagerInstance = this;
            sceneSwitcher = gameObject.GetComponent<SceneSwitcher>();
            scannerManager = gameObject.GetComponent<ScannerManager>();

            Array.ForEach(gameObject.GetComponents<ManagerComponent>(), comp => comp.Setup());
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    //perhaps you should make the other components public statics on this one,
    //that would make this manager more dynamic and would remove the need for wrapper methods

    public void SwitchScene(int index)
    {
        sceneSwitcher.SwitchScene(index);
    }

    public void SubScribeToScanner()
    {
        //stump
    }

    public void UnSubScribeToScanner()
    {
        //stump
    }
}

public enum AppState
{
    InGame,
    InMenu
}

public struct PlayerData
{
    public int MaxHealth;
    public int CurrentHealth;
    public int CurrentActionPoints;
    public int VictoryPoints;
}
