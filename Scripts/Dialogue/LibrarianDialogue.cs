//Author: Ian Stolte
//Date: 10/11/23
//Desc: Dialogue controller for the librarian npc

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LibrarianDialogue : MonoBehaviour
{
    [Tooltip("Plays when you first speak to the librarian")] [SerializeField] List<DialogueObject> quest1 = new List<DialogueObject>();
    [Tooltip("Plays if you talk to the librarian again")] [SerializeField] List<DialogueObject> quest2 = new List<DialogueObject>();
    [Tooltip("Plays if you choose evey dialogue option in quest2")] [SerializeField] List<DialogueObject> quest3 = new List<DialogueObject>();
    [Tooltip("Plays if you have picked up an important book")] [SerializeField] List<DialogueObject> bookCollected = new List<DialogueObject>();
    [Tooltip("Plays if you've collected all 4 random books'")] [SerializeField] List<DialogueObject> allRandomBooks = new List<DialogueObject>();
    [Tooltip("Plays when player approaches a bookshelf for the first time")] [SerializeField] List<DialogueObject> climbingHint = new List<DialogueObject>();
    [Tooltip("Plays if you've cleaned up 3 things or less")] [SerializeField] List<DialogueObject> startedCleanup = new List<DialogueObject>();
    [Tooltip("Plays if you've cleaned up 4-6 things")] [SerializeField] List<DialogueObject> continuingCleanup = new List<DialogueObject>();
    [Tooltip("Plays if you've cleaned up 7+ things")] [SerializeField] List<DialogueObject> givingUp = new List<DialogueObject>();

    PlayerInfo playerInfo;
    PlayerMovement playerMovement;
    DialogueManager dialogue;
    AudioManager audioManager;

    bool climbHintGiven;

    void Start()
    {
        playerMovement = GameObject.Find("Player").GetComponent<PlayerMovement>();
        playerInfo = GameObject.Find("Player").GetComponent<PlayerInfo>();
        dialogue = GameObject.Find("Dialogue Manager").GetComponent<DialogueManager>();
        audioManager = GameObject.Find("Audio Manager").GetComponent<AudioManager>();

        if (playerInfo.librarian_quest == PlayerInfo.librarian_questState.DISAPPEAR){
            GetComponent<SpriteRenderer>().enabled = false;
        }  
    }

    void Update()
    {
        if (!climbHintGiven && playerMovement.canClimb && Physics2D.OverlapBox(GameObject.Find("Player").transform.position, new Vector2(2, 4), 0, LayerMask.GetMask("Climbable")))
        {
            climbHintGiven = true;
            StartCoroutine(dialogue.PlayDialogue(climbingHint));
        }

        Bounds b = GetComponent<BoxCollider2D>().bounds;
        if (playerMovement.freeze || playerInfo.librarian_quest != PlayerInfo.librarian_questState.UNFINISHED)
        {
            foreach (Transform child in transform) {
                if (child.gameObject.name != "Light 2D") 
                    child.gameObject.SetActive(false);
            }
        }
        else
        {
            int n = 2;
            if (Physics2D.OverlapBox(b.center, b.extents * 2, 0, LayerMask.GetMask("Player")) && !playerMovement.pulling && playerMovement.IsGrounded())
            {
                n = 0;
                if (Input.GetKeyDown(playerInfo.binds[5]))
                {
                    StartCoroutine(Interact());
                }
            }
            transform.GetChild(1+n).gameObject.SetActive(false);
            transform.GetChild(2+n).gameObject.SetActive(false);
            if (!playerInfo.ghostsMet[4] || playerInfo.libraryCleanup >= 9)
            {
                transform.GetChild(3-n).gameObject.SetActive(true);
                transform.GetChild(4-n).gameObject.SetActive(false);
            }
            else if (playerInfo.showEllipses[4])
            {
                transform.GetChild(4-n).gameObject.SetActive(true);
            }
        }

        //audio fade
        if (playerInfo.librarian_quest != PlayerInfo.librarian_questState.DISAPPEAR)
        {
            float distance = Vector2.Distance(transform.position, GameObject.Find("Player").transform.position);
            Sound s = Array.Find(audioManager.music, sound => sound.name == "Librarian Theme");
            if (distance < 20)    
                s.source.volume = 0.015f * (20 - distance);
            else
            s.source.volume = 0;
        }
    }

    public IEnumerator Interact()
    {
        StartCoroutine(audioManager.StartFade("Library Theme (melody)", 2, 0));
        if (!playerInfo.ghostsMet[4])
        {
            StartCoroutine(dialogue.PlayDialogue(quest1));
            playerInfo.showEllipses[4] = true;
            GameObject[] clutter = GameObject.FindGameObjectsWithTag("Clutter");
            foreach (GameObject obj in clutter)
            {
                obj.tag = "Interactable";
            }
            playerMovement.canClimb = true;
        }
        else if (playerInfo.libraryCleanup == 0)
        {
            if (quest2.Count > 4)
                StartCoroutine(dialogue.PlayDialogue(quest2));
            else
                StartCoroutine(dialogue.PlayDialogue(quest3));
            playerInfo.showEllipses[4] = false;
        }
        else if (playerInfo.libraryCleanup < 5)
        {
            StartCoroutine(dialogue.PlayDialogue(startedCleanup));
            playerInfo.showEllipses[4] = false;
        }
        else if (playerInfo.libraryCleanup < 9)
        {
            StartCoroutine(dialogue.PlayDialogue(continuingCleanup));
            playerInfo.showEllipses[4] = false;
        }
        else {
            StartCoroutine(dialogue.PlayDialogue(givingUp));
            playerInfo.librarian_quest = PlayerInfo.librarian_questState.DISAPPEAR;
            GameObject.Find("Big Clutter").SetActive(false);

            audioManager.Stop("Library Theme (melody)");
            StartCoroutine(audioManager.StartFade("Library Theme (background)", 10, 0));
            StartCoroutine(audioManager.StartFade("Librarian Theme", 10, 0.25f));
            StartCoroutine(audioManager.StartFade("Library Theme (quiet)", 10, 0.4f));
        }
        playerInfo.ghostsMet[4] = true;
        yield return new WaitUntil(() => !playerMovement.freeze);
        if (playerInfo.librarian_quest == PlayerInfo.librarian_questState.UNFINISHED && (playerInfo.FindItem("childhoodBook").collected || playerInfo.FindItem("schoolBook").collected || playerInfo.FindItem("sisterBook").collected) && !playerInfo.librarian_bookDialoguePlayed)
        {
            playerInfo.librarian_bookDialoguePlayed = true;
            if (playerInfo.FindItem("childhoodBook").collected)
                BookResponse(playerInfo.FindItem("childhoodBook").name);
            else if (playerInfo.FindItem("schoolBook").collected)
                BookResponse(playerInfo.FindItem("schoolBook").name);
            else if (playerInfo.FindItem("sisterBook").collected)
                BookResponse(playerInfo.FindItem("sisterBook").name);
        }
        yield return new WaitUntil(() => !playerMovement.freeze);
        StartCoroutine(audioManager.StartFade("Library Theme (melody)", 2, 0.05f));
    }

    void BookResponse(string title)
    {
        DialogueObject newLine = new DialogueObject("That book you have:** '" + title + "'? **I think I read that when I was alive!", DialogueObject.dialogueFrame.LIBRARIAN);
        bookCollected[1] = newLine;
        StartCoroutine(dialogue.PlayDialogue(bookCollected));
    }

    public void EndingMusic()
    {
        StartCoroutine(audioManager.StartFade("Library Theme (quiet)", 4, 0));
    }

    public void StayEnding()
    {
        StartCoroutine(audioManager.StartFade("Library Theme (quiet)", 4, 0.3f));
        playerInfo.librarian_quest = PlayerInfo.librarian_questState.STAY;
    }

    public void CutLines(int index)
    {
        if (index == 1) {
            StartCoroutine(CutLinesCor("What's your story?", "Nowadays I don't get many visitors, but at least I have the books...", quest2));
        }
        else if (index == 2) {
            StartCoroutine(CutLinesCor("What's your favorite book?", "Maybe**.**.**.**** no. ***It's just too hard to pick.", quest2));
        }
        else if (index == 3) {
            StartCoroutine(CutLinesCor("This place is strange...", "I guess it's good there wasn't anyone asking for recommendations.", quest2));
        }
    }

    IEnumerator CutLinesCor(string start, string end, List<DialogueObject> list)
    {
        yield return new WaitUntil(() => !dialogue.playing);
        bool cutting = false;
        List<DialogueObject> toRemove = new List<DialogueObject>();
        foreach (DialogueObject obj in list)
        {
            if (obj.line == start) {
                toRemove.Add(obj);
                cutting = true;
            }
            else if (obj.line == end) {
                toRemove.Add(obj);
                break;
            }
            else if (cutting == true) {
                toRemove.Add(obj);
            }
        }
        foreach (DialogueObject obj in toRemove)
        {
            list.Remove(obj);
        }
    }

    public void Disappear()
    {
        StartCoroutine(audioManager.StartFade("Librarian Theme", 1, 0));
        StartCoroutine(DisappearCor());
    }

    IEnumerator DisappearCor()
    {
        yield return new WaitUntil(() => !playerMovement.freeze);
        GameObject.Find("Fader").GetComponent<Animator>().Play("FadeOut");
        yield return new WaitForSeconds(0.5f);
        GetComponent<SpriteRenderer>().enabled = false;
        GameObject bookshelves = GameObject.Find("Bookshelves");
        foreach (Transform obj in bookshelves.transform)
        {
            obj.gameObject.SetActive(false);
        }
        GameObject.Find("Fader").GetComponent<Animator>().Play("FadeIn");
    }
}