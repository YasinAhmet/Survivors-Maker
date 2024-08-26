using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanceFinisher : MonoBehaviour
{
    public void QuitInstance() {
        UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }
}
