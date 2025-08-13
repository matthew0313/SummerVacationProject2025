using System;
using System.Linq;
using NetworkAudioManagement;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.UI.Image;

[RequireComponent(typeof(Rigidbody2D), typeof(NetworkTransform), typeof(NetworkRigidbody2D))]
[RequireComponent(typeof(Player_HpComp), typeof(Player_WardComp), typeof(Player_Inventory))]
[RequireComponent(typeof(Player_Animations))]
public class Player : Entity
{
    public static Player localPlayer;
    public Player_HpComp hpComp { get; private set; }
    public Player_WardComp wardComp { get; private set; }
    public Player_Inventory inventory { get; private set; }
    public Player_Animations animations { get; private set; }

    public Animator anim;
    Rigidbody2D rb;
    Camera mainCam;

    [Header("Movements")]
    [SerializeField] float moveSpeed = 5.0f;
    [SerializeField] Transform rotator, flipped;
    [SerializeField] float footstepDistMin = 3.0f, footstepDistMax = 5.0f;

    [Header("Potions")]
    [SerializeField] KeyCode potionUseKey;

    public readonly NetworkVariable<Team> team = new(Team.None);
    public readonly NetworkVariable<bool> PVP = new(false), inRound = new(false);

    public readonly NetworkVariable<int> playerKills = new(0);
    public readonly NetworkVariable<int> deaths = new(0);
    [Rpc(SendTo.Authority)] public void SetTeamRpc(Team team) => this.team.Value = team;
    [Rpc(SendTo.Authority)] public void SetPVPRpc(bool PVP) => this.PVP.Value = PVP;
    [Rpc(SendTo.Authority)] public void SetInRoundRpc(bool inRound) => this.inRound.Value = inRound;

    [Rpc(SendTo.Authority)]
    public void ResetRoundStatsRpc()
    {
        playerKills.Value = 0;
        deaths.Value = 0;
    }
    bool isDead => hpComp.isDead.Value;
    TopLayer topLayer;

    private void Awake()
    {
        hpComp = GetComponent<Player_HpComp>();
        wardComp = GetComponent<Player_WardComp>();
        inventory = GetComponent<Player_Inventory>();
        animations = GetComponent<Player_Animations>();
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
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
        }
        if (localPlayer != null) anim.SetInteger(relationID, (int)localPlayer.GetRelation(this));
        
    }
    private void OnEnable()
    {
        GameManager.Instance.AddPlayer(this);
    }
    private void OnDisable()
    {
        GameManager.Instance.RemovePlayer(this);
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (IsOwner) topLayer.OnStateExit();
    }

    public Relation GetRelation(Entity other)
    {
        if (other == null) return Relation.None;
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
    void UpdateRotation()
    {
        Vector2 dir = (mainCam.ScreenToWorldPoint(Input.mousePosition) - rotator.position).normalized;
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
    Vector2 MoveInput() => InputManager.Instance.MoveInput();
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
                if (origin.MoveInput().magnitude > 0.1f)
                {
                    parentLayer.ChangeState("Moving"); return;
                }
                origin.rb.linearVelocity = Vector2.zero;
            }
        }
        class Moving : State<Player>
        {
            public Moving(Player origin, Layer<Player> parent) : base(origin, parent) { }
            PlayingSound footsteps = null;
            public override void OnStateEnter()
            {
                base.OnStateEnter();
                origin.animations.isWalking.Value = true;
                footsteps = NetworkAudioManager.Instance.PlaySound("Walking").SetDistance(origin, origin.footstepDistMin, origin.footstepDistMax).SetLoop(true);
            }
            public override void OnStateUpdate()
            {
                base.OnStateUpdate();
                if (origin.MoveInput().magnitude <= 0.1f)
                {
                    parentLayer.ChangeState("Idle"); return;
                }
                origin.rb.linearVelocity = origin.MoveInput() * origin.moveSpeed;
            }
            public override void OnStateExit()
            {
                base.OnStateExit();
                origin.animations.isWalking.Value = false;
                footsteps.Stop();
            }
        }
        class Dead : State<Player>
        {
            public Dead(Player origin, Layer<Player> parent) : base(origin, parent) { }
            public override void OnStateEnter()
            {
                base.OnStateEnter();
                origin.animations.isDead.Value = true;
            }
            public override void OnStateUpdate()
            {
                base.OnStateUpdate();
                if (!origin.isDead) parentLayer.ChangeState("Idle");
            }
            public override void OnStateExit()
            {
                base.OnStateExit();
                origin.animations.isDead.Value = false;
            }
        }
    }

    //Kills
    public event Action<Entity> onKill;
    [Rpc(SendTo.Owner)]
    public void OnKillRpc(NetworkBehaviourReference killedRef)
    {
        if (!killedRef.TryGet(out Entity killed)) return;
        onKill?.Invoke(killed);
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
