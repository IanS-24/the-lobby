using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public float musicVolume;
    [SerializeField] GameObject musicSlider;
    public float sfxVolume;
    [SerializeField] GameObject sfxSlider;
    public float textSpeed;
    [SerializeField] GameObject textSlider;

    [Header("Colors")]
    public Color32 onColor;
    public Color32 offColor;

    void Start()
    {
        MusicSlider();
        SfxSlider();
        TextSlider();
    }

    public void MusicSlider()
    {
        foreach (Transform child in musicSlider.transform)
        {
            if (musicVolume * 5 >= child.GetSiblingIndex() + 1)
            {
                child.GetComponent<Image>().color = onColor;
            }
            else
                child.GetComponent<Image>().color = offColor;
        }
    }

    public void SfxSlider()
    {
        foreach (Transform child in sfxSlider.transform)
        {
            if (sfxVolume * 5 >= child.GetSiblingIndex() + 1)
            {
                child.GetComponent<Image>().color = onColor;
            }
            else
                child.GetComponent<Image>().color = offColor;
        }
    }

    public void TextSlider()
    {
        foreach (Transform child in textSlider.transform)
        {
            if ((1.2f-textSpeed) * 5 >= child.GetSiblingIndex() + 0.1f)
            {
                child.GetComponent<Image>().color = onColor;
            }
            else
                child.GetComponent<Image>().color = offColor;
        }
    }

    /*public void IncrementMusic(int sign)
    {
        musicVolume += 0.2f*sign;
        if (musicVolume > 1)
            musicVolume = 1;
        else if (musicVolume < 0)
            musicVolume = 0;
        MusicSlider();
    }

    public void IncrementSfx(int sign)
    {
        sfxVolume += 0.2f * sign;
        if (sfxVolume > 1)
            sfxVolume = 1;
        else if (sfxVolume < 0)
            sfxVolume = 0;
       SfxSlider();
    }

    public void IncrementTextSpeed(int sign)
    {
        textSpeed += 0.2f * sign;
        if (textSpeed > 1)
            textSpeed = 1;
        else if (textSpeed < 0)
            textSpeed = 0;
        TextSlider();
    }*/

    public void SetMusic(float n)
    {
        /*if (musicVolume > n)
            musicVolume = n - 0.2f;
        else*/
            musicVolume = n;
        MusicSlider();
    }

    public void SetSfx(float n)
    {
        /*if (sfxVolume > n)
            sfxVolume = n - 0.2f;
        else*/
            sfxVolume = n;
        SfxSlider();
    }

    public void SetTextSpeed(float n)
    {
        /*if (textSpeed > n)
            textSpeed = n - 0.2f;
        else*/
            textSpeed = 1.2f-n;
        TextSlider();
    }
}
