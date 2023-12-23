using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PanOnButton : MonoBehaviour
{
    [SerializeField] UnityEvent eventToPlay;

    bool activated;
    PlayerInfo player;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerInfo>();
    }

    void Update()
    {
        if (!activated && player.button)
        {
            activated = true;
            StartCoroutine(GameObject.Find("Main Camera").GetComponent<CameraFollow>().PanToPoint(transform.position, 1, eventToPlay));
        }
    }
}
