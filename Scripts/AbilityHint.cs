using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityHint : MonoBehaviour
{
    GameObject playerObj;
    PlayerMovement player;

    enum type
    {
        WALL_JUMP,
        DOUBLE_JUMP
    }

    [SerializeField] type ability;

    void Start()
    {
        playerObj = GameObject.Find("Player");
        if (playerObj != null)
            player = playerObj.GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (playerObj != null)
        {
            GetComponent<TMPro.TextMeshProUGUI>().text = "" + playerObj.GetComponent<PlayerInfo>().binds[4];

            if (ability == type.WALL_JUMP)
            {
                if (!player.canWallJump || player.freeze)
                {
                    transform.parent.GetComponent<CanvasGroup>().alpha = 0;
                }
                else if (player.OnWall() != 0 && !player.IsGrounded() && !player.climbing)
                {
                    transform.parent.GetComponent<CanvasGroup>().alpha = 1;
                    if (player.moveTimer > 0)
                    {
                        Destroy(transform.parent.GetChild(2).gameObject);
                        Destroy(transform.parent.GetChild(0).gameObject);
                        Destroy(gameObject);
                    }
                }
                else
                {
                    transform.parent.GetComponent<CanvasGroup>().alpha = 0.2f;
                }
            }
            else if (ability == type.DOUBLE_JUMP)
            {
                if (player.maxJumps < 1 || player.freeze)
                {
                    transform.parent.GetComponent<CanvasGroup>().alpha = 0;
                }
                else if (!player.IsGrounded() && !player.climbing)
                {
                    transform.parent.GetComponent<CanvasGroup>().alpha = 1;
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        Destroy(transform.parent.GetChild(2).gameObject);
                        Destroy(transform.parent.GetChild(0).gameObject);
                        Destroy(gameObject);
                    }
                }
                else
                {
                    transform.parent.GetComponent<CanvasGroup>().alpha = 0.2f;
                }
            }
        }

    }
}
