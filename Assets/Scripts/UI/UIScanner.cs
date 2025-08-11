using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIScanner : MonoBehaviour
{
    static List<RaycastResult> hits = new();
    public static List<RaycastResult> ScanUI(Vector2 pos)
    {
        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = pos }, hits);
        return hits;
    }
}