using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static LightObjectPool;

public class AutoFadeAway : MonoBehaviour, IBootable, ITextMeshProContact
{
    public TextMeshProUGUI textMP;
    public Vector2 fadeDirection = Vector2.zero;
    public float fadeSpeed = 1;
    public float minFadeSpeed = 0.7f;
    public float fadeTime = 1;
    public float maxSize = 1;
    public float minSize = 1;
    public float timePassed = 0;
    public float fadeLoopDelay = 0.1f;

    public void BootSync()
    {
        timePassed = 0;
        gameObject.SetActive(true);
        fadeDirection = new Vector2(Random.Range(minFadeSpeed, fadeSpeed), Random.Range(minFadeSpeed, fadeSpeed));
        textMP.alpha = 1;
        float randomSize = Random.Range(minSize, maxSize);
        transform.localScale = new Vector3(randomSize, randomSize, 1);
        StartCoroutine(FadeLoop());
    }

    public IEnumerator FadeLoop() {
        while (textMP.alpha > 0.1f) {
            yield return new WaitForSeconds(fadeLoopDelay);
            transform.position = transform.position + (Vector3)fadeDirection*fadeLoopDelay;
            float time = fadeTime - (fadeTime)/(fadeTime/timePassed);
            textMP.alpha = time;
        }

        gameObject.SetActive(false);
        yield return this;
    }

    // Update is called once per frame
    void Update()
    {
        timePassed += Time.deltaTime;
    }

    public void SetText(string text) {
        textMP.text = text;
    }

    public IEnumerator Boot()
    {
        throw new System.NotImplementedException();
    }
}
