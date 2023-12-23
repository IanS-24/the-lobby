//Author: Ian Stolte
//Date: 7/13/23
//Desc: Has functions for controlling dialogue

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Tooltip("A list of all dialogue frames/portraits")] [SerializeField] GameObject[] framePrefabs;
    [SerializeField] GameObject choicePrefab;
    [SerializeField] TextMeshProUGUI txt;
    [SerializeField] GameObject ambientText;
    [SerializeField] GameObject firstDialogueHint;
    string currentText;
    bool skip;
    [HideInInspector] public bool playing;
    GameObject currentFrame;
    int lastFrame;
    int currentChoice;

    //Ambient Dialogue
    [HideInInspector] public IEnumerator ambientDialogue;

    //Interactive dialogue
    bool choosingDialogue;
    bool linePlayed;
    List<GameObject> dialogueChoices;
    int index;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            skip = true;
        }
        if (choosingDialogue)
        {
            for (int i = 0; i < dialogueChoices.Count; i++)
            {
                if (i == index)
                    dialogueChoices[i].GetComponent<Image>().color = new Color32(192, 162, 121, 255);
                else
                    dialogueChoices[i].GetComponent<Image>().color = new Color32(154, 137, 111, 255);
            }
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                index--;
                if (index < 0)
                    index += dialogueChoices.Count;
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                index++;
                if (index >= dialogueChoices.Count)
                    index -= dialogueChoices.Count;
            }
        }
    }

    public void StopAmbientDialogue()
    {
        if (ambientDialogue != null)
            StopCoroutine(ambientDialogue);
        ambientText.GetComponent<CanvasGroup>().alpha = 0;
    }

    public void AmbientDialogue(string[] lines)
    {
        if (ambientDialogue != null)
            StopCoroutine(ambientDialogue);
        ambientDialogue = AmbientDialogueCor(lines);
        StartCoroutine(ambientDialogue);
    }

    public IEnumerator AmbientDialogueCor(string[] lines)
    {
        foreach (string line in lines)
        {
            ambientText.GetComponent<TextMeshProUGUI>().text = line;
            for (float i = 0; i < 1.02f; i+= 0.02f)
            {
                yield return new WaitForSeconds(0.01f);
                ambientText.GetComponent<CanvasGroup>().alpha = i;
            }
            yield return new WaitForSeconds(1 + line.Length * 0.03f);
            for (float i = 1; i > 0; i -= 0.02f)
            {
                yield return new WaitForSeconds(0.008f);
                ambientText.GetComponent<CanvasGroup>().alpha = i;
            }
            ambientText.GetComponent<CanvasGroup>().alpha = 0;
        }
    }

    public IEnumerator PlayDialogue(List<DialogueObject> dialogue, int font = 38)
    {
        StopAmbientDialogue();
        GameObject player = GameObject.Find("Player");
        player.GetComponent<PlayerMovement>().freeze = true;
        lastFrame = 0;
        currentFrame = txt.gameObject;
        txt.fontSize = font;
        txt.gameObject.SetActive(true);
        currentChoice = 0;

        player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        for (int i = 0; i < dialogue.Count; i++)
        {
            //interactive dialogue
            linePlayed = false;
            if (dialogue[i].choice == currentChoice)
            {
                StartCoroutine(PlayLine(dialogue, i));
                yield return new WaitUntil(() => !playing);
            }
            else if (dialogue[i].choice == 0 || SharedBeginning(dialogue[i].choice, currentChoice))
            {
                currentChoice = 0;
                StartCoroutine(PlayLine(dialogue, i));
                yield return new WaitUntil(() => !playing);
            }
            //player choice popup
            else if (dialogue[i].choice == 10*currentChoice + 1 || (currentChoice == 0 && dialogue[i].choice != 0))
            {
                choosingDialogue = true;
                dialogueChoices = new List<GameObject>();
                currentChoice = dialogue[i].choice-1;
                int offset = 0;
                //loop through dialogue searching for choices
                while (dialogue[i + offset].choice != 0 && i + offset < dialogue.Count - 1)
                {
                    if (dialogue[i + offset].choice > currentChoice && dialogue[i + offset].choice < currentChoice+4) //if start of next branch...
                    {
                        currentChoice = dialogue[i + offset].choice;
                        if (dialogueChoices.Count < 3)
                        {
                            GameObject obj = Instantiate(choicePrefab, transform.position, transform.rotation, GameObject.Find("Canvas").transform);
                            obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = dialogue[i + offset].line;
                            dialogueChoices.Add(obj);
                        }
                    }
                    offset++;
                }
                for (int j = 0; j < dialogueChoices.Count; j++) //display each of the boxes
                {
                    if (dialogueChoices.Count == 2)
                        dialogueChoices[j].GetComponent<RectTransform>().anchoredPosition = new Vector2(50, -240 + -70 * j);
                    else if (dialogueChoices.Count == 3)
                        dialogueChoices[j].GetComponent<RectTransform>().anchoredPosition = new Vector2(50, -220 + -60 * j);
                }
                index = 0;
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0));
                choosingDialogue = false;
                while(dialogue[i].line != dialogueChoices[index].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text) //skip to correct branch
                {
                    i++;
                }
                foreach (GameObject o in dialogueChoices) //clear boxes
                {
                    Destroy(o);
                }
                i++; //skip response player clicked on
                currentChoice = dialogue[i].choice;
                StartCoroutine(PlayLine(dialogue, i));
                yield return new WaitUntil(() => !playing);
            }
            if (dialogue[i].endDialogue && linePlayed)
            {
                break;
            }
        }
        txt.gameObject.SetActive(false);
        if (currentFrame != txt.gameObject)
            Destroy(currentFrame);
        yield return new WaitForSeconds(0.1f);
        player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        player.GetComponent<PlayerMovement>().freeze = false;
    }

    bool SharedBeginning(int shortN, int longN)
    {
        while(longN > 0)
        {
            if (shortN == longN) {
                return true;
            }
            else {
                longN = longN/10;
            }
        }
        return false;
    }

    IEnumerator PlayLine(List<DialogueObject> dialogue, int i)
    {
        skip = false;
        playing = true;
        linePlayed = true;
        if ((int)dialogue[i].frame != lastFrame || i == 0 /*|| dialogue[i-index-1].choice == 1*/)
        {
            if (i != 0)
            {
                Destroy(currentFrame);
            }
            currentFrame = Instantiate(framePrefabs[(int)dialogue[i].frame], transform.position, transform.rotation, GameObject.Find("Canvas").transform);
            currentFrame.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -280);
            currentFrame.transform.SetSiblingIndex(1);
        }
        lastFrame = (int)dialogue[i].frame;
        IEnumerator talkCor = Talk(dialogue[i].line);
        StartCoroutine(talkCor);
        for (float j = 0; j < dialogueTime(dialogue[i].line); j += 0.1f)
        {
            if (skip == false)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
        if (!GameObject.Find("Player").GetComponent<PlayerInfo>().firstDialogue)
        {
            GameObject.Find("Player").GetComponent<PlayerInfo>().firstDialogue = true;
            //firstDialogueHint.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "" + GameObject.Find("Player").GetComponent<PlayerInfo>().binds[4];
            firstDialogueHint.SetActive(true);
        }
        else {
            firstDialogueHint.SetActive(false);
        }
        yield return new WaitForSeconds(0.01f);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0));
        StopCoroutine(talkCor);
        txt.text = "";
        firstDialogueHint.SetActive(false);
        playing = false;
        if (dialogue[i].eventToPlay != null)
            dialogue[i].eventToPlay.Invoke();
    }

    IEnumerator Talk(string line)
    {
        float textSpeed = GameObject.Find("Settings").GetComponent<Settings>().textSpeed;
        currentText = "";
        foreach (char c in line)
        {
            if (c != '*')
            {
                currentText += c;
            }
            txt.text = currentText;
            if (skip == false)
            {
                if (c == ' ')
                {
                    yield return new WaitForSeconds(0.1f * textSpeed);
                }
                else if (c == '.')
                {
                    yield return new WaitForSeconds(0.3f * textSpeed);
                }
                else if (c == ',')
                {
                    yield return new WaitForSeconds(0.2f * textSpeed);
                }
                else if (c == '*')
                {
                    yield return new WaitForSeconds(0.2f * textSpeed);
                }
                else if (line.Length > 30)
                {
                    yield return new WaitForSeconds(0.1f * textSpeed);
                }
                else
                {
                    yield return new WaitForSeconds(0.15f * textSpeed);
                }
            }
        }
    }

    float dialogueTime(string line)
    {
        float textSpeed = GameObject.Find("Settings").GetComponent<Settings>().textSpeed;
        float time = 0;
        foreach (char c in line)
        {
            if (c == ' ')
            {
                time += 0.1f * textSpeed;
            }
            else if (c == '.')
            {
                time += 0.3f * textSpeed;
            }
            else if (c == ',')
            {
                time += 0.2f * textSpeed;
            }
            else if (c == '*')
            {
                time += 0.2f * textSpeed;
            }
            else if (line.Length > 30)
            {
                time += 0.1f * textSpeed;
            }
            else
            {
                time += 0.15f * textSpeed;
            }
        }
        return time;
    }
}