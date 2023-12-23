using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TransparentWall: MonoBehaviour
{
    [SerializeField] byte noOverlap;
    [SerializeField] byte playerOverlap;

    bool triggered;

    void Update()
    {
        Bounds b = GetComponent<TilemapCollider2D>().bounds;
        if (Physics2D.OverlapBox(b.center, b.extents * 2, 0, LayerMask.GetMask("Player")))
        {
            if (!triggered)
            {
                triggered = true;
                StartCoroutine(Fade(noOverlap, playerOverlap));
            }
        }
        else if (triggered)
        {
            triggered = false;
            StartCoroutine(Fade(playerOverlap, noOverlap));
        }
    }

    IEnumerator Fade(float start, float end)
    {
        for (float i = 0; i < 1.1f; i += 0.1f)
        {
            byte alpha = (byte)(Mathf.Lerp(start, end, i/1));
            GetComponent<Tilemap>().color = new Color32(255, 255, 255, alpha);
            foreach (Transform child in transform)
            {
                child.GetComponent<Tilemap>().color = new Color32(255, 255, 255, alpha);
            }
            yield return new WaitForSeconds(0.03f);
        }
    }
}