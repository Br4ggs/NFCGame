using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public void SwitchScene(int index)
    {
        AppManager.INSTANCE.SwitchScene(index);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
