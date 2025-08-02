using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using static UnityEngine.UI.Image;

[RequireComponent(typeof(Rigidbody2D), typeof(NetworkTransform), typeof(NetworkRigidbody2D))]
[RequireComponent(typeof(Player_HpComp))]
public class Player : Entity
{
    public static Player localPlayer;
    public Player_HpComp hpComp { get; private set; }
    Rigidbody2D rb;

    public Animator anim;

    [Header("Movements")]
    [SerializeField] float moveSpeed = 5.0f;
    [SerializeField] Transform rotator, flipped;
    
    [Header("Ward System")]
    [SerializeField] private KeyCode wardPlaceKey = KeyCode.W;
    [SerializeField] private Ward.WardType currentWardType = Ward.WardType.Vision;
    [SerializeField] private float wardPlacementRange = 3f;

    public readonly NetworkVariable<Team> team = new(Team.None);
    public readonly NetworkVariable<bool> PVP = new(false);
    bool isDead => hpComp.isDead.Value;
    TopLayer topLayer;

    private void Awake()
    {
        hpComp = GetComponent<Player_HpComp>();
        rb = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        if (IsLocalPlayer)
        {
            localPlayer = this;
            foreach(var i in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).OfType<ILocalPlayerSpawnCallback>())
            {
                i.OnLocalPlayerSpawn(this);
            }
        }

        if (IsOwner)
        {
            topLayer = new(this);
            topLayer.OnStateEnter();
        }
    }
    readonly int relationID = Animator.StringToHash("Relation");
    private void Update()
    {
        if (IsOwner)
        {
            topLayer.OnStateUpdate();
            UpdateRotation();
            UpdateWardPlacement();
        }
        if (localPlayer != null) anim.SetInteger(relationID, (int)localPlayer.GetRelation(this));
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        GameManager.Instance.RegisterPlayer(OwnerClientId, this);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        GameManager.Instance.UnregisterPlayer(OwnerClientId);
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (IsOwner) topLayer.OnStateExit();
    }

    [Rpc(SendTo.Authority)]
    public void SetTeamRpc(Team team) => this.team.Value = team;
    [Rpc(SendTo.Authority)]
    public void SetPVPRpc(bool PVP) => this.PVP.Value = PVP;

    public Relation GetRelation(Entity other)
    {
        if (other == this) return Relation.Self;
        if (other is Player player)
        {
            if (!PVP.Value) return Relation.None;
            if (team.Value != Team.None && team.Value == player.team.Value) return Relation.Ally;
            return Relation.Enemy;
        }
        return Relation.None;
    }

    //Movement
    [Rpc(SendTo.Owner)]
    public void TeleportRpc(Vector2 position) => transform.position = position;
    
    // Ward System
    private void UpdateWardPlacement()
    {
        if (Input.GetKeyDown(wardPlaceKey))
        {
            PlaceWard();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentWardType = Ward.WardType.Vision;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentWardType = Ward.WardType.Damage;
        }
    }
    
    private void PlaceWard()
    {
        if (WardManager.Instance != null)
        {
            Vector3 placementPosition = GetWardPlacementPosition();
            WardManager.Instance.PlaceWardServerRpc(placementPosition, currentWardType);
        }
    }
    
    private Vector3 GetWardPlacementPosition()
    {
        Vector3 playerPosition = transform.position;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;
        
        Vector3 direction = (mousePosition - playerPosition).normalized;
        Vector3 placementPosition = playerPosition + direction * wardPlacementRange;
        
        return placementPosition;
    }
    
    public void SetWardType(Ward.WardType wardType)
    {
        currentWardType = wardType;
    }
    
    public Ward.WardType GetCurrentWardType()
    {
        return currentWardType;
    }
    void UpdateRotation()
    {
        Vector2 dir;
        if (DeviceManager.deviceType == DeviceType.Desktop) dir = (Input.mousePosition - rotator.position).normalized;
        else dir = InputManager.Instance.Aim.ReadValue<Vector2>();
        rotator.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        if(dir.x > 0)
        {
            flipped.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            rotator.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
        if(dir.x < 0)
        {
            flipped.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
            rotator.localScale = new Vector3(1.0f, -1.0f, 1.0f);
        }
    }
    class TopLayer : TopLayer<Player>
    {
        public TopLayer(Player origin) : base(origin)
        {
            defaultState = new Idle(origin, this);
            AddState("Idle", defaultState);
            AddState("Moving", new Moving(origin, this));
            AddState("Dead", new Dead(origin, this));
            origin.hpComp.onDeath += () => ChangeState("Dead");
        }
        class Idle : State<Player>
        {
            public Idle(Player origin, Layer<Player> parent) : base(origin, parent) { }
            public override void OnStateUpdate()
            {
                base.OnStateUpdate();
                if (InputManager.Instance.Movement.ReadValue<Vector2>().magnitude > 0.1f)
                {
                    parentLayer.ChangeState("Moving"); return;
                }
                origin.rb.linearVelocity = Vector2.zero;
            }
        }
        class Moving : State<Player>
        {
            public Moving(Player origin, Layer<Player> parent) : base(origin, parent) { }
            public override void OnStateUpdate()
            {
                base.OnStateUpdate();
                if (InputManager.Instance.Movement.ReadValue<Vector2>().magnitude <= 0.1f)
                {
                    parentLayer.ChangeState("Idle"); return;
                }
                origin.rb.linearVelocity = InputManager.Instance.Movement.ReadValue<Vector2>() * origin.moveSpeed;
            }
        }
        class Dead : State<Player>
        {
            public Dead(Player origin, Layer<Player> parent) : base(origin, parent) { }
            public override void OnStateUpdate()
            {
                base.OnStateUpdate();
                if (!origin.isDead) parentLayer.ChangeState("Idle");
            }
        }
    }
}
[System.Serializable]
public enum Team
{
    None = 0,
    Red = 1,
    Blue = 2
}
[System.Serializable]
public enum Relation
{
    None = 0,
    Enemy = 1,
    Ally = 2,
    Self = 3
}
