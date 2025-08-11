using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class SlotUI : MonoBehaviour
{
    [SerializeField] protected Animator anim;

    readonly int selectedID = Animator.StringToHash("Selected");
    public void SetSelected(bool selected)
    {
        anim.SetBool(selectedID, selected);
    }
}