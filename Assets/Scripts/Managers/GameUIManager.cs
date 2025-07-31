using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }
    public GameUIManager() => Instance = this;
}