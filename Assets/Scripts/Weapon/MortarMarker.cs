using UnityEngine;
using System.Collections;
using Unity.Netcode.Components;
using Unity.Netcode;

public class MortarMarker : MonoBehaviour
{
    [SerializeField] Animator anim;
    readonly int showID = Animator.StringToHash("Show");
    public void Show() => anim.SetBool(showID, true);
    public void Hide() => anim.SetBool(showID, false);
}