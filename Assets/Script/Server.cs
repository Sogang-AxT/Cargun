using UnityEngine;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;

#if !UNITY_WEBGL || UNITY_EDITOR
using SocketIOClient;
using Newtonsoft.Json.Linq;
#endif

// 플레이어 데이터 구조
[System.Serializable]
public class PlayerData
{
    public string id;
    public string nickname;
    public string color;
    public string turret;
}

// 터렛 각도 업데이트 데이터
[System.Serializable]
public class TurretAngleData
{
    public string playerId;
    public string turret;
    public float angle;
}

// 업그레이드 데이터
[System.Serializable]
public class UpgradeData
{
    public string turret;
    public string upgradeType;
    public int level;
}

public class Server : MonoBehaviour
{
    public static Server Instance;

    [Header("Server Settings")]
    public string serverURL = "https://mgtul.duckdns.org";

    [Header("Game State")]
    public Dictionary<string, PlayerData> players = new Dictionary<string, PlayerData>();
    public int playerCount = 0;

    [Header("Upgrade State")]
    public Dictionary<string, int[]> upgradeStates = new Dictionary<string, int[]>();

#if !UNITY_WEBGL || UNITY_EDITOR
    // 에디터용 SocketIOClient
    private SocketIOUnity socket;
    private Queue<Action> mainThreadActions = new Queue<Action>();
    private object queueLock = new object();
#endif

    private bool isConnected = false;

#if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL용 JavaScript 함수 import
    [DllImport("__Internal")]
    private static extern void InitSocketIO(string url);
    
    [DllImport("__Internal")]
    private static extern bool RegisterUnityClient();
    
    [DllImport("__Internal")]
    private static extern bool EmitToServer(string eventName, string jsonData);
    
    [DllImport("__Internal")]
    private static extern bool IsSocketConnected();
    
    [DllImport("__Internal")]
    private static extern void DisconnectSocket();
#endif

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 업그레이드 초기화
        upgradeStates["A"] = new int[4] { 0, 0, 0, 0 };
        upgradeStates["B"] = new int[4] { 0, 0, 0, 0 };
        upgradeStates["C"] = new int[4] { 0, 0, 0, 0 };
        upgradeStates["D"] = new int[4] { 0, 0, 0, 0 };
    }

    void Start()
    {
        ConnectToServer();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        // 에디터: 메인 스레드에서 큐 처리
        lock (queueLock)
        {
            while (mainThreadActions.Count > 0)
            {
                var action = mainThreadActions.Dequeue();
                action?.Invoke();
            }
        }
#endif
    }

    void ConnectToServer()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL 빌드
        Debug.Log("🌐 WebGL 모드: JavaScript Bridge 사용");
        InitSocketIO(serverURL);
#else
        // 에디터 모드
        Debug.Log("🖥️ 에디터 모드: SocketIOClient 사용");
        ConnectToServerEditor();
#endif
    }

