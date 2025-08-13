using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class AnimEventChannel : MonoBehaviour
{
    [SerializeField] List<UnityEvent> events = new();
    public int eventCount => events.Count;
    public void CallEvent(int index)
    {
        if (index < 0 || index >= events.Count) return;
        events[index].Invoke();
    }
}