using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractText : MonoBehaviour
{
    [SerializeField] GameObject rectangle;
    bool showText;

    void Update()
    {
        GameObject player = GameObject.Find("Player");

        GetComponent<TMPro.TextMeshProUGUI>().text = "" + player.GetComponent<PlayerInfo>().binds[5];

        showText = false;
        GameObject[] interactObjs = GameObject.FindGameObjectsWithTag("Interactable");
        foreach (GameObject obj in interactObjs)
        {
            Bounds b = obj.GetComponent<BoxCollider2D>().bounds;
            if (Physics2D.OverlapBox(b.center, b.extents * 2, 0, LayerMask.GetMask("Player")) && obj.GetComponent<SpriteRenderer>().enabled)
                showText = true;
        }

        if (showText && !player.GetComponent<PlayerMovement>().pulling && player.GetComponent<PlayerMovement>().IsGrounded() &&
            !player.GetComponent<PlayerMovement>().freeze)
        {
            GetComponent<TMPro.TextMeshProUGUI>().enabled = true;
            rectangle.SetActive(true);
            transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            GetComponent<TMPro.TextMeshProUGUI>().enabled = false;
            rectangle.SetActive(false);
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}
