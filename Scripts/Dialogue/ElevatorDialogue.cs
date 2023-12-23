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

public class ElevatorDialogue : MonoBehaviour
{
    //[Header("Dialogue")]

    [Tooltip("One-time lines on scene load")] [SerializeField] List<DialogueObject> startDialogue = new List<DialogueObject>();
    [Tooltip("Repeatable lines on scene load")] [SerializeField] List<DialogueObject> startDialogue2 = new List<DialogueObject>();
    [Tooltip("Dialogue lines if replaying the level")] [SerializeField] List<DialogueObject> replayDialogue = new List<DialogueObject>();
    [Tooltip("Inital dialogue lines")] [SerializeField] List<DialogueObject> dialogue1 = new List<DialogueObject>();
    [Tooltip("Dialogue lines if button is collected")] [SerializeField] List<DialogueObject> buttonCollected = new List<DialogueObject>();

    PlayerInfo playerInfo;
    PlayerMovement playerMovement;
    DialogueManager dialogue;
    AudioManager audioManager;
    LevelSelect levelSelect;

    void Start()
    {
        playerMovement = GameObject.Find("Player").GetComponent<PlayerMovement>();
        playerInfo = GameObject.Find("Player").GetComponent<PlayerInfo>();
        dialogue = GameObject.Find("Dialogue Manager").GetComponent<DialogueManager>();
        audioManager = GameObject.Find("Audio Manager").GetComponent<AudioManager>();
        levelSelect = GameObject.Find("Level Select").GetComponent<LevelSelect>();

        if (!levelSelect.unlocked[levelSelect.currentLevel])
        {
            StartCoroutine(dialogue.PlayDialogue(startDialogue));
            playerInfo.showEllipses[0]  = true;
        }
        else
        {
            StartCoroutine(dialogue.PlayDialogue(startDialogue2));
            playerInfo.showEllipses[0]  = true;
        }
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
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(2).gameObject.SetActive(false);
            if (playerInfo.button)
            {
                transform.GetChild(3).gameObject.SetActive(true);
                transform.GetChild(4).gameObject.SetActive(false);
            }
            else if (playerInfo.showEllipses[0])
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
            if (playerInfo.button)
            {
                transform.GetChild(1).gameObject.SetActive(true);
                transform.GetChild(2).gameObject.SetActive(false);
            }
            else
            {
                transform.GetChild(1).gameObject.SetActive(false);
                if (playerInfo.showEllipses[0])
                    transform.GetChild(2).gameObject.SetActive(true);
            }
        }

        //audio fade
        float distance = Vector2.Distance(transform.position, GameObject.Find("Player").transform.position);
        Sound s = Array.Find(audioManager.music, sound => sound.name == "Elevator Theme (" + audioManager.currentSong + ")");
        if (s != null) {
            if (distance < 20)    
                s.source.volume = 0.02f * (20 - distance);
            else
            s.source.volume = 0;
        }
        else {
            //Debug.Log(audioManager.currentSong);
        }
    }

    public IEnumerator Interact()
    {
        StartCoroutine(audioManager.StartFade(audioManager.currentSong, 2, 0.2f));
        if (playerInfo.button)
        {
            StartCoroutine(dialogue.PlayDialogue(buttonCollected));
            yield return new WaitUntil(() => !playerMovement.freeze);
            levelSelect.ChangeScene(levelSelect.currentLevel+1);
        }
        else if (levelSelect.unlocked[levelSelect.currentLevel])
        {
            StartCoroutine(dialogue.PlayDialogue(replayDialogue));
            playerInfo.showEllipses[0] = false;
        }
        else
        {
            StartCoroutine(dialogue.PlayDialogue(dialogue1));
            playerInfo.showEllipses[0] = false;
        }
        playerInfo.ghostsMet[0] = true;
        yield return new WaitUntil(() => !playerMovement.freeze);
        if (!playerInfo.button)
            StartCoroutine(audioManager.StartFade(audioManager.currentSong, 2, 0.4f));
    }
}