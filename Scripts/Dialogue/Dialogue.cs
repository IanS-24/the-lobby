//Author: Ian Stolte
//Date: 7/13/23
//Desc: Will play dialogue when you interact with this object

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

public class Dialogue : InteractableObject
{
    [Header("Dialogue")]

    [Tooltip("Dialogue lines on scene load")] [SerializeField] List<DialogueObject> startDialogue= new List<DialogueObject>();
    [Tooltip("Inital dialogue lines")] [SerializeField] List<DialogueObject> dialogue1 = new List<DialogueObject>();
    [Tooltip("Secondary dialogue lines")] [SerializeField] List<DialogueObject> dialogue2 = new List<DialogueObject>();
    [Tooltip("Dialogue lines after button collected / quest complete")] [SerializeField] List<DialogueObject> successDialogue = new List<DialogueObject>();

    bool firstMeeting = true;

    [Header("Attributes")]

    //[SerializeField] int nextLevel;
    [SerializeField] GameObject[] questItems;
    [SerializeField] UnityEvent introductionEvent;
    [SerializeField] UnityEvent successEvent;

    public enum unlockAbility
    {
        NONE,
        WALL_JUMP,
        DOUBLE_JUMP,
        DASH
    }

    //[Tooltip("The ability to unlock (if any)")] [SerializeField] unlockAbility unlock;

    private void Start()
    {
        if (startDialogue.Count > 0)
        {
            StartCoroutine(GameObject.Find("Dialogue Manager").GetComponent<DialogueManager>().PlayDialogue(startDialogue));
        }
    }

    public override void Interact()
    {
        //if quest complete
        bool questComplete = false;
        if (questItems.Length > 0)
            questComplete = true;
        foreach (GameObject obj in questItems)
        {
            if (obj.activeSelf && obj.GetComponent<SpriteRenderer>().enabled)
            {
                questComplete = false;
            }
        }
        if (questComplete)
        {
            StartCoroutine(GameObject.Find("Dialogue Manager").GetComponent<DialogueManager>().PlayDialogue(successDialogue));
            successEvent.Invoke();
        }
        else if (firstMeeting)
        {
            firstMeeting = false;
            StartCoroutine(GameObject.Find("Dialogue Manager").GetComponent<DialogueManager>().PlayDialogue(dialogue1));
        }
        else
        {
            if (dialogue2.Count > 0)
            {
                StartCoroutine(GameObject.Find("Dialogue Manager").GetComponent<DialogueManager>().PlayDialogue(dialogue2));
            }
            else
            {
                StartCoroutine(GameObject.Find("Dialogue Manager").GetComponent<DialogueManager>().PlayDialogue(dialogue1));
                introductionEvent.Invoke();
            }
        }
    }

    public void UnlockAbility(unlockAbility unlock)
    {
        StartCoroutine(UnlockAbilityCor(unlock));
    }

    public IEnumerator UnlockAbilityCor(unlockAbility unlock)
    {
        yield return new WaitUntil(() => !GameObject.Find("Player").GetComponent<PlayerMovement>().freeze);
        if (unlock != unlockAbility.NONE)
        {
            transform.GetChild(1).gameObject.GetComponent<TextMeshPro>().text = "";
            GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("NewAbility");
            if (unlock == unlockAbility.WALL_JUMP)
            {
                StartCoroutine(UnlockText("Wall Jump Unlocked!"));
                GameObject.Find("Player").GetComponent<PlayerMovement>().canWallJump = true;
            }
            else if (unlock == unlockAbility.DOUBLE_JUMP)
            {
                StartCoroutine(UnlockText("Double Jump Unlocked!"));
                GameObject.Find("Player").GetComponent<PlayerMovement>().maxJumps = 1;
            }
            else if (unlock == unlockAbility.DASH)
            {
                GameObject.Find("Player").GetComponent<PlayerMovement>().canDash = true;
            }
        }
    }

    IEnumerator UnlockText(string text)
    {
        AudioManager audio = GameObject.Find("Audio Manager").GetComponent<AudioManager>();
        GameObject.Find("Player").GetComponent<PlayerMovement>().freeze = true;
        GameObject unlockText = GameObject.Find("New Ability");
        unlockText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
        for (float i = 0; i < 1.1f; i += 0.1f)
        {
            unlockText.GetComponent<CanvasGroup>().alpha = i*0.5f;
            yield return new WaitForSeconds(0.05f);
        }
        unlockText.transform.GetChild(0).gameObject.SetActive(true);
        unlockText.GetComponent<CanvasGroup>().alpha = 1;
        unlockText.GetComponent<Image>().color = new Color32(0, 0, 0, 180);
        yield return new WaitForSeconds(2);
        GameObject.Find("Player").GetComponent<PlayerMovement>().freeze = false;
        for (float i = 0; i < 1.1f; i += 0.1f)
        {
            unlockText.GetComponent<CanvasGroup>().alpha = 1 - i;
            yield return new WaitForSeconds(0.1f);
        }
        unlockText.transform.GetChild(0).gameObject.SetActive(false);
        yield return new WaitUntil(() => GameObject.Find("Player").GetComponent<PlayerMovement>().hInput != 0);

    }

    public void NextScene()
    {
        StartCoroutine(NextSceneCor());
    }

    IEnumerator NextSceneCor()
    {
        yield return new WaitUntil(() => !GameObject.Find("Player").GetComponent<PlayerMovement>().freeze);
        GameObject.Find("Fader").GetComponent<Animator>().Play("FadeOut");
        LevelSelect levelSelect = GameObject.Find("Level Select").GetComponent<LevelSelect>();
        levelSelect.currentLevel++;
        yield return new WaitForSeconds(0.5f);
        levelSelect.unlocked[levelSelect.currentLevel-1] = true;
        SceneManager.LoadScene("Level " + levelSelect.currentLevel);
        GameObject.Find("Fader").GetComponent<Animator>().Play("FadeIn");
    }
}
