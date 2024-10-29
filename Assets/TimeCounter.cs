using System;
using UnityEngine;
using System.Collections;
using GWBase;

public class TimeCounter : MonoBehaviour
{
    private float count;

    private void Start()
    {
        
        GUI.depth = 2;
        StartCoroutine(Setup());
    }

    private void Update()
    {
        timePassed += Time.deltaTime;
    }

    public float timePassed;
    private IEnumerator Setup()
    {
        while (true)
        {
            count = 1f / Time.unscaledDeltaTime;
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private void OnGUI()
    {
        GUI.Label(new Rect(5, 30, 200, 50), "Time: " + Math.Round(timePassed, 1));
    }
}