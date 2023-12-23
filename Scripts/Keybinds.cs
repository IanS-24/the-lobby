using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Keybinds : MonoBehaviour
{
    [SerializeField] GameObject menuKeybind;
    public bool doNotExit;
    bool rebound = false;
    float inputTimer = 0.1f;

    KeyCode currentKey;
    
    PlayerInfo player;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerInfo>();
    }

    void Update()
    {
        inputTimer -= Time.deltaTime;
        Transform menuText = menuKeybind.transform.GetChild(1);
        if (GameObject.Find("Level Select").GetComponent<LevelSelect>().paused || SceneManager.GetActiveScene().name == "Main Menu" ||
            GameObject.Find("Player").GetComponent<PlayerMovement>().freeze) {
            menuKeybind.SetActive(false);
        }
        else if (player.binds[6] == KeyCode.Escape) {
            menuKeybind.SetActive(true);
            menuText.GetComponent<TMPro.TextMeshProUGUI>().text = "Esc";
        }
        else {
            menuKeybind.SetActive(true);
            menuText.GetComponent<TMPro.TextMeshProUGUI>().text = "" + player.binds[6];
        }
    }
    
    void OnGUI()
    {
        Event e = Event.current;
        if (e.isKey && e.keyCode != KeyCode.None && rebound == false && e.keyCode != KeyCode.RightBracket)
        {
            currentKey = e.keyCode;
            rebound = true;
        }
    }

    //method to call the coroutine (since we can't do it from the button directly)
    public void Rebind(GameObject txt)
    {
        rebound = true;
        //while (inputTimer > 0) { }
        //inputTimer = 1f;
        StartCoroutine(RebindLoop(txt));
    }

    public IEnumerator RebindLoop(GameObject txt)
    {
        doNotExit = true;
        //yield return new WaitForSeconds(0.1f);
        currentKey = player.binds[int.Parse(txt.name.Substring(txt.name.Length - 1))];
        rebound = false;
        txt.GetComponent<TMPro.TextMeshProUGUI>().text = "_";
        
        yield return new WaitUntil(() => rebound == true);

        EventSystem.current.SetSelectedGameObject(null);
        txt.GetComponent<TMPro.TextMeshProUGUI>().text = "" + currentKey;
        int index = int.Parse(txt.name.Substring(txt.name.Length-1)); //get the last character in the object's name
        for (int i = 0; i < player.binds.Length; i++)
        {
            if (player.binds[i] == currentKey && i != index)
            {
                player.binds[i] = KeyCode.None;
                GameObject.Find("Keybind " + i).GetComponent<TMPro.TextMeshProUGUI>().text = "_";
            }
        }
        player.binds[index] = currentKey;
        doNotExit = false;
    }
}