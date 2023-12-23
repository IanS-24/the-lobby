using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    float currentTime;
    //float[] savedTimes; -- could have a list of checkpointed times that we display to the user if they redo the course
    bool timing;

    public GameObject resetPrompt;
    GameObject[] starts;
    GameObject[] pauses;

    void Update()
    {
        if (timing)
            currentTime += Time.deltaTime;
        
        if (currentTime == 0) {
            GetComponent<TMPro.TextMeshProUGUI>().text = "";
        }
        else {
            GetComponent<TMPro.TextMeshProUGUI>().text = "" + FormatTime(currentTime);
        }

        //reset
        if (Input.GetKeyDown(KeyCode.R))
        {
            resetPrompt.SetActive(true);
            timing = false;
        }

        starts = GameObject.FindGameObjectsWithTag("StartTimer");
        pauses = GameObject.FindGameObjectsWithTag("PauseTimer");

        foreach (GameObject collider in starts)
        {
            Bounds startBounds = collider.GetComponent<BoxCollider2D>().bounds;
            if (Physics2D.OverlapBox(startBounds.center, startBounds.extents * 2, 0, LayerMask.GetMask("Player")) && !timing)
            {
                timing = true;
                //collider.SetActive(false);
            }
        }

        foreach (GameObject collider in pauses)
        {
            Bounds pauseBounds = collider.GetComponent<BoxCollider2D>().bounds;
            if (Physics2D.OverlapBox(pauseBounds.center, pauseBounds.extents * 2, 0, LayerMask.GetMask("Player")) && timing)
            {
                timing = false;
                PauseBox pauseScript = collider.GetComponent<PauseBox>();
                StartCoroutine(GameObject.Find("Dialogue Manager").GetComponent<DialogueManager>().PlayDialogue(pauseScript.lines));
                GameObject.Find("Parkour Ghost").transform.position = pauseScript.ghostPosition;
                StartCoroutine(GameObject.Find("Main Camera").GetComponent<CameraFollow>().PanToPoint(pauseScript.ghostPosition, 0.3f, pauseScript.eventToPlay));
                //Pan up to ghost
                //GameObject.Find("Dialogue Manager").GetComponent<DialogueManager>().AmbientDialogue(new string[]{"Oh, nice job...", "I mean, you made it, at least.", "To be honest, I wasn't sure you'd get this far!"});
                collider.SetActive(false);    
            }
        }
    }

    public void NoReset()
    {
        resetPrompt.SetActive(false);
        timing = true;
    }

    public void YesReset()
    {
        currentTime = 0;
        StartCoroutine(YesResetCor());
    }

    IEnumerator YesResetCor()
    {
        GameObject.Find("Fader").GetComponent<Animator>().Play("FadeOut");
        yield return new WaitForSeconds(0.5f);
        GameObject.Find("Player").transform.position = new Vector3(-476,-18,-1);
        resetPrompt.SetActive(false);
        foreach (GameObject box in pauses)
        {
            box.SetActive(true);
        }
        GameObject.Find("Fader").GetComponent<Animator>().Play("FadeIn");
    }

    string FormatTime(float time)
    {
        float minutes = ((int)time)/60;
        float seconds = time%60;

        if (minutes == 0)
            return "" + Mathf.Round(seconds*10)/10.0f;
        else if (seconds < 10) 
            return "" + minutes + ":0" + Mathf.Round(seconds*10)/10.0f;
        else
            return "" + minutes + ":" + Mathf.Round(seconds*10)/10.0f;
    }
}