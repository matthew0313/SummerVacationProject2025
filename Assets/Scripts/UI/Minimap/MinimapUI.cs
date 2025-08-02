using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RawImage))]
public class MinimapUI : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] Camera minimapCamera;

    RawImage rawImage;
    PlayerCamera playerCamera;
    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        playerCamera = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<PlayerCamera>();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 pos = transform.InverseTransformPoint(Input.mousePosition) * (rawImage.texture.width / rawImage.rectTransform.rect.width);
        playerCamera.FreeTrack(minimapCamera.ScreenToWorldPoint(pos));
    }
}