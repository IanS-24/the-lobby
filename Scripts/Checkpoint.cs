using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] bool silent;
    bool unlocked;
    
    void Update()
    {
        Bounds b = GetComponent<BoxCollider2D>().bounds;
        if (Physics2D.OverlapBox(b.center, b.extents * 2, 0, LayerMask.GetMask("Player")))
        {
            //Set checkpoint
            if (GameObject.Find("Player").GetComponent<PlayerMovement>().checkpoint != transform.position)
            {
                if (!silent)
                    GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Checkpoint");
                GetComponent<Animator>().Play("SpiritOrbOn");
                GameObject.Find("Player").GetComponent<PlayerMovement>().checkpoint = transform.position;
            }
            //Save button
            if (GameObject.Find("Player").GetComponent<PlayerInfo>().button)
            {
                GameObject.Find("Player").GetComponent<PlayerInfo>().buttonSaved = true;
            }
        }
        else if (GameObject.Find("Player").GetComponent<PlayerMovement>().checkpoint != transform.position)
        {
            GetComponent<Animator>().Play("SpiritOrbOff");
        }
    }
}
