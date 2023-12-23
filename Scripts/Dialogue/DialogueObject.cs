using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct DialogueObject
{
    public string line;
    public enum dialogueFrame
    {
        PLAYER,
        ELEVATOR,
        KNIGHT,
        KING,
        LIBRARIAN
    }
    public dialogueFrame frame;
    public int choice;
    public bool endDialogue;
    public UnityEvent eventToPlay;

    public DialogueObject(string line, dialogueFrame frame=dialogueFrame.PLAYER)
    {
        this.line = line;
        this.frame = frame;
        this.choice = 0;
        this.endDialogue = false;
        this.eventToPlay = null;
    }
}
