//Author: Ian Stolte
//Date: 8/26/23
//Desc: Dialogue controller for the knight npc

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class KnightDialogue : MonoBehaviour
{
    //[Header("Dialogue")]

    //[Tooltip("Dialogue lines on scene load")] [SerializeField] List<DialogueObject> startDialogue = new List<DialogueObject>();
    [Tooltip("Inital dialogue lines")] [SerializeField] List<DialogueObject> dialogue1 = new List<DialogueObject>();
    [Tooltip("Secondary dialogue lines")] [SerializeField] List<DialogueObject> dialogue2 = new List<DialogueObject>();
    [Tooltip("Dialogue lines if button is collected")] [SerializeField] List<DialogueObject> buttonCollected = new List<DialogueObject>();
    [Tooltip("One-time lines if sword is found")] [SerializeField] List<DialogueObject> swordCollected1 = new List<DialogueObject>();
    [Tooltip("Repeatable lines if sword is found")] [SerializeField] List<DialogueObject> swordCollected2 = new List<DialogueObject>();
    [Tooltip("One-time lines if king met")] [SerializeField] List<DialogueObject> kingMet1 = new List<DialogueObject>();
    [Tooltip("Repeatable lines if king met")] [SerializeField] List<DialogueObject> kingMet2 = new List<DialogueObject>();
    [Tooltip("Dialogue lines if quest complete (both king met and sword found)")] [SerializeField] List<DialogueObject> questComplete = new List<DialogueObject>();

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

        if (playerInfo.knight_questFinished){
            GetComponent<SpriteRenderer>().enabled = false;
            GameObject.Find("Sword").SetActive(false);
        }   
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
            if ((playerInfo.ghostsMet[2] && !playerInfo.knight_kingDialoguePlayed) || (playerInfo.FindItem("sword").collected && !playerInfo.knight_swordDialoguePlayed)
            || !playerInfo.ghostsMet[1] || (playerInfo.ghostsMet[2] && playerInfo.FindItem("sword").collected))
            {
                transform.GetChild(3).gameObject.SetActive(true);
                transform.GetChild(4).gameObject.SetActive(false);
            }
            else if (playerInfo.showEllipses[1] || (playerInfo.button && !playerInfo.knight_buttonDialoguePlayed))
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
            if ((playerInfo.ghostsMet[2] && !playerInfo.knight_kingDialoguePlayed) || (playerInfo.FindItem("sword").collected && !playerInfo.knight_swordDialoguePlayed) ||
            !playerInfo.ghostsMet[1] || (playerInfo.ghostsMet[2] && playerInfo.FindItem("sword").collected))
            {
                transform.GetChild(1).gameObject.SetActive(true);
                transform.GetChild(2).gameObject.SetActive(false);
            }
            else
            {
                transform.GetChild(1).gameObject.SetActive(false);
                if (playerInfo.showEllipses[1] || (playerInfo.button && !playerInfo.knight_buttonDialoguePlayed))
                    transform.GetChild(2).gameObject.SetActive(true);
            }
        }

        //audio fade
        float distance = Vector2.Distance(transform.position, GameObject.Find("Player").transform.position);
        Sound s = Array.Find(audioManager.music, sound => sound.name == "Knight Theme");
        if (distance < 20)    
            s.source.volume = 0.02f * (20 - distance);
        else
           s.source.volume = 0;
    }

    public IEnumerator Interact()
    {
        StartCoroutine(audioManager.StartFade("Beyond the Veil", 2, 0.2f));
        if (playerInfo.ghostsMet[2] && playerInfo.FindItem("sword").collected)
        {
            StartCoroutine(dialogue.PlayDialogue(questComplete));
            StartCoroutine(Disappear());
        }
        else if (playerInfo.ghostsMet[2] && !playerInfo.knight_kingDialoguePlayed)
        {
            StartCoroutine(dialogue.PlayDialogue(kingMet1));
            playerInfo.knight_kingDialoguePlayed = true;
            playerInfo.showEllipses[1] = true;
        }
        else if (playerInfo.FindItem("sword").collected && !playerInfo.knight_swordDialoguePlayed)
        {
            StartCoroutine(dialogue.PlayDialogue(swordCollected1));
            playerInfo.knight_swordDialoguePlayed = true;
            playerInfo.showEllipses[1] = true;
        }
        else if (!playerInfo.ghostsMet[1])
        {
            StartCoroutine(dialogue.PlayDialogue(dialogue1));
            playerInfo.showEllipses[1] = true;
            //StartCoroutine(AbilityUnlock());
        }
        else if (playerInfo.button && !playerInfo.knight_buttonDialoguePlayed) //get rid of? --- hides ellipses even if swordCollected2 is new dialogue (also breaks the flow of conversation)
        {
            playerInfo.knight_buttonDialoguePlayed = true;
            StartCoroutine(dialogue.PlayDialogue(buttonCollected));
            playerInfo.showEllipses[1] = false;
        }
        else if (playerInfo.ghostsMet[2] && playerInfo.knight_kingDialoguePlayed)
        {
            StartCoroutine(dialogue.PlayDialogue(kingMet2));
            playerInfo.showEllipses[1] = false;
        }
        else if (playerInfo.FindItem("sword").collected && playerInfo.knight_swordDialoguePlayed)
        {
            StartCoroutine(dialogue.PlayDialogue(swordCollected2));
            playerInfo.showEllipses[1] = false;
        }
        else
        {
            StartCoroutine(dialogue.PlayDialogue(dialogue2));
            playerInfo.showEllipses[1] = false;
        }
        playerInfo.ghostsMet[1] = true;
        yield return new WaitUntil(() => !playerMovement.freeze);
        StartCoroutine(audioManager.StartFade("Beyond the Veil", 2, 0.4f));
    }

    IEnumerator Disappear()
    {
        playerInfo.knight_questFinished = true;
        yield return new WaitUntil(() => !playerMovement.freeze);
        GameObject.Find("Fader").GetComponent<Animator>().Play("FadeOut");
        yield return new WaitForSeconds(0.5f);
        GetComponent<SpriteRenderer>().enabled = false;
        GameObject.Find("Fader").GetComponent<Animator>().Play("FadeIn");
    }
}