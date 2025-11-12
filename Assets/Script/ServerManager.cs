using UnityEngine;
using System.Collections.Generic;
using System;

#if !UNITY_WEBGL || UNITY_EDITOR
using SocketIOClient;
using Newtonsoft.Json.Linq;
#endif

[System.Serializable]
public class ServerGameData
{
    public int totalPlayer;
    public int shipShield;
    public int shipAttack;
    public int[] startItem;
    public float[] turretRotation;
    public bool[] turretShoot;
    public int[] turretPlayer;
}

public class ServerManager : MonoBehaviour
{
    private readonly string _serverURL = "https://mgtul.duckdns.org";

    private System.Uri _serverURI;
    private bool _isConnected;

#if UNITY_EDITOR
    private SocketIOUnity _socket;
    private Queue<Action> _mainThreadActions;
    private object _queueLock;
#endif

    private void Awake()
    {
        _serverURI = null;
        _isConnected = false;

        _mainThreadActions = new Queue<Action>();
        _queueLock = new object();

        // ✅ ServerDataManager 배열 초기화
        ServerDataManager.Start_Item = new int[6];
        ServerDataManager.Turret_Rotation = new float[4];
        ServerDataManager.Turret_Shoot = new bool[4];
        ServerDataManager.Turret_Player = new int[4];
    }

    private void Start()
    {
#if UNITY_EDITOR
        ConnectToServerEditor();
#endif
    }

    private void Update()
    {
#if UNITY_EDITOR
        lock (_queueLock)
        {
            while (_mainThreadActions.Count > 0)
            {
                var action = _mainThreadActions.Dequeue();
                action?.Invoke();
            }
        }
#endif
    }

    private void ConnectToServerEditor()
    {
        _serverURI = new System.Uri(_serverURL);
        _socket = new SocketIOUnity(_serverURI);

        _socket.OnConnected += (sender, e) => {
            _isConnected = true;
            Debug.Log("✅ 서버 연결 성공!");

            var unityClient = new JObject
            {
                ["connectType"] = "UNITY_EDITOR"
            };
            _socket.Emit("registerUnity", unityClient);
        };

        _socket.OnDisconnected += (sender, e) => {
            _isConnected = false;
            Debug.Log("❌ 서버 연결 끊김");
        };

        // ✅ 게임 데이터 수신 (ServerDataManager에 저장)
        _socket.On("gameData", (response) => {
            RunOnMainThread(() => {
                try
                {
                    var rawJson = response.ToString();

                    // ✅ 배열 형식이면 첫 번째 요소 추출
                    if (rawJson.StartsWith("[") && rawJson.EndsWith("]"))
                    {
                        rawJson = rawJson.Substring(1, rawJson.Length - 2);
                    }

                    var jObjectData = JObject.Parse(rawJson);

                    // TotalPlayer
                    ServerDataManager.TotalPlayer = jObjectData["totalPlayer"]?.ToObject<int>() ?? 0;

                    // Ship Attributes
                    ServerDataManager.Ship_Shield = jObjectData["shipShield"]?.ToObject<int>() ?? 0;
                    ServerDataManager.Ship_Attack = jObjectData["shipAttack"]?.ToObject<int>() ?? 0;

                    // Start Items
                    var startItemArray = jObjectData["startItem"]?.ToObject<int[]>();
                    if (startItemArray != null && startItemArray.Length == 6)
                    {
                        ServerDataManager.Start_Item = startItemArray;
                    }

                    // Turret Rotation
                    var turretRotationArray = jObjectData["turretRotation"]?.ToObject<float[]>();
                    if (turretRotationArray != null && turretRotationArray.Length == 4)
                    {
                        ServerDataManager.Turret_Rotation = turretRotationArray;
                    }

                    // Turret Shoot
                    var turretShootArray = jObjectData["turretShoot"]?.ToObject<bool[]>();
                    if (turretShootArray != null && turretShootArray.Length == 4)
                    {
                        ServerDataManager.Turret_Shoot = turretShootArray;
                    }

                    // Turret Player
                    var turretPlayerArray = jObjectData["turretPlayer"]?.ToObject<int[]>();
                    if (turretPlayerArray != null && turretPlayerArray.Length == 4)
                    {
                        ServerDataManager.Turret_Player = turretPlayerArray;
                    }

                    Debug.Log($"[GameData] TotalPlayer: {ServerDataManager.TotalPlayer}, " +
                             $"Shield: {ServerDataManager.Ship_Shield}, Attack: {ServerDataManager.Ship_Attack}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("[Error] Game Data Parsing Error: " + ex.Message);
                }
            });
        });

        // ✅ 플레이어 목록 수신 (필요시 사용)
        _socket.On("playerList", (response) => {
            RunOnMainThread(() => {
                try
                {
                    var rawJsonPlayerList = response.ToString();

                    if (rawJsonPlayerList.StartsWith("[") && rawJsonPlayerList.EndsWith("]"))
                    {
                        rawJsonPlayerList = rawJsonPlayerList.Substring(1, rawJsonPlayerList.Length - 2);
                    }

                    if (string.IsNullOrEmpty(rawJsonPlayerList) || rawJsonPlayerList == "{}")
                    {
                        return;
                    }

                    var data = JObject.Parse(rawJsonPlayerList);
                    Debug.Log($"[PlayerList] 플레이어 수: {data.Count}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("[Error] Player List Parsing Error: " + ex.Message);
                }
            });
        });

        // ✅ Phase 변경 수신
        _socket.On("phaseChange", (response) => {
            RunOnMainThread(() => {
                try
                {
                    var phase = response.ToString().Trim('"');
                    Debug.Log($"[PhaseChange] Phase: {phase}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("[Error] Phase Change Error: " + ex.Message);
                }
            });
        });

        _socket.Connect();
    }

    private void RunOnMainThread(Action action)
    {
        lock (_queueLock)
        {
            _mainThreadActions.Enqueue(action);
        }
    }

    // ✅ Phase 변경 브로드캐스트
    public void BroadcastPhaseChange(string phase)
    {
#if UNITY_EDITOR
        if (_socket != null && _isConnected)
        {
            _socket.Emit("phaseChange", phase);
            Debug.Log($"[BroadcastPhase] Phase 전송: {phase}");
        }
#endif
    }

    private void OnApplicationQuit()
    {
#if UNITY_EDITOR
        if (_socket != null)
        {
            _socket.Disconnect();
        }
#endif
    }
}