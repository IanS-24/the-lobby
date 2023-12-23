using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearOnButton : MonoBehaviour
{
    PlayerInfo player;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerInfo>();
    }

    void Update()
    {
        if (player.button)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;
        }
        else
        {
            GetComponent<SpriteRenderer>().enabled = true;
            GetComponent<BoxCollider2D>().enabled = true;
        }
    }
}
