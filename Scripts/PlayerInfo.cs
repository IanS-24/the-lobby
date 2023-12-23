using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerInfo : MonoBehaviour
{
    public bool button;
    public bool buttonSaved;

    public int currentNote;

    public bool firstDialogue;
    public bool[] ghostsMet; //0-elevator, 1-knight, 2-king, 3-no arm, 4-librarian
    public bool[] showEllipses;

    //Inventory
    public List<InventoryObject> inventory = new List<InventoryObject>();
    public List<InventoryObject> books = new List<InventoryObject>();
    [SerializeField] GameObject itemCollectedUI;
    IEnumerator itemPickup;

    //Quests & Npcs
    [HideInInspector] public bool king_knightDialoguePlayed;
    [HideInInspector] public bool knight_kingDialoguePlayed;
    [HideInInspector] public bool knight_swordDialoguePlayed;
    [HideInInspector] public bool knight_buttonDialoguePlayed;
    [HideInInspector] public bool knight_questFinished;
    [HideInInspector] public bool librarian_bookDialoguePlayed;

    public enum librarian_questState
    {
        UNFINISHED,
        STAY,
        DISAPPEAR
    }
    [HideInInspector] public librarian_questState librarian_quest;
    [HideInInspector] public int libraryCleanup;

    public KeyCode[] binds;

    void Start()
    {
        currentNote = 1;
        firstDialogue = false;
        ghostsMet = new bool[5];
        showEllipses = new bool[5];
        //libraryCleanup = 10;

        //Inventory
        if (inventory.Count == 0)
        {
            InventoryObject childhoodBook = new InventoryObject("childhoodBook", "The Cat in the Hat", new string[]{"Wait, *this was my favorite book when I was younger!", "I used to love reading along with the silly rhymes, singing them over and over...", "**I bet it drove my parents insane, but they never said a word.", ".*.*.**I haven't thought about those memories in years."}, false);
            inventory.Add(childhoodBook);
            //llama, llama, red pajama
            InventoryObject schoolBook = new InventoryObject("schoolBook", "Fahrenheit 451", new string[]{"Oh, I did a report on this last year.", "Spoiler alert: **it wasn't very good. ***But at least I got an A!", "****...little good that does me now." }, false);
            inventory.Add(schoolBook);
            InventoryObject sisterBook = new InventoryObject("sisterBook", "The Hunger Games", new string[]{"The Hunger Games... **my sister loved this book.", "I'm not quite sure why, to be honest. **It's kinda... **straightforward.", "Something about it must have drawn her in, though.", "She read it like twenty times!", "****.*.*.*maybe I should've asked her why."}, false);
            inventory.Add(sisterBook);
            InventoryObject book1 = new InventoryObject("randomBook", "The Ballad of the Weeping King", new string[]{"Huh. This looks old."});
            inventory.Add(book1);
            books.Add(book1);
            InventoryObject book2 = new InventoryObject("randomBook", "An Uninvited Guest", new string[]{"Oh, a horror novel", "And according to this review, it's the scariest book they've ever read!", "I doubt it."});
            inventory.Add(book2);
            books.Add(book2);
            InventoryObject book3 = new InventoryObject("randomBook", "Gunpowder Soup", new string[]{"Ugh. Looks like a 'classic'", "The title doesn't even make sense!"});
            inventory.Add(book3);
            books.Add(book3);
            InventoryObject book4 = new InventoryObject("randomBook", "Midnight", new string[0]);
            inventory.Add(book4);
            books.Add(book4);
            InventoryObject sword = new InventoryObject("sword", "Knight's Blade", new string[0]);
            inventory.Add(sword);
        }
    }

    void Update()
    {
        GameObject.Find("Button Indicator").GetComponent<Image>().enabled = button;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        button = false;
        buttonSaved = false;
        if (scene.name != "Main Menu")
        {
            GetComponent<SpriteRenderer>().enabled = true;
            SaveSystem.SavePlayer();
        }
        else
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    public void CollectItem(string itemID)
    { 
        if (itemID == "sword")
        {
            FindItem("sword").collected = true;
            showEllipses[2] = true; //king sword dialogue
        }

        if (itemID == "randomBook")
        {
            if (books.Count > 0)
            {
                GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Collect");
                int n = Random.Range(0, books.Count);
                foreach (InventoryObject obj in inventory)
                {
                    if (obj.name == books[n].name)
                        obj.collected = true;
                }
                if (itemPickup != null)
                    StopCoroutine(itemPickup);
                itemPickup = ItemPickup(books[n]);
                StartCoroutine(itemPickup);
                books.RemoveAt(n);
                GameObject.Find("Books Text").GetComponent<TMPro.TextMeshProUGUI>().text = "Books collected: " + (4-books.Count) + "/4";
                if (books.Count == 0)
                    GameObject.Find("Books Text").GetComponent<TMPro.TextMeshProUGUI>().color = new Color32(123, 183, 125, 255);
                GameObject.Find("Books Text").GetComponent<Animator>().Play("FadeText");
            }
        }
        else {
            GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("CollectImportant");
            InventoryObject item = null;
            foreach (InventoryObject obj in inventory)
            {
                if (obj.id == itemID)
                {
                    obj.collected = true;
                    item = obj;
                }
            }
            if (itemPickup != null)
                StopCoroutine(itemPickup);
            if (item != null)
            {
                itemPickup = ItemPickup(item);
                StartCoroutine(itemPickup);
            }
        }
    }

    IEnumerator ItemPickup(InventoryObject obj)
    {
        DialogueManager dialogue = GameObject.Find("Dialogue Manager").GetComponent<DialogueManager>();
        dialogue.StopAmbientDialogue();
        itemCollectedUI.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = obj.name;
        itemCollectedUI.SetActive(true);
        GetComponent<PlayerMovement>().freeze = true;
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        GetComponent<PlayerMovement>().freeze = false;
        itemCollectedUI.SetActive(false);
        if (obj.ambient == true)
            dialogue.AmbientDialogue(obj.dialogue);    
        else
        {
            List<DialogueObject> dialogueObjects = new List<DialogueObject>();
            foreach (string str in obj.dialogue)
            {
                DialogueObject dialogueObj = new DialogueObject(str);
                dialogueObjects.Add(dialogueObj);
            }
            StartCoroutine(dialogue.PlayDialogue(dialogueObjects));
        }
    }

    public InventoryObject FindItem(string name)
    {
            foreach (InventoryObject obj in inventory)
            {
                if (obj.id == name)
                    return obj;
            }
            return null;
    }
}