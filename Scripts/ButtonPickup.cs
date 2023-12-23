using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPickup : MonoBehaviour
{
    public bool pickedUp;

    void Update()
    {
        Bounds b = GetComponent<BoxCollider2D>().bounds;
        if (Physics2D.OverlapBox(b.center, b.extents * 2, 0, LayerMask.GetMask("Player")) && !pickedUp)
        {
            pickedUp = true;
            GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("ButtonPickup");
            GameObject.Find("Player").GetComponent<PlayerInfo>().button = true;
            GetComponent<SpriteRenderer>().enabled = false;
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}
