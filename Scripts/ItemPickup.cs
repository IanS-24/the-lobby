using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] string itemName;

    void Update()
    {
        Bounds b = GetComponent<BoxCollider2D>().bounds;
        if (Physics2D.OverlapBox(b.center, b.extents * 2, 0, LayerMask.GetMask("Player")))
        {
            GameObject.Find("Player").GetComponent<PlayerInfo>().CollectItem(itemName);
            gameObject.SetActive(false);
        }
    }
}