#if !UNITY_WEBGL || UNITY_EDITOR
    // 에디터용 연결
    void ConnectToServerEditor()
    {
        var uri = new System.Uri(serverURL);
        socket = new SocketIOUnity(uri);

        socket.OnConnected += (sender, e) =>
        {
            isConnected = true;
            Debug.Log("✅ 서버 연결 성공! (에디터)");

            var unityClient = new JObject();
            unityClient["type"] = "unity";
            socket.Emit("registerUnity", unityClient);
            Debug.Log("Unity 클라이언트 등록 완료");
        };

        socket.OnDisconnected += (sender, e) =>
        {
            isConnected = false;
            Debug.Log("❌ 서버 연결 끊김 (에디터)");
        };

        socket.On("joined", (response) =>
        {
            RunOnMainThread(() =>
            {
                try
                {
                    var data = response.GetValue<JObject>();
                    PlayerData player = new PlayerData
                    {
                        id = data["id"].ToString(),
                        nickname = data["nickname"].ToString(),
                        color = data["color"].ToString(),
                        turret = data["turret"].ToString()
                    };

                    if (!players.ContainsKey(player.id))
                    {
                        players[player.id] = player;
                        playerCount++;
                    }

                    if (Interface.Instance != null)
                    {
                        Interface.Instance.UpdatePlayerList();
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("플레이어 입장 파싱 오류: " + ex.Message);
                }
            });
        });

        socket.On("roomFull", (response) =>
        {
            Debug.Log("방 만원: " + response);
        });

        socket.On("playerList", (response) =>
        {
            RunOnMainThread(() =>
            {
                try
                {
                    string rawJson = response.ToString();

                    if (rawJson.StartsWith("[") && rawJson.EndsWith("]"))
                    {
                        rawJson = rawJson.Substring(1, rawJson.Length - 2);
                    }

                    players.Clear();
                    playerCount = 0;

                    if (string.IsNullOrEmpty(rawJson) || rawJson == "{}")
                    {
                        if (Interface.Instance != null)
                        {
                            Interface.Instance.UpdatePlayerList();
                            Interface.Instance.ShowReadyPhase();
                        }
                        return;
                    }

                    JObject data = JObject.Parse(rawJson);

                    foreach (var kvp in data)
                    {
                        var playerObj = kvp.Value as JObject;
                        if (playerObj != null)
                        {
                            PlayerData player = new PlayerData
                            {
                                id = playerObj["id"]?.ToString() ?? "",
                                nickname = playerObj["nickname"]?.ToString() ?? "Unknown",
                                color = playerObj["color"]?.ToString() ?? "green",
                                turret = playerObj["turret"]?.ToString() ?? "A"
                            };

                            players[player.id] = player;
                            playerCount++;
                        }
                    }

                    if (Interface.Instance != null)
                    {
                        Interface.Instance.UpdatePlayerList();
                        Interface.Instance.ShowReadyPhase();
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"플레이어 목록 파싱 오류: {ex.Message}");
                }
            });
        });

        socket.On("updateTurret", (response) =>
        {
            RunOnMainThread(() =>
            {
                try
                {
                    var data = response.GetValue<JObject>();
                    string turret = data["turret"].ToString();
                    float angle = float.Parse(data["angle"].ToString());

                    if (Turret.Instance != null)
                    {
                        Turret.Instance.UpdateTurretAngle(turret, angle);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("터렛 각도 업데이트 오류: " + ex.Message);
                }
            });
        });

        socket.On("upgradeApplied", (response) =>
        {
            RunOnMainThread(() =>
            {
                try
                {
                    var data = response.GetValue<JObject>();
                    string turret = data["turret"].ToString();
                    string upgradeType = data["upgradeType"].ToString();
                    int level = int.Parse(data["level"].ToString());

                    int upgradeIndex = upgradeType[0] - 'A';
                    if (upgradeStates.ContainsKey(turret))
                    {
                        upgradeStates[turret][upgradeIndex] = level;
                    }

                    Debug.Log($"Turret {turret} - Upgrade {upgradeType} Lv.{level}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("업그레이드 적용 오류: " + ex.Message);
                }
            });
        });

        socket.On("phaseChange", (response) =>
        {
            string phase = response.GetValue<string>();
            RunOnMainThread(() =>
            {
                if (Interface.Instance != null)
                {
                    if (phase == "ready")
                    {
                        Interface.Instance.ShowReadyPhase();
                    }
                    else if (phase == "combat")
                    {
                        Interface.Instance.ShowCombatPhase();
                    }
                }
            });
        });

        socket.Connect();
        Debug.Log("서버 연결 시도 중: " + serverURL);
    }

    void RunOnMainThread(Action action)
    {
        lock (queueLock)
        {
            mainThreadActions.Enqueue(action);
        }
    }
#endif

    // ========== WebGL Callback 함수들 (JavaScript → Unity) ==========

    // 연결 성공
    public void OnSocketConnected(string empty)
    {
        isConnected = true;
        Debug.Log("✅ 서버 연결 성공! (WebGL)");
        // registerUnity는 jslib에서 자동으로 전송됨
    }

    // 연결 끊김
    public void OnSocketDisconnected(string empty)
    {
        isConnected = false;
        Debug.Log("❌ 서버 연결 끊김 (WebGL)");
    }

    // 플레이어 입장
    public void OnPlayerJoined(string json)
    {
        try
        {
            PlayerData player = JsonUtility.FromJson<PlayerData>(json);

            if (!players.ContainsKey(player.id))
            {
                players[player.id] = player;
                playerCount++;
            }

            if (Interface.Instance != null)
            {
                Interface.Instance.UpdatePlayerList();
            }

            Debug.Log($"플레이어 입장: {player.nickname}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("플레이어 입장 파싱 오류: " + ex.Message);
        }
    }

    // 방 꽉참
    public void OnRoomFull(string message)
    {
        Debug.Log("방 만원: " + message);
    }

    // 플레이어 목록 업데이트
    public void OnPlayerListUpdated(string json)
    {
        try
        {
            players.Clear();
            playerCount = 0;

            if (string.IsNullOrEmpty(json) || json == "{}")
            {
                if (Interface.Instance != null)
                {
                    Interface.Instance.UpdatePlayerList();
                    Interface.Instance.ShowReadyPhase();
                }
                return;
            }

            // JSON 파싱 (WebGL에서는 간단한 파싱)
            var dict = MiniJSON.Json.Deserialize(json) as Dictionary<string, object>;

            if (dict != null)
            {
                foreach (var kvp in dict)
                {
                    var playerDict = kvp.Value as Dictionary<string, object>;
                    if (playerDict != null)
                    {
                        PlayerData player = new PlayerData
                        {
                            id = playerDict.ContainsKey("id") ? playerDict["id"].ToString() : "",
                            nickname = playerDict.ContainsKey("nickname") ? playerDict["nickname"].ToString() : "Unknown",
                            color = playerDict.ContainsKey("color") ? playerDict["color"].ToString() : "green",
                            turret = playerDict.ContainsKey("turret") ? playerDict["turret"].ToString() : "A"
                        };

                        players[player.id] = player;
                        playerCount++;
                    }
                }
            }

            if (Interface.Instance != null)
            {
                Interface.Instance.UpdatePlayerList();
                Interface.Instance.ShowReadyPhase();
            }

            Debug.Log($"플레이어 목록 업데이트: {playerCount}명");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("플레이어 목록 파싱 오류: " + ex.Message);
        }
    }

    // 터렛 각도 업데이트
    public void OnTurretAngleUpdated(string json)
    {
        try
        {
            TurretAngleData data = JsonUtility.FromJson<TurretAngleData>(json);

            if (Turret.Instance != null)
            {
                Turret.Instance.UpdateTurretAngle(data.turret, data.angle);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("터렛 각도 업데이트 오류: " + ex.Message);
        }
    }

    // 터렛 전환
    public void OnTurretSwitched(string turret)
    {
        Debug.Log("터렛 전환: " + turret);
    }

    // 업그레이드 적용
    public void OnUpgradeApplied(string json)
    {
        try
        {
            UpgradeData data = JsonUtility.FromJson<UpgradeData>(json);

            int upgradeIndex = data.upgradeType[0] - 'A';
            if (upgradeStates.ContainsKey(data.turret))
            {
                upgradeStates[data.turret][upgradeIndex] = data.level;
            }

            Debug.Log($"Turret {data.turret} - Upgrade {data.upgradeType} Lv.{data.level}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("업그레이드 적용 오류: " + ex.Message);
        }
    }

    // Phase 변경
    public void OnPhaseChanged(string phase)
    {
        Debug.Log("Phase 변경: " + phase);

        if (Interface.Instance != null)
        {
            if (phase == "ready")
            {
                Interface.Instance.ShowReadyPhase();
            }
            else if (phase == "combat")
            {
                Interface.Instance.ShowCombatPhase();
            }
        }
    }

    // ========== 서버로 데이터 전송 (공통) ==========

    public void SendToServer(string eventName, string jsonData = "")
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL: JavaScript 브리지 사용
        if (IsSocketConnected())
        {
            EmitToServer(eventName, jsonData);
        }
        else
        {
            Debug.LogWarning("서버 미연결 상태 (WebGL)");
        }
#else
        // 에디터: SocketIOClient 사용
        if (socket != null && isConnected)
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                socket.Emit(eventName);
            }
            else
            {
                try
                {
                    var data = JObject.Parse(jsonData);
                    socket.Emit(eventName, data);
                    Debug.Log($"📤 전송: {eventName} - {jsonData}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("전송 오류: " + ex.Message);
                }
            }
        }
        else
        {
            Debug.LogWarning("서버 미연결 상태 (에디터)");
        }
#endif
    }

    public void BroadcastPhaseChange(string phase)
    {
        string jsonData = $"{{\"phase\":\"{phase}\"}}";
        SendToServer("phaseChange", jsonData);
    }

    public void BroadcastPlayerList()
    {
        SendToServer("playerList", "{}");
    }

    void OnApplicationQuit()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        DisconnectSocket();
#else
        if (socket != null)
        {
            socket.Disconnect();
        }
#endif
    }

    void OnDestroy()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        DisconnectSocket();
#else
        if (socket != null)
        {
            socket.Disconnect();
        }
#endif
    }
}

// ========== MiniJSON (WebGL용 간단한 JSON 파서) ==========
namespace MiniJSON
{
    public static class Json
    {
        public static object Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            return Parser.Parse(json);
        }
    }

    sealed class Parser : System.IDisposable
    {
        const string WHITE_SPACE = " \t\n\r";
        const string WORD_BREAK = " \t\n\r{}[],:\"";

        enum TOKEN
        {
            NONE,
            CURLY_OPEN,
            CURLY_CLOSE,
            SQUARED_OPEN,
            SQUARED_CLOSE,
            COLON,
            COMMA,
            STRING,
            NUMBER,
            TRUE,
            FALSE,
            NULL
        }

        StringReader json;

        Parser(string jsonString)
        {
            json = new StringReader(jsonString);
        }

        public static object Parse(string jsonString)
        {
            using (var instance = new Parser(jsonString))
            {
                return instance.ParseValue();
            }
        }

        public void Dispose()
        {
            if (json != null)
            {
                json.Dispose();
                json = null;
            }
        }

        Dictionary<string, object> ParseObject()
        {
            Dictionary<string, object> table = new Dictionary<string, object>();

            json.Read();

            while (true)
            {
                switch (NextToken)
                {
                    case TOKEN.NONE:
                        return null;
                    case TOKEN.COMMA:
                        continue;
                    case TOKEN.CURLY_CLOSE:
                        return table;
                    default:
                        string name = ParseString();
                        if (name == null)
                            return null;

                        if (NextToken != TOKEN.COLON)
                            return null;

                        json.Read();
                        table[name] = ParseValue();
                        break;
                }
            }
        }

        List<object> ParseArray()
        {
            List<object> array = new List<object>();

            json.Read();

            var parsing = true;
            while (parsing)
            {
                TOKEN nextToken = NextToken;

                switch (nextToken)
                {
                    case TOKEN.NONE:
                        return null;
                    case TOKEN.COMMA:
                        continue;
                    case TOKEN.SQUARED_CLOSE:
                        parsing = false;
                        break;
                    default:
                        object value = ParseByToken(nextToken);
                        array.Add(value);
                        break;
                }
            }

            return array;
        }

        object ParseValue()
        {
            TOKEN nextToken = NextToken;
            return ParseByToken(nextToken);
        }

        object ParseByToken(TOKEN token)
        {
            switch (token)
            {
                case TOKEN.STRING:
                    return ParseString();
                case TOKEN.NUMBER:
                    return ParseNumber();
                case TOKEN.CURLY_OPEN:
                    return ParseObject();
                case TOKEN.SQUARED_OPEN:
                    return ParseArray();
                case TOKEN.TRUE:
                    return true;
                case TOKEN.FALSE:
                    return false;
                case TOKEN.NULL:
                    return null;
                default:
                    return null;
            }
        }

        string ParseString()
        {
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            char c;

            json.Read();

            bool parsing = true;
            while (parsing)
            {
                if (json.Peek() == -1)
                    break;

                c = NextChar;
                switch (c)
                {
                    case '"':
                        parsing = false;
                        break;
                    case '\\':
                        if (json.Peek() == -1)
                        {
                            parsing = false;
                            break;
                        }

                        c = NextChar;
                        switch (c)
                        {
                            case '"':
                            case '\\':
                            case '/':
                                s.Append(c);
                                break;
                            case 'b':
                                s.Append('\b');
                                break;
                            case 'f':
                                s.Append('\f');
                                break;
                            case 'n':
                                s.Append('\n');
                                break;
                            case 'r':
                                s.Append('\r');
                                break;
                            case 't':
                                s.Append('\t');
                                break;
                        }
                        break;
                    default:
                        s.Append(c);
                        break;
                }
            }

            return s.ToString();
        }

        object ParseNumber()
        {
            string number = NextWord;

            if (number.IndexOf('.') == -1)
            {
                long parsedInt;
                long.TryParse(number, out parsedInt);
                return parsedInt;
            }

            double parsedDouble;
            double.TryParse(number, out parsedDouble);
            return parsedDouble;
        }

        void EatWhitespace()
        {
            while (WHITE_SPACE.IndexOf(PeekChar) != -1)
            {
                json.Read();

                if (json.Peek() == -1)
                    break;
            }
        }

        char PeekChar
        {
            get { return System.Convert.ToChar(json.Peek()); }
        }

        char NextChar
        {
            get { return System.Convert.ToChar(json.Read()); }
        }

        string NextWord
        {
            get
            {
                System.Text.StringBuilder word = new System.Text.StringBuilder();

                while (WORD_BREAK.IndexOf(PeekChar) == -1)
                {
                    word.Append(NextChar);

                    if (json.Peek() == -1)
                        break;
                }

                return word.ToString();
            }
        }

        TOKEN NextToken
        {
            get
            {
                EatWhitespace();

                if (json.Peek() == -1)
                    return TOKEN.NONE;

                char c = PeekChar;
                switch (c)
                {
                    case '{':
                        return TOKEN.CURLY_OPEN;
                    case '}':
                        json.Read();
                        return TOKEN.CURLY_CLOSE;
                    case '[':
                        return TOKEN.SQUARED_OPEN;
                    case ']':
                        json.Read();
                        return TOKEN.SQUARED_CLOSE;
                    case ',':
                        json.Read();
                        return TOKEN.COMMA;
                    case '"':
                        return TOKEN.STRING;
                    case ':':
                        return TOKEN.COLON;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '-':
                        return TOKEN.NUMBER;
                }

                string word = NextWord;

                switch (word)
                {
                    case "false":
                        return TOKEN.FALSE;
                    case "true":
                        return TOKEN.TRUE;
                    case "null":
                        return TOKEN.NULL;
                }

                return TOKEN.NONE;
            }
        }
    }

    class StringReader : System.IDisposable
    {
        string str;
        int position;

        public StringReader(string s)
        {
            str = s;
            position = 0;
        }

        public int Peek()
        {
            if (position >= str.Length)
                return -1;

            return str[position];
        }

        public int Read()
        {
            if (position >= str.Length)
                return -1;

            return str[position++];
        }

        public void Dispose()
        {
            str = null;
        }
    }
}