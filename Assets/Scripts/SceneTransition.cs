using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public FadeScreen fadeScreen;

    public void goToScene(int sceneIndex)
    {
        StartCoroutine(TransitionCoroutine(sceneIndex));
    }

    private IEnumerator TransitionCoroutine(int sceneIndex)
    {
        fadeScreen.FadeOut();
        yield return new WaitForSeconds(fadeScreen.fadeDuration);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }

    public void goToSceneAsync(int sceneIndex)
    {
        StartCoroutine(TransitionAsyncCoroutine(sceneIndex));
    }

    private IEnumerator TransitionAsyncCoroutine(int sceneIndex)
    {
        fadeScreen.FadeOut();
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false; // 等待淡出完成後再切換場景
        
        float timer = 0f;
        while (timer < fadeScreen.fadeDuration && !asyncLoad.isDone)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        asyncLoad.allowSceneActivation = true; // 淡出完成後允許場景切換
    }
}
