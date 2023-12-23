//Author: Ian Stolte
//Date: 7/13/23
//Desc: A base class for any interactable object (can't attach directly to GameObjects)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    protected PlayerMovement player;

    void Update()
    {
        Bounds b = GetComponent<BoxCollider2D>().bounds;
        player = GameObject.Find("Player").GetComponent<PlayerMovement>();
        if (Physics2D.OverlapBox(b.center, b.extents*2, 0, LayerMask.GetMask("Player")) && !player.pulling && player.IsGrounded())
        {
            if (Input.GetKeyDown(GameObject.Find("Player").GetComponent<PlayerInfo>().binds[5]) && !player.freeze)
            {
                Interact();
            }
        }
    }

    public abstract void Interact();
}
