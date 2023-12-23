using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class NoteLight : MonoBehaviour
{
    PlayerInfo playerInfo;
    AudioManager audioManager;
    bool activated;

    void Start()
    {
        playerInfo = GameObject.Find("Player").GetComponent<PlayerInfo>();
        audioManager = GameObject.Find("Audio Manager").GetComponent<AudioManager>();
    }
    
    void Update()
    {
        Bounds b = GetComponent<BoxCollider2D>().bounds;

        if (Physics2D.OverlapBox(b.center, b.extents*2, 0, LayerMask.GetMask("Player")))
        {
            if (!activated)
            {
                StopCoroutine("Fade");
                activated = true;
                GetComponent<Light2D>().intensity = 0.8f;
                transform.GetChild(0).GetComponent<Light2D>().intensity = 0.2f;
                audioManager.Play("Note" + playerInfo.currentNote);
                playerInfo.currentNote = playerInfo.currentNote%11 + 1;
            }
        }
        else if (activated && !Physics2D.OverlapBox(b.center, b.extents*3, 0, LayerMask.GetMask("Player")))
        {
            activated = false;
            StartCoroutine("Fade");
        }
    }

    IEnumerator Fade()
    {
        for (float i = 1; i > 0; i-=0.01f)
        {
            transform.GetChild(0).GetComponent<Light2D>().intensity = 0.2f*i;
            GetComponent<Light2D>().intensity = 0.2f + 0.6f*i;
            yield return new WaitForSeconds(0.01f);
        }

        for (float i = 1; i > 0; i-=0.01f)
        {
            GetComponent<Light2D>().intensity = 0.2f*i;
            yield return new WaitForSeconds(0.01f);
        }
    }
}
