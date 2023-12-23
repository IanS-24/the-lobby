//Author: Ian Stolte
//Date: 8/28/23
//Desc: Dialogue controller for the king npc

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class KingDialogue : MonoBehaviour
{
    //[Header("Dialogue")]

    //[Tooltip("Dialogue lines on scene load")] [SerializeField] List<DialogueObject> startDialogue = new List<DialogueObject>();
    [Tooltip("Inital dialogue lines")] [SerializeField] List<DialogueObject> dialogue1 = new List<DialogueObject>();
    [Tooltip("Secondary dialogue lines")] [SerializeField] List<DialogueObject> dialogue2 = new List<DialogueObject>();
    //[Tooltip("Dialogue lines if button is collected")] [SerializeField] List<DialogueObject> buttonCollected = new List<DialogueObject>();
    [Tooltip("One-time lines if sword is found")] [SerializeField] List<DialogueObject> swordCollected = new List<DialogueObject>();
    //[Tooltip("Repeatable lines if sword is found")] [SerializeField] List<DialogueObject> swordCollected2 = new List<DialogueObject>();
    [Tooltip("One-time lines if knight met")] [SerializeField] List<DialogueObject> knightMet1 = new List<DialogueObject>();
    [Tooltip("Repeatable lines if knight met")] [SerializeField] List<DialogueObject> knightMet2 = new List<DialogueObject>();
    //[Tooltip("Dialogue lines if quest complete (both king met and sword found)")] [SerializeField] List<DialogueObject> questComplete = new List<DialogueObject>();

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
        
        playerInfo.showEllipses[2] = true;
        if (playerInfo.knight_questFinished)
            GetComponent<SpriteRenderer>().enabled = false;
           
    }

    void Update()
    {
        Bounds b = GetComponent<BoxCollider2D>().bounds;
        if (playerMovement.freeze || playerInfo.knight_questFinished)
        {
            foreach (Transform child in transform) {
                if (child.gameObject.name != "Light 2D")
                    child.gameObject.SetActive(false);
            }
        }
        else if (Physics2D.OverlapBox(b.center, b.extents * 2, 0, LayerMask.GetMask("Player")) && !playerMovement.pulling && playerMovement.IsGrounded())
        {
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(2).gameObject.SetActive(false);
            if (playerInfo.ghostsMet[1] && !playerInfo.king_knightDialoguePlayed)
            {
                transform.GetChild(3).gameObject.SetActive(true);
                transform.GetChild(4).gameObject.SetActive(false);
            }
            else if (playerInfo.showEllipses[2])
            {
                transform.GetChild(4).gameObject.SetActive(true);
            }
            if (Input.GetKeyDown(playerInfo.binds[5]))
            {
                StartCoroutine(Interact());
            }
        }
        else
        {
            transform.GetChild(3).gameObject.SetActive(false);
            transform.GetChild(4).gameObject.SetActive(false);
            if (playerInfo.ghostsMet[1] && !playerInfo.king_knightDialoguePlayed)
            {
                transform.GetChild(1).gameObject.SetActive(true);
                transform.GetChild(2).gameObject.SetActive(false);
            }
            else
            {
                transform.GetChild(1).gameObject.SetActive(false);
                if (playerInfo.showEllipses[2])
                    transform.GetChild(2).gameObject.SetActive(true);
            }
        }

        //audio fade
        float distance = Vector2.Distance(transform.position, GameObject.Find("Player").transform.position);
        Sound s = Array.Find(audioManager.music, sound => sound.name == "King Theme");
        if (distance < 20)    
            s.source.volume = 0.02f * (20 - distance);
        else
           s.source.volume = 0;
    }

    public IEnumerator Interact()
    {
        StartCoroutine(audioManager.StartFade("Beyond the Veil", 2, 0.2f));
        if (playerInfo.ghostsMet[1] && !playerInfo.king_knightDialoguePlayed)
        {
            StartCoroutine(dialogue.PlayDialogue(knightMet1));
            playerInfo.king_knightDialoguePlayed = true;
            playerInfo.showEllipses[2] = true;
        }
        else if (playerInfo.FindItem("sword").collected)
        {
            StartCoroutine(dialogue.PlayDialogue(swordCollected));
            playerInfo.showEllipses[2] = false;
        }
        else if (!playerInfo.ghostsMet[2]) //first meeting
        {
            StartCoroutine(dialogue.PlayDialogue(dialogue1));
            playerInfo.showEllipses[2] = true;
        }
        else if (playerInfo.ghostsMet[1] && playerInfo.king_knightDialoguePlayed)
        {
            StartCoroutine(dialogue.PlayDialogue(knightMet2));
            playerInfo.showEllipses[2] = false;
        }
        else
        {
            StartCoroutine(dialogue.PlayDialogue(dialogue2));
            playerInfo.showEllipses[2] = false;
        }
        playerInfo.ghostsMet[2] = true;
        yield return new WaitUntil(() => !playerMovement.freeze);
        StartCoroutine(audioManager.StartFade("Beyond the Veil", 2, 0.4f));
    }
}
