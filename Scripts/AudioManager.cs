//Author: Ian Stolte
//Date: 7/13/23
//Desc: Manages the game audio (sound effects are called from other scripts, level music is called here)

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [Header("Editable")]
    public Sound[] music;
    public Sound[] sfx;

    [Header("Don't edit")]
    public AudioSource[] audios;

    [HideInInspector] public string currentSong;

    void Awake()
    {
        foreach (Sound s in music)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
        foreach (Sound s in sfx)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
        audios = gameObject.GetComponents<AudioSource>();
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Play("Menu");
            currentSong = "Menu";
        }
        SwapMusic();
    }

    /*void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }*/

    /*void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Level 1")
        {
            if (currentSong != "A Haunted Hotel")
            {
                FadeOutAll();
                currentSong = "A Haunted Hotel";
                Play("A Haunted Hotel");
                StartCoroutine(StartFade("A Haunted Hotel", 2, 0.4f));
            }
        }
        else if (/*scene.name == "Level 2" || scene.name == "Level 3")
        {
            if (currentSong != "Oxenfree")
            {
                FadeOutAll();
                currentSong = "Oxenfree";
                Play("Oxenfree");
                StartCoroutine(StartFade("Oxenfree", 2, 0.4f));
            }
        }
        else if (scene.name == "Level 4" || scene.name == "Level 5")
        {
            if (currentSong != "Approaching Heaven")
            {
                FadeOutAll();
                currentSong = "Approaching Heaven";
                Play("Approaching Heaven");
                StartCoroutine(StartFade("Approaching Heaven", 2, 0.7f));
            }
        }
        else if (scene.name == "Level 6")
        {
            FadeOutAll();
            currentSong = "Moving On";
            Play("Moving On");
            StartCoroutine(StartFade("Moving On", 2, 0.4f));
            GameObject.Find("Player").GetComponent<PlayerMovement>().freeze = true;
            GameObject.Find("Fader").GetComponent<Animator>().Play("FadeBlack");
        }
        else if (scene.name == "Level 1" || scene.name == "Level 2")
        {
            if (currentSong != "Beyond the Veil")
            {
                currentSong = "Beyond the Veil";
                Play("Beyond the Veil");
                Play("Elevator Theme (Beyond the Veil)");
                Play("Knight Theme");
                Play("King Theme");
                StartCoroutine(StartFade("Beyond the Veil", 2, 0.4f));
                FadeOutAll();
            }
        }
    }*/

    public void SwapMusic()
    {
        if (SceneManager.GetActiveScene().name == "Level 1")
        {
            if (currentSong != "Beyond the Veil")
            {
                currentSong = "Beyond the Veil";
                Play("Beyond the Veil");
                Play("Elevator Theme (Beyond the Veil)");
                Play("Knight Theme");
                //Play("King Theme");
                StartCoroutine(StartFade("Beyond the Veil", 2, 0.4f));
            }
        }
        else if (SceneManager.GetActiveScene().name == "Level 2")
        {
            if (currentSong != "Oxenfree")
            {
                currentSong = "Oxenfree";
                Play("Oxenfree");
                Play("Elevator Theme (Oxenfree)");
                StartCoroutine(StartFade("Oxenfree", 2, 0.4f));
            }
        }
        else if (SceneManager.GetActiveScene().name == "Library (5)")
        {
            if (currentSong != "Library Theme")
            {
                currentSong = "Library Theme";
                Play("Library Theme (background)");
                Play("Library Theme (melody)");
                Play("Librarian Theme");
                Play("Library Theme (quiet)");
                StartCoroutine(StartFade("Library Theme (background)", 2, 0.4f));
                StartCoroutine(StartFade("Library Theme (melody)", 2, 0.05f));
            }
        }
    }

    public IEnumerator FadeOutAll(float duration)
    {
        foreach (Sound s in music)
        {
            if (s.source.volume != 0)
                StartCoroutine(StartFade(s.name, duration, 0));
        }
        yield return new WaitForSeconds(duration);
        foreach (Sound s in music)
        {
            s.source.Stop();
        }
    }

    public IEnumerator QuietAll(float duration, float n)
    {
        foreach (Sound s in music)
        {
            if (s.source.volume != 0)
                StartCoroutine(StartFade(s.name, duration, s.source.volume*n));
        }
        yield return new WaitForSeconds(duration);
        foreach (Sound s in music)
        {
            if (s.source.volume != 0)
                StartCoroutine(StartFade(s.name, duration, s.source.volume/n));
        }

    }

    public void Play(string name)
    {
        Sound s = Array.Find(sfx, sound => sound.name == name);
        if (s == null)
            s = Array.Find(music, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("Sound: " + name + " not found!");
            return;
        }
        s.source.Play();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sfx, sound => sound.name == name);
        if (s == null)
            s = Array.Find(music, sound => sound.name == name);        
        if (s == null)
        {
            Debug.Log("Sound: " + name + " not found!");
            return;
        }
        s.source.Stop();
    }

    public IEnumerator StartFade(string name, float duration, float end)
    {
        Sound s = Array.Find(sfx, sound => sound.name == name);
        if (s == null)
            s = Array.Find(music, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("Sound: " + name + " not found!");
            yield break;
        }

        float currentTime = 0;
        float start = s.source.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            s.source.volume = Mathf.Lerp(start, end, currentTime / duration);
            yield return null;
        }

        //if (end == 0)
        //    s.source.Stop();
    }
}