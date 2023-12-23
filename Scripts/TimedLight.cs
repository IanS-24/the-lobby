using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering.Universal;

public class TimedLight : MonoBehaviour
{
    [SerializeField] GameObject platformToHide;
    [SerializeField] float timer;
    bool ready = true;

    void Update()
    {
         Bounds b = GetComponent<BoxCollider2D>().bounds;

        if (Physics2D.OverlapBox(b.center, b.extents*2, 0, LayerMask.GetMask("Player")))
        {
            if (ready)
            {
                platformToHide.SetActive(true);
                StartCoroutine("Fade");
                GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("TimedLight");
            }
        }
    }

    IEnumerator Fade()
    {
        ready = false;
        for (float i = timer; i > 1; i-=0.01f)
        {
            GetComponent<Light2D>().intensity = i/timer;
            platformToHide.GetComponent<Tilemap>().color = new Color32(255, 255, 255, (byte)(255*(i/timer)));
            yield return new WaitForSeconds(0.01f);
        }
        platformToHide.SetActive(false);
        for (float i = 1; i > 0; i-=0.01f)
        {
            GetComponent<Light2D>().intensity = i/timer;
            yield return new WaitForSeconds(0.01f);
        }
        yield return new WaitForSeconds(1);
        ready = true;
    }
}
