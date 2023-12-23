using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    [Header("Level Setup")]
    public int numLevels;
    public bool[] unlocked;
    public Vector3[] startingPositions;
    public int currentLevel;

    [Header("Menu")]
    public bool paused;
    [SerializeField] GameObject fader;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject levelSelectMenu;
    [SerializeField] GameObject settingsMenu;
    [SerializeField] GameObject keybindMenu;
    [SerializeField] GameObject buttonBlock;
    [SerializeField] GameObject newGamePrompt;

    PlayerMovement player;

    void Awake()
    {
        unlocked = new bool[numLevels];
        unlocked[0] = true;
        unlocked[1] = true;
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name != "Main Menu")
        {
            if (!paused && Input.GetKeyDown(GameObject.Find("Player").GetComponent<PlayerInfo>().binds[6]) && !GameObject.Find("Player").GetComponent<PlayerMovement>().freeze)
            {
                paused = true;
                Time.timeScale = 0;
                pauseMenu.SetActive(true);
                for (int i = 0; i < numLevels; i++)
                {
                    buttonBlock.transform.GetChild(i).gameObject.SetActive(!unlocked[i]);
                }
            }
            else if (paused && Input.GetKeyDown(GameObject.Find("Player").GetComponent<PlayerInfo>().binds[6]) && !GameObject.Find("Keybind Manager").GetComponent<Keybinds>().doNotExit)
            {
                paused = false;
                Time.timeScale = 1;
                pauseMenu.SetActive(false);
                levelSelectMenu.SetActive(false);
                settingsMenu.SetActive(false);
                keybindMenu.SetActive(false);
            }
        }
        else if (Input.GetKeyDown(GameObject.Find("Player").GetComponent<PlayerInfo>().binds[6]) && !GameObject.Find("Keybind Manager").GetComponent<Keybinds>().doNotExit)
        {
            levelSelectMenu.SetActive(false);
            settingsMenu.SetActive(false);
            keybindMenu.SetActive(false);
            newGamePrompt.SetActive(false);
        }
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
        if (scene.name == "Main Menu")
            MenuButtons();
    }

    public void LoadSave()
    {
        PlayerData data = SaveSystem.LoadPlayer();
        if (data != null)
        {
            PlayerInfo playerInfo = GameObject.Find("Player").GetComponent<PlayerInfo>();
            playerInfo.binds = data.binds;

            currentLevel = data.currentLevel;
            LoadScene("Level " + currentLevel);
            for (int i = 0; i < unlocked.Length; i++)
            {
                if (i < data.levelsUnlocked)
                    unlocked[i] = true;
                else
                    unlocked[i] = false;
            }
    
            Settings settings = GameObject.Find("Settings").GetComponent<Settings>();
            settings.musicVolume = data.musicVolume;
            settings.sfxVolume = data.sfxVolume;
            settings.textSpeed = data.textSpeed;
            settings.MusicSlider();
            settings.SfxSlider();
            settings.TextSlider();

            if (data.wallJump)
            {
                GameObject.Find("Player").GetComponent<PlayerMovement>().canWallJump = true;
                Destroy(GameObject.Find("Wall Jump Hint"));
            }
            GameObject.Find("Player").GetComponent<PlayerMovement>().maxJumps = data.maxJumps;
            if (data.maxJumps > 0)
                Destroy(GameObject.Find("Double Jump Hint"));

            playerInfo.firstDialogue = data.firstDialogue;
            playerInfo.ghostsMet = data.ghostsMet;
            playerInfo.showEllipses = data.showEllipses;

            playerInfo.inventory = data.inventory;

            playerInfo.king_knightDialoguePlayed = data.king_knightDialoguePlayed;

            playerInfo.knight_buttonDialoguePlayed = data.knight_buttonDialoguePlayed;
            playerInfo.knight_kingDialoguePlayed = data.knight_kingDialoguePlayed;
            playerInfo.knight_swordDialoguePlayed = data.knight_swordDialoguePlayed;
            playerInfo.knight_questFinished = data.knight_questFinished;
        }
        else
        {
            Debug.Log("Save not found...");
            ChangeScene(1);
        }
    }

    public void NewGame()
    {
        PlayerData data = SaveSystem.LoadPlayer();
        if (data == null || newGamePrompt.activeSelf)
        {
            newGamePrompt.SetActive(false);
            SaveSystem.SavePlayer(true);
            LoadSave();
        }
        else
        {
            newGamePrompt.SetActive(true);
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void LoadScene(string sceneName)
    {
        paused = false;
        Time.timeScale = 1;
        StartCoroutine(LoadSceneCor(sceneName));
    }

    IEnumerator LoadSceneCor(string sceneName)
    {
        GameObject.Find("Fader").GetComponent<Animator>().Play("FadeOut");
        yield return new WaitForSeconds(0.5f);
        pauseMenu.SetActive(false);
        SceneManager.LoadScene(sceneName);
        GameObject.Find("Fader").GetComponent<Animator>().Play("FadeIn");
    }

    public void ChangeScene(int n)
    {
        paused = false;
        Time.timeScale = 1;
        StartCoroutine(ChangeSceneCor(n));
    }

    IEnumerator ChangeSceneCor(int n)
    {
        AudioManager audioManager = GameObject.Find("Audio Manager").GetComponent<AudioManager>();

        unlocked[n-1] = true;
        GameObject.Find("Fader").GetComponent<Animator>().Play("FadeOut");
        if (currentLevel != n)
        {
            currentLevel = n;
            StartCoroutine(audioManager.FadeOutAll(1.5f));
            yield return new WaitForSeconds(0.5f);
            GameObject.Find("Save Text").GetComponent<Animator>().Play("FadeText");
            yield return new WaitForSeconds(1);
            pauseMenu.SetActive(false);
            levelSelectMenu.SetActive(false);
            settingsMenu.SetActive(false);
            SceneManager.LoadScene("Level " + n);
            //audioManager.Play("LevelLoad");
            yield return new WaitForSeconds(0.5f);
            GameObject.Find("Fader").GetComponent<Animator>().Play("FadeIn");
            audioManager.SwapMusic();
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            pauseMenu.SetActive(false);
            levelSelectMenu.SetActive(false);
            settingsMenu.SetActive(false);
            SceneManager.LoadScene("Level " + n);
            GameObject.Find("Fader").GetComponent<Animator>().Play("FadeIn");
        }
    }

    void MenuButtons()
    {
        GameObject.Find("Quit Button").GetComponent<Button>().onClick.AddListener(Quit);
        GameObject.Find("Level Select Button").GetComponent<Button>().onClick.AddListener(delegate { SetActive(levelSelectMenu); });
        GameObject.Find("Settings Button").GetComponent<Button>().onClick.AddListener(delegate { SetActive(settingsMenu); });
        GameObject.Find("Continue Button").GetComponent<Button>().onClick.AddListener(LoadSave);
        GameObject.Find("New Game Button").GetComponent<Button>().onClick.AddListener(NewGame);
    }

    void SetActive(GameObject g)
    {
        g.SetActive(true);
    }

    public void RefreshKeybinds()
    {
        for (int i = 0; i < GameObject.Find("Player").GetComponent<PlayerInfo>().binds.Length; i++)
        {
            GameObject.Find("Keybind " + i).GetComponent<TMPro.TextMeshProUGUI>().text = "" + GameObject.Find("Player").GetComponent<PlayerInfo>().binds[i];
        }
    }
}