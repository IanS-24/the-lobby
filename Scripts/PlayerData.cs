using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    //Level select
    public int currentLevel;
    public int levelsUnlocked;
    
    //Abilities
    public bool wallJump;
    public int maxJumps;

    //Settings
    public float musicVolume;
    public float sfxVolume;
    public float textSpeed;
    public KeyCode[] binds;

    //Dialogue & quests
    public bool firstDialogue;
    public bool[] ghostsMet;
    public bool[] showEllipses;

    //Inventory
    public List<InventoryObject> inventory = new List<InventoryObject>();

    public bool king_knightDialoguePlayed;
    public bool knight_kingDialoguePlayed;
    public bool knight_swordDialoguePlayed;
    public bool knight_buttonDialoguePlayed;
    public bool knight_questFinished;

    public PlayerData(bool newGame = false)
    {
        if (newGame)
        {
            currentLevel = 1;
            levelsUnlocked = 1;

            binds = new KeyCode[7];
            binds[0] = KeyCode.A;
            binds[1] = KeyCode.LeftArrow;
            binds[2] = KeyCode.D;
            binds[3] = KeyCode.RightArrow;
            binds[4] = KeyCode.Space;
            binds[5] = KeyCode.E;
            binds[6] = KeyCode.Escape;
            
            wallJump = false;
            maxJumps = 0;
            musicVolume = 0.6f;
            sfxVolume = 0.6f;
            textSpeed = 0.6f;

            firstDialogue = false;
            ghostsMet = new bool[4]; //TODO: update this as we add NPCs
            showEllipses = new bool[4];

            king_knightDialoguePlayed = false;
            knight_kingDialoguePlayed = false;
            knight_swordDialoguePlayed = false;
            knight_buttonDialoguePlayed = false;
            knight_questFinished = false;
        }
        else
        {
            PlayerInfo playerInfo = GameObject.Find("Player").GetComponent<PlayerInfo>();

            currentLevel = GameObject.Find("Level Select").GetComponent<LevelSelect>().currentLevel;
            bool[] unlocked = GameObject.Find("Level Select").GetComponent<LevelSelect>().unlocked;
            int i = 0;
            while (i < unlocked.Length-1 && unlocked[i])
            {
                i++;
            }
            if (unlocked[unlocked.Length-1]) //check last level
                i++;
            levelsUnlocked = i;
            binds = playerInfo.binds;
            wallJump = GameObject.Find("Player").GetComponent<PlayerMovement>().canWallJump;
            maxJumps = GameObject.Find("Player").GetComponent<PlayerMovement>().maxJumps;
            
            Settings settings = GameObject.Find("Settings").GetComponent<Settings>();
            musicVolume = settings.musicVolume;
            sfxVolume = settings.sfxVolume;
            textSpeed = settings.textSpeed;

            firstDialogue = playerInfo.firstDialogue;
            ghostsMet = playerInfo.ghostsMet;
            showEllipses = playerInfo.showEllipses;

            inventory = playerInfo.inventory;

            king_knightDialoguePlayed = playerInfo.king_knightDialoguePlayed;

            knight_kingDialoguePlayed = playerInfo.knight_kingDialoguePlayed;
            knight_swordDialoguePlayed = playerInfo.knight_swordDialoguePlayed;
            knight_buttonDialoguePlayed = playerInfo.knight_buttonDialoguePlayed;
            knight_questFinished = playerInfo.knight_questFinished;

        }
    }
}
