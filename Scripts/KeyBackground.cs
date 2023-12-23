using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyBackground : MonoBehaviour
{
    [SerializeField] GameObject keybind;
    [SerializeField] bool textCentered = false;
    Vector2 coords;

    void Start()
    {
        coords = GetComponent<RectTransform>().anchoredPosition;
    }

    void Update()
    {
        transform.localScale = new Vector2((keybind.GetComponent<TMPro.TextMeshProUGUI>().text.Length+1)*0.5f, transform.localScale.y);
        if (!textCentered)
            GetComponent<RectTransform>().anchoredPosition = new Vector2(coords.x + 15*(transform.localScale.x-1), coords.y);
        if (keybind.transform.childCount > 0)
            keybind.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(45 + 38*(transform.localScale.x-1), 0);
    }
}
