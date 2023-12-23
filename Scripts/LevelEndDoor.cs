using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndDoor : InteractableObject
{
    [SerializeField] string destination;

    public override void Interact()
    {
        StartCoroutine(Teleport());
    }

    IEnumerator Teleport()
    {
        GameObject.Find("Player").GetComponent<PlayerMovement>().freeze = true;
        GameObject.Find("Fader").GetComponent<Animator>().Play("TransitionFade");
        GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("DoorOpen");
        yield return new WaitForSeconds(0.5f);
        GameObject.Find("Player").GetComponent<PlayerMovement>().freeze = false;
        SceneManager.LoadScene(destination);
    }
}
