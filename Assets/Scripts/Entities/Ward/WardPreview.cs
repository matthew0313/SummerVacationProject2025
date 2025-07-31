using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WardPreview : MonoBehaviour
{
    [SerializeField] Animator anim;
    readonly int canPlaceID = Animator.StringToHash("CanPlace");
    public void SetCanPlace(bool canPlace) => anim.SetBool(canPlaceID, canPlace);

    public bool isObstructed => obstructions.Count > 0;
    List<Collider2D> obstructions = new();
    private void OnTriggerEnter2D(Collider2D collision)
    {
        obstructions.Add(collision);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        obstructions.Remove(collision);
    }
}