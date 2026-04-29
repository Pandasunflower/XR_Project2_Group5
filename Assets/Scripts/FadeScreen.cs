using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeScreen : MonoBehaviour
{
    public bool fadeInOnStart = true; // 是否在開始時淡入
    public float fadeDuration = 1f; // 淡入淡出持續時間
    public Color fadeColor;
    private Renderer rend;
    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        if (fadeInOnStart)
        {
            FadeIn();
        }
    }

    public void FadeIn()
    {
        StartCoroutine(FadeCoroutine(1f, 0f));
    }

    public void FadeOut()
    {
        StartCoroutine(FadeCoroutine(0f, 1f));
    }

    public void Fade(float alphaIn, float alphaOut)
    {
        StartCoroutine(FadeCoroutine(alphaIn, alphaOut));
    }

    public IEnumerator FadeCoroutine(float alphaIn, float alphaOut)
    {
        float timer = 0f;
        while (timer <= fadeDuration)
        {
            Color newColor = fadeColor;
            newColor.a = Mathf.Lerp(alphaIn, alphaOut, timer / fadeDuration);
            rend.material.SetColor("_BaseColor", newColor);
            timer += Time.deltaTime;
            yield return null;
        }
        Color finalColor = fadeColor;
        finalColor.a = alphaOut;
        rend.material.SetColor("_BaseColor", finalColor);
    }


}
