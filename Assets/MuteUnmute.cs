using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MuteUnmute : MonoBehaviour
{
    public Sprite mutedSprite;

    public Sprite unmutedSprite;
    private bool isMuted = false;

    private Image _renderer;
    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<Image>();
    }

    public void Toggle()
    {
        isMuted = !isMuted;
        if (isMuted)
        {
            _renderer.sprite = mutedSprite;
            AudioListener.pause = true;
            AudioListener.volume = 0;
        }
        else
        {
            _renderer.sprite = unmutedSprite;
            AudioListener.pause = false;
            AudioListener.volume = 1;
        }
    }
}
