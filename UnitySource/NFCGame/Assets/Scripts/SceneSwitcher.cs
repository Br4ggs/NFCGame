using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour, ManagerComponent
{
    public float animationTime;
    public GameObject transitionPrefab;
    private GameObject transitionInstance;
    private Animator animator;

    public void Setup()
    {
        transitionInstance = Instantiate(transitionPrefab);
        animator = transitionInstance.GetComponent<Animator>();

        if (animator == null)
            Debug.LogAssertion("the transition prefab must have an animator attached to it");

        DontDestroyOnLoad(transitionInstance);
        transitionInstance.SetActive(false);
    }

    public void SwitchScene(int index)
    {
        StartCoroutine(SwitchSceneRoutine(index));
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
