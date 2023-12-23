using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBox : MonoBehaviour
{
    public Quaternion startingRotation;
    bool activated = false;
    PlayerMovement player;

    void Start()
    {
        transform.rotation = startingRotation;
        player = GameObject.Find("Player").GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (player.freeze)
        {
            if (!activated)
            {
                StartCoroutine(RotateBack());
            }
        }
        else {
            activated = false;
        }
    }

    IEnumerator RotateBack()
    {
        activated = true;
        if (startingRotation.eulerAngles.z == 90 || startingRotation.eulerAngles.z == 270) {
            gameObject.layer = LayerMask.NameToLayer("RotateBoxGround");
        }
        else {
            gameObject.layer = LayerMask.NameToLayer("RotateBoxWall");
        }
        yield return new WaitForSeconds(0.5f);
        transform.rotation = startingRotation;
    }
}
