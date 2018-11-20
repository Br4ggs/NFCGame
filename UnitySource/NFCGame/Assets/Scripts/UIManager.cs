using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public int switchToScene;

    public void Activate()
    {
        AppManager.ManagerInstance.SwitchScene(switchToScene);
    }
}
