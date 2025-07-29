using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeviceManager : MonoBehaviour
{
    static DeviceManager Instance;
    public DeviceManager() => Instance = this;

    [Header("Debug")]
    [SerializeField] bool overrideDeviceType = false;
    [SerializeField] DeviceType debugDeviceType = DeviceType.Desktop;
    public static DeviceType deviceType
    {
        get
        {
            if (Instance != null && Instance.overrideDeviceType) return Instance.debugDeviceType;
            return SystemInfo.deviceType;
        }
    }
}