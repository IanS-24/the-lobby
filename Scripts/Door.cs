using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : InteractableObject
{
    public override void Interact()
    {
        gameObject.tag = "Untagged";
        GetComponent<EdgeCollider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("DoorOpen");
        //play door open animation
    }
}
