using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TransparentTilemap : MonoBehaviour
{
    [SerializeField] byte beforeButton;
    [SerializeField] byte afterButton;
    [SerializeField] byte playerOverlap;

    void Update()
    {
        if (GameObject.Find("Player").GetComponent<PlayerInfo>().button)
        {
            GetComponent<TilemapCollider2D>().isTrigger = true;
            Bounds b = GetComponent<TilemapCollider2D>().bounds;
            if (Physics2D.OverlapBox(b.center, b.extents * 2, 0, LayerMask.GetMask("Player")))
            {
                GetComponent<Tilemap>().color = new Color32(255, 255, 255, playerOverlap);
            }
            else
            {
                GetComponent<Tilemap>().color = new Color32(255, 255, 255, afterButton);
            }
        }
        else
        {
            GetComponent<TilemapCollider2D>().isTrigger = false;
            gameObject.GetComponent<Tilemap>().color = new Color32(255, 255, 255, beforeButton);
        }
    }
}
