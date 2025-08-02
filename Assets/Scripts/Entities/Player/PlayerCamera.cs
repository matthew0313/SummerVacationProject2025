using System.Collections;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
public class PlayerCamera : MonoBehaviour, ILocalPlayerSpawnCallback
{
    CinemachineCamera cineCamera;

    [SerializeField] KeyCode trackPlayerKey = KeyCode.Space;
    [SerializeField] float freeTrackZ = -10.0f;
    [SerializeField] float freeTrackLerpRate = 3.0f;
    public bool trackingPlayer { get; private set; } = true;

    TopLayer topLayer;
    Player player;
    Vector2 trackingPos = Vector2.zero;
    private void Awake()
    {
        cineCamera = GetComponent<CinemachineCamera>();
    }
    public void OnLocalPlayerSpawn(Player localPlayer)
    {
        this.player = localPlayer;
        topLayer = new(this);
        topLayer.OnStateEnter();
    }
    private void Update()
    {
        if (player == null) return;
        topLayer.OnStateUpdate();
        if (Input.GetKeyDown(trackPlayerKey))
        {
            topLayer.ChangeState("TrackingPlayer");
        }
    }
    public void FreeTrack(Vector2 targetPos)
    {
        if (player == null) return;
        trackingPos = targetPos;
        topLayer.ChangeState("FreeTrack");
    }
    class TopLayer : TopLayer<PlayerCamera>
    {
        public TopLayer(PlayerCamera origin) : base(origin)
        {
            defaultState = new TrackingPlayer(origin, this);
            AddState("TrackingPlayer", defaultState);
            AddState("FreeTrack", new FreeTrack(origin, this));
        }
        class TrackingPlayer : State<PlayerCamera>
        {
            public TrackingPlayer(PlayerCamera origin, Layer<PlayerCamera> parent) : base(origin, parent) { }
            public override void OnStateEnter()
            {
                base.OnStateEnter();
                origin.trackingPlayer = true;
                origin.cineCamera.Target.TrackingTarget = origin.player.transform;
            }
            public override void OnStateExit()
            {
                base.OnStateExit();
                origin.trackingPlayer = false;
                origin.cineCamera.Target.TrackingTarget = null;
            }
        }
        class FreeTrack : State<PlayerCamera>
        {
            public FreeTrack(PlayerCamera origin, Layer<PlayerCamera> parent) : base(origin, parent) { }
            public override void OnStateUpdate()
            {
                base.OnStateUpdate();
                Vector2 pos = Vector2.Lerp(origin.transform.position, origin.trackingPos, origin.freeTrackLerpRate * Time.deltaTime);
                origin.transform.position = new Vector3(pos.x, pos.y, origin.freeTrackZ);
            }
        }
    }
}