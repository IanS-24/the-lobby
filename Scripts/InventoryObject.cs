using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Events;

[System.Serializable]
public class InventoryObject
{
    public string id;
    public string name;
    //public string description;
    public string[] dialogue;
    public bool ambient;
    public bool collected;

    public InventoryObject(string id, string name, /*string description,*/ string[] dialogue, bool ambient=true)
    {
        this.id = id;
        this.name = name;
        //this.description = description;
        this.dialogue = dialogue;
        this.ambient = ambient;
        this.collected = false;
    }
}
