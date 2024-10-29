using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{

    private float fpsCounterTimer = 0;
    private float fpsCounter = 0;
    private void Start()
    {
        GUI.depth = 2;
    }
    
    private void OnGUI()
    {
        GUI.Label(new Rect(5, 10, 200, 50), "FPS: " + Mathf.Round(avgFrameRate));
    }
    
     public int avgFrameRate = 0;
    
     public void Update ()
     {
         fpsCounterTimer += Time.deltaTime;
         fpsCounter++;

         if (fpsCounterTimer > 1) 
         {
             avgFrameRate = (int)fpsCounter;
             fpsCounter = 0;
             fpsCounterTimer = 0;
         }
     }
}