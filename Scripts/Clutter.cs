using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clutter : InteractableObject
{
    bool activated;
    [SerializeField] GameObject bookPrefab;

    public override void Interact()
    {
        PlayerInfo playerInfo = GameObject.Find("Player").GetComponent<PlayerInfo>();
        if (!activated && GameObject.Find("Player").GetComponent<PlayerMovement>().canClimb)
        {
            activated = true;
            playerInfo.libraryCleanup++;
            GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Clutter");
            List<GameObject> disabledObjs = new List<GameObject>();
            foreach(Transform child in transform.parent)
            {
                if (!child.GetComponent<SpriteRenderer>().enabled)
                    disabledObjs.Add(child.gameObject);
            }
            if (disabledObjs.Count >= 3 && playerInfo.librarian_quest == PlayerInfo.librarian_questState.UNFINISHED)
            {
                GameObject objToEnable = disabledObjs[Random.Range(0, disabledObjs.Count)];
                objToEnable.GetComponent<SpriteRenderer>().enabled = true;
                objToEnable.GetComponent<Clutter>().activated = false;
            }
            GetComponent<SpriteRenderer>().enabled = false;
            if (playerInfo.libraryCleanup == 1 || playerInfo.libraryCleanup == 5)
            {
                playerInfo.showEllipses[4] = true;
            }
            if (Random.Range(0, 4) == 0) //25% chance
            {
                GameObject.Find("Player").GetComponent<PlayerInfo>().CollectItem("randomBook");
            }
        }

    }
}
