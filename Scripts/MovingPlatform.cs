//Author: Ian Stolte
//Date: 7/13/23
//Desc: Moves a platform (or other object) back and forth

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    enum directions
    {
        VERTICAL,
        HORIZONTAL
    }

    enum types
    {
        LINEAR,
        THREE_POINT
    }

    [Tooltip("Whether the platform moves horizontally or vertically (horizontal by default)")] [SerializeField] directions direction;
    [SerializeField] types movementType;

    [SerializeField] float speed;
    [SerializeField] float start;
    [SerializeField] float middle;
    [SerializeField] float end;
    [SerializeField] float pause;
    float pauseTimer;
    [Tooltip("Whether the player attaches to the platform (used for actual platforms, not used for ghost hover)")] [SerializeField] bool playerAttach;
    [Tooltip("Waits until player interacts with it to start moving")] [SerializeField] bool waitForPlayer;
    
    bool playerOn;
    Vector3 previousPos;

    PlayerMovement player;
    Vector3 startingPos;

    void Start()
    {
        if (start > end)
        {
            float temp = end;
            end = start;
            start = temp;
        }

        player = GameObject.Find("Player").GetComponent<PlayerMovement>();
        if (waitForPlayer)
        {
            player.deathEvent.AddListener(ResetWait);
            startingPos = transform.position;
        }
    }

    void FixedUpdate()
    {
        Bounds b = GetComponent<BoxCollider2D>().bounds;
        if ((player.OnPlatform() || player.climbing) && Physics2D.OverlapBox(b.center, b.extents*2.2f, 0, LayerMask.GetMask("Player")))
        {
            /*if (!playerOn)
            {
                playerOn = true;
                if (playerAttach)
                {

                    //GameObject.Find("Player").transform.SetParent(gameObject.transform);
                }
            }*/
            if (playerAttach)
            {
                GameObject.Find("Player").transform.position += transform.position - previousPos;
                waitForPlayer = false;
            }
        }
        previousPos = transform.position;
        /*else if (playerOn)
        {
            playerOn = false;
            GameObject.Find("Player").transform.SetParent(null);
        }*/

        if (!player.freeze && !waitForPlayer)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0)
            {
                if (direction == directions.VERTICAL)
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y + Time.deltaTime * speed, transform.position.z);
                    //middle
                    if (Mathf.Abs(transform.position.y-middle) < 0.05f && pauseTimer < -0.1f && movementType == types.THREE_POINT)
                    {
                        pauseTimer = pause;
                    }
                    else if (transform.position.y < start && pauseTimer < -0.1f)
                    {
                        pauseTimer = pause;
                        speed = Mathf.Abs(speed);

                    }
                    else if (transform.position.y > end && pauseTimer < -0.1f)
                    {
                        pauseTimer = pause;
                        speed = -Mathf.Abs(speed);
                    }
                }
                else
                {
                    transform.position = new Vector3(transform.position.x + Time.deltaTime * speed, transform.position.y, transform.position.z);
                    //middle
                    if (Mathf.Abs(transform.position.x-middle) < 0.05f && pauseTimer < -0.1f && movementType == types.THREE_POINT)
                    {
                        pauseTimer = pause;
                    }
                    else if (transform.position.x < start && pauseTimer < -0.1f)
                    {
                        pauseTimer = pause;
                        speed = Mathf.Abs(speed);
                    }
                    else if (transform.position.x > end && pauseTimer < -0.1f)
                    {
                        pauseTimer = pause;
                        speed = -Mathf.Abs(speed);
                    }
                }
            }
        }
    }
    private void OnDrawGizmos()
    {
        if (direction == directions.VERTICAL)
        {
            Gizmos.DrawLine(new Vector3(transform.position.x, start, transform.position.z), new Vector3(transform.position.x, end, transform.position.z));
        }
        else
        {
            Gizmos.DrawLine(new Vector3(start, transform.position.y, transform.position.z), new Vector3(end, transform.position.y, transform.position.z));
        }
    }

    void ResetWait()
    {
        StartCoroutine(ResetWaitCor());
    }
    IEnumerator ResetWaitCor()
    {
        yield return new WaitForSeconds(0.5f);
        transform.position = startingPos;
        waitForPlayer = true;
        speed = Mathf.Abs(speed);
    }
}
