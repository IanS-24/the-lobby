//Author: Ian Stolte
//Date: 9/16/23
//Desc: Dialogue controller for the no-arm ghost

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NoArmDialogue : MonoBehaviour
{
    //[Header("Dialogue")]

    [Tooltip("Inital dialogue lines")] [SerializeField] List<DialogueObject> dialogue1 = new List<DialogueObject>();
    [Tooltip("Secondary dialogue lines")] [SerializeField] List<DialogueObject> dialogue2 = new List<DialogueObject>();

    PlayerInfo playerInfo;
    PlayerMovement playerMovement;
    DialogueManager dialogue;
    AudioManager audioManager;

    void Start()
    {
        playerMovement = GameObject.Find("Player").GetComponent<PlayerMovement>();
        playerInfo = GameObject.Find("Player").GetComponent<PlayerInfo>();
        dialogue = GameObject.Find("Dialogue Manager").GetComponent<DialogueManager>();
        audioManager = GameObject.Find("Audio Manager").GetComponent<AudioManager>();
    }

    void Update()
    {
        Bounds b = GetComponent<BoxCollider2D>().bounds;
        if (playerMovement.freeze)
        {
            foreach (Transform child in transform) {
                if (child.gameObject.name != "Light 2D")
                    child.gameObject.SetActive(false);
            }
        }
        else if (Physics2D.OverlapBox(b.center, b.extents * 2, 0, LayerMask.GetMask("Player")) && !playerMovement.pulling && playerMovement.IsGrounded())
        {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(false);
            if (!playerInfo.ghostsMet[3])
            {
                transform.GetChild(2).gameObject.SetActive(true);
                transform.GetChild(3).gameObject.SetActive(false);
            }
            else if (playerInfo.showEllipses[3])
            {
                transform.GetChild(3).gameObject.SetActive(true);
            }
            if (Input.GetKeyDown(playerInfo.binds[5]))
            {
                StartCoroutine(Interact());
            }
        }
        else
        {
            transform.GetChild(2).gameObject.SetActive(false);
            transform.GetChild(3).gameObject.SetActive(false);
            if (!playerInfo.ghostsMet[3])
            {
                transform.GetChild(0).gameObject.SetActive(true);
                transform.GetChild(1).gameObject.SetActive(false);
            }
            else
            {
                transform.GetChild(0).gameObject.SetActive(false);
                if (playerInfo.showEllipses[3])
                    transform.GetChild(1).gameObject.SetActive(true);
            }
        }

        //audio fade
        /*float distance = Vector2.Distance(transform.position, GameObject.Find("Player").transform.position);
        Sound s = Array.Find(audioManager.music, sound => sound.name == "NoArm Theme");
        if (distance < 20)    
            s.source.volume = 0.02f * (20 - distance);
        else
           s.source.volume = 0;*/
    }

    public IEnumerator Interact()
    {
        //StartCoroutine(audioManager.StartFade("___", 2, 0.2f));
        if (!playerInfo.ghostsMet[3])
        {
            StartCoroutine(dialogue.PlayDialogue(dialogue1));
            playerInfo.showEllipses[3] = true;
        }
        else
        {
            StartCoroutine(dialogue.PlayDialogue(dialogue2));
            playerInfo.showEllipses[3] = false;
        }
        playerInfo.ghostsMet[3] = true;
        yield return new WaitUntil(() => !playerMovement.freeze);
        //StartCoroutine(audioManager.StartFade("____", 2, 0.4f));
    }
}
