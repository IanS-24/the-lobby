//Author: Ian Stolte
//Date: 7/13/23
//Desc: Script for the laundry cart

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pullable : InteractableObject
{
    public override void Interact()
    {
        if (!player.pulling && player.canPull)
        {
            StartCoroutine(Pull());
        }

        IEnumerator Pull()
        {
            GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("CartPush");
            player.pullObject = gameObject;
            if (GameObject.Find("Player").transform.position.x > transform.position.x)
            {
                transform.position += new Vector3(-0.1f, 0, 0);
            }
            else
            {
                transform.position += new Vector3(0.1f, 0, 0);
            }
            yield return new WaitForSeconds(0.1f);
            player.pulling = true;
        }
    }
}