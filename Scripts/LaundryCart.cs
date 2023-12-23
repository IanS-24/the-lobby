//Author: Ian Stolte
//Date: 7/31/23
//Desc: Additional behavior for the laundry cart

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaundryCart : MonoBehaviour
{
    Vector3 spawnpoint;
    PlayerMovement player;
    [SerializeField] bool onCart;

    void Start()
    {
        spawnpoint = transform.position;
        player = GameObject.Find("Player").GetComponent<PlayerMovement>();
    }

    void Update()
    {
        Bounds b = GetComponent<BoxCollider2D>().bounds;
        //detach from player
        if (!Physics2D.OverlapBox(b.center, b.extents * 3, 0, LayerMask.GetMask("Player")) && player.pulling)
        {
            StartCoroutine(GameObject.Find("Player").GetComponent<PlayerMovement>().StopPulling());
        }

        //respawn
        if (Physics2D.OverlapBox(b.center, b.extents * 2, 0, LayerMask.GetMask("Hazard")))
        {
            transform.position = spawnpoint;
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            StartCoroutine((GameObject.Find("Player").GetComponent<PlayerMovement>().StopPulling()));
        }

        //player on cart
        if (Physics2D.OverlapBox(transform.position, new Vector2(1.8f, 2), 0, LayerMask.GetMask("Player")))
        {
            if (!onCart)
            {
                onCart = true;
                player.jumpMultiplier = 1.3f;
                GetComponent<Animator>().Play("CartSquish");
            }
        }
        //player jump off
        else if (onCart)
        {
            onCart = false;
            player.jumpMultiplier = 1;
            GetComponent<Animator>().Play("CartUnsquish");
        }
    }
}
