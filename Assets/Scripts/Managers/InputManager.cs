using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public InputManager() => Instance = this;


    public bool inputEnabled = true;
    public Vector2 MoveInput()
    {
        if (inputEnabled) return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        return Vector2.zero;
    }
    public bool ItemUseInput()
    {
        if (inputEnabled) return Input.GetMouseButtonDown(0) && UIScanner.ScanUI(Input.mousePosition).Count <= 0;
        return false;
    }
    public bool ItemUseHoldInput()
    {
        if(inputEnabled) return Input.GetMouseButton(0) && UIScanner.ScanUI(Input.mousePosition).Count <= 0;
        return false;
    }
    public bool EscapeInput()
    {
        return Input.GetKeyDown(KeyCode.Escape);
    }
}