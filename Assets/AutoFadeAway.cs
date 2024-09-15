using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using GWBase;

public class AutoFadeAway : MonoBehaviour, IBootable, ITextMeshProContact
{
    public TextMeshProUGUI textMP;
    public float fadeTime = 1;
    public float maxSize = 1;
    public float minSize = 1;
    public float timePassed = 0;
    public void BootSync()
    {
        timePassed = 0;
        gameObject.SetActive(true);
        textMP.alpha = 1;
        float randomSize = Random.Range(minSize, maxSize);
        transform.localScale = new Vector3(randomSize, randomSize, 1);
        gameObject.SetActive(true);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!gameObject.activeSelf) return;
        
        timePassed += Time.fixedDeltaTime;
        float time = fadeTime - (fadeTime)/(fadeTime/timePassed);
        textMP.alpha = time;

        if (textMP.alpha < 0.1f)
        {
            gameObject.SetActive(false);
        }
    }

    public void SetText(string text) {
        textMP.text = text;
    }

    public IEnumerator Boot()
    {
        throw new System.NotImplementedException();
    }
}
