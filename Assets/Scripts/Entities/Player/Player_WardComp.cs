using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player_WardComp : NetworkBehaviour
{
    Player origin;
    bool isDead => origin.hpComp.isDead.Value;

    [SerializeField] Ward wardPrefab;
    [SerializeField] WardPreview wardPreviewPrefab;
    [SerializeField] KeyCode wardPlaceKey = KeyCode.Q;
    [SerializeField] float m_wardPlaceCooldown = 5.0f, wardPlaceRange = 5.0f;
    [SerializeField] int m_maxWardsOwned = 10, m_maxWardsPlaced = 3;
    [SerializeField] LayerMask obstacleMask;
    public float wardPlaceCooldown => m_wardPlaceCooldown;
    public int maxWardsOwned => m_maxWardsOwned;
    public int maxWardsPlaced => m_maxWardsPlaced;
    public int wardsOwned => origin.wards.Value;
    public float wardPlaceCounter { get; private set; } = 0.0f;

    public readonly List<Ward> wardsPlaced = new();
    TopLayer topLayer;

    public event Action onWardsOwnedChange, onWardsPlacedChange;
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, wardPlaceRange);
    }
    private void Awake()
    {
        origin = GetComponent<Player>();
    }
    private void Start()
    {
        if (IsOwner)
        {
            topLayer = new(this);
            topLayer.OnStateEnter();
        }
    }
    private void Update()
    {
        if (IsOwner)
        {
            if (wardPlaceCounter > 0) wardPlaceCounter = Mathf.Max(wardPlaceCounter - Time.deltaTime, 0);
            topLayer.OnStateUpdate();
        }
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (IsOwner)
        {
            topLayer.OnStateExit();
        }
    }
    public void AddWard(Ward ward)
    {
        wardsPlaced.Add(ward);
        onWardsPlacedChange?.Invoke();
    }
    public void RemoveWard(Ward ward)
    {
        wardsPlaced.Remove(ward);
        onWardsPlacedChange?.Invoke();
    }
    public bool canPlaceWard => wardPlaceCounter <= 0.0f && wardsOwned > 0 && !isDead && wardsPlaced.Count < maxWardsPlaced && origin.PVP.Value;
    class TopLayer : TopLayer<Player_WardComp>
    {
        public TopLayer(Player_WardComp origin) : base(origin)
        {
            defaultState = new Idle(origin, this);
            AddState("Idle", defaultState);
            AddState("Placing", new Placing(origin, this));
        }
        class Idle : State<Player_WardComp>
        {
            public Idle(Player_WardComp origin, Layer<Player_WardComp> parent) : base(origin, parent) { }
            public override void OnStateUpdate()
            {
                base.OnStateUpdate();
                if (origin.canPlaceWard && Input.GetKeyDown(origin.wardPlaceKey))
                {
                    parentLayer.ChangeState("Placing"); return;
                }
            }
        }
        class Placing : State<Player_WardComp>
        {
            public Placing(Player_WardComp origin, Layer<Player_WardComp> parent) : base(origin, parent) { }

            readonly int placingWardID = Animator.StringToHash("PlacingWard");
            WardPreview wardPreview;
            Camera mainCam = Camera.main;
            public override void OnStateEnter()
            {
                base.OnStateEnter();
                if (wardPreview == null) wardPreview = Instantiate(origin.wardPreviewPrefab);
                wardPreview.gameObject.SetActive(true);
                wardPreview.SetCanPlace(false);
                wardPreview.transform.position = mainCam.ScreenToWorldPoint(Input.mousePosition);
                origin.origin.anim.SetBool(placingWardID, true);
            }
            public override void OnStateUpdate()
            {
                base.OnStateUpdate();
                if (!origin.canPlaceWard)
                {
                    parentLayer.ChangeState("Idle"); return;
                }
                Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
                wardPreview.transform.position = mousePos;
                bool canPlace =
                    !wardPreview.isObstructed &&
                    Vector2.Distance(origin.transform.position, mousePos) < origin.wardPlaceRange &&
                    !Physics2D.Raycast(
                        origin.transform.position,
                        (mousePos - (Vector2)origin.transform.position).normalized,
                        Vector2.Distance(mousePos, origin.transform.position), origin.obstacleMask);
                wardPreview.SetCanPlace(canPlace);
                if (canPlace && Input.GetMouseButtonDown(0))
                {
                    Ward ward = Instantiate(origin.wardPrefab, wardPreview.transform.position, Quaternion.identity);
                    ward.NetworkObject.Spawn();
                    ward.SetOwnerRpc(origin.origin);
                    origin.wardsOwned--;
                    origin.wardPlaceCounter = origin.wardPlaceCooldown;
                    origin.AddWard(ward);
                    parentLayer.ChangeState("Idle"); return;
                }
            }
            public override void OnStateExit()
            {
                base.OnStateExit();
                wardPreview.gameObject.SetActive(false);
                origin.origin.anim.SetBool(placingWardID, false);
            }
        }
    }

}