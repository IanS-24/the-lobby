using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassThrough : MonoBehaviour
{
    void Update()
    {
        GameObject player = GameObject.Find("Player");
        Bounds b = player.transform.GetChild(0).GetComponent<BoxCollider2D>().bounds;
        
        if ((player.GetComponent<Rigidbody2D>().velocity.y >= -0.01 && !player.GetComponent<PlayerMovement>().OnPlatform()
            && !Physics2D.OverlapBox(b.center, b.extents*2, 0, LayerMask.GetMask("Stairs"))) || player.GetComponent<PlayerMovement>().climbing ||
            player.GetComponent<PlayerMovement>().moveTimer > 0)
        {
            GetComponent<EdgeCollider2D>().enabled = false;
        }
        else
        {
            GetComponent<EdgeCollider2D>().enabled = true;
        }
    }
}
