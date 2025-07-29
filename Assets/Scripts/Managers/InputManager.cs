using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public InputManager() => Instance = this;
    PlayerInputs playerInputs;

    public InputAction Movement => playerInputs.Default.Movement;
    public InputAction Aim => playerInputs.Default.Aim;
    public InputAction UseWeapon => playerInputs.Default.UseWeapon;
    private void OnEnable()
    {
        if (playerInputs == null) playerInputs = new();
        playerInputs.Enable();
    }
    private void OnDisable()
    {
        playerInputs.Disable();
    }
}