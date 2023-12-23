using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PauseBox : MonoBehaviour
{
    public Vector3 ghostPosition;
    public List<DialogueObject> lines = new List<DialogueObject>();
    public UnityEvent eventToPlay;
}
