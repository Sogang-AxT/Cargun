using UnityEngine;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using UnityEngine.Events;

#if !UNITY_WEBGL || UNITY_EDITOR
using SocketIOClient;
using Newtonsoft.Json.Linq;
#endif

[System.Serializable]
public class ServerGameData
{
    public int totalPlayer;
    public float[] turretRotation;
    public bool[] turretShoot;
    public int[] turretPlayer;
}

public class ServerManager : MonoBehaviour {
    public static UnityEvent<GCEnumManager.GAMEPHASE> OnBroadcastPhaseChange = new();
    private readonly string _serverURL = "https://mgtul.duckdns.org";

    private bool _isConnected;

#if UNITY_EDITOR
    private System.Uri _serverURI;
    private SocketIOUnity _socket;
    private Queue<Action> _mainThreadActions;
    private object _queueLock;
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void InitSocketIO(string url);
    [DllImport("__Internal")] private static extern bool RegisterUnityClient();
    [DllImport("__Internal")] private static extern bool EmitToServer(string eventName, string jsonData);
    [DllImport("__Internal")] private static extern bool IsSocketConnected();
    [DllImport("__Internal")] private static extern void DisconnectSocket();
#endif

    private void Awake()
    {
        OnBroadcastPhaseChange.AddListener(BroadcastPhaseChange);
        _isConnected = false;

#if UNITY_EDITOR
        _mainThreadActions = new Queue<Action>();
        _queueLock = new object();
#endif

        ServerDataManager.Turret_Rotation = new float[4];
        ServerDataManager.Turret_Shoot = new bool[4];
        ServerDataManager.Turret_Player = new int[4];
    }

    private void Start()
    {
#if UNITY_EDITOR
        ConnectToServerEditor();
#endif
#if UNITY_WEBGL && !UNITY_EDITOR
        ConnectToServerWebGL();
#endif
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (_mainThreadActions != null && _queueLock != null)
        {
            lock (_queueLock)
            {
                while (_mainThreadActions.Count > 0)
                {
                    var action = _mainThreadActions.Dequeue();
                    action?.Invoke();
                }
            }
        }
#endif
    }

#if UNITY_EDITOR
    // ==================== UNITY EDITOR 전용 코드 ====================

    private void ConnectToServerEditor()
    {
        _serverURI = new System.Uri(_serverURL);
        _socket = new SocketIOUnity(_serverURI);

        _socket.OnConnected += (sender, e) => {
            _isConnected = true;
            Debug.Log("✅ 서버 연결 성공! (Editor)");

            var unityClient = new JObject
            {
                ["connectType"] = "UNITY_EDITOR"
            };
            _socket.Emit("registerUnity", unityClient);
        };

        _socket.OnDisconnected += (sender, e) => {
            _isConnected = false;
            Debug.Log("❌ 서버 연결 끊김 (Editor)");
        };

        // ✅ 게임 데이터 수신
        _socket.On("gameData", (response) => {
            RunOnMainThread(() => {
                try
                {
                    var rawJson = response.ToString();

                    if (rawJson.StartsWith("[") && rawJson.EndsWith("]"))
                    {
                        rawJson = rawJson.Substring(1, rawJson.Length - 2);
                    }

                    var jObjectData = JObject.Parse(rawJson);

                    ServerDataManager.TotalPlayer = jObjectData["totalPlayer"]?.ToObject<int>() ?? 0;

                    var turretRotationArray = jObjectData["turretRotation"]?.ToObject<float[]>();
                    if (turretRotationArray != null && turretRotationArray.Length == 4)
                    {
                        ServerDataManager.Turret_Rotation = turretRotationArray;
                    }

                    var turretShootArray = jObjectData["turretShoot"]?.ToObject<bool[]>();
                    if (turretShootArray != null && turretShootArray.Length == 4)
                    {
                        ServerDataManager.Turret_Shoot = turretShootArray;
                    }

                    var turretPlayerArray = jObjectData["turretPlayer"]?.ToObject<int[]>();
                    if (turretPlayerArray != null && turretPlayerArray.Length == 4)
                    {
                        ServerDataManager.Turret_Player = turretPlayerArray;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("[Error] Game Data Parsing Error: " + ex.Message);
                }
            });
        });

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
                    
                    // TODO: 플레이어 로그인; TotalPlayer 갱신하도록 수정해야
                    GameManager.OnPlayerJoin.Invoke(data.Count);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("[Error] Player List Parsing Error: " + ex.Message);
                }
            });
        });

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
        if (_queueLock != null && _mainThreadActions != null)
        {
            lock (_queueLock)
            {
                _mainThreadActions.Enqueue(action);
            }
        }
    }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
    // ==================== UNITY WEBGL 전용 코드 ====================
    
    private void ConnectToServerWebGL() {
        Debug.Log("🌐 WebGL 모드로 서버 연결 시작");
        InitSocketIO(_serverURL);
    }

    public void OnSocketConnected() {
        _isConnected = true;
        Debug.Log("✅ 서버 연결 성공! (WebGL)");
        RegisterUnityClient();
    }

    public void OnSocketDisconnected() {
        _isConnected = false;
        Debug.Log("❌ 서버 연결 끊김 (WebGL)");
    }

    public void OnGameDataReceived(string jsonData) {
        try {
            var parser = new SimpleJSONParser();
            var data = parser.Parse(jsonData) as Dictionary<string, object>;

            if (data == null) return;

            ServerDataManager.TotalPlayer = data.ContainsKey("totalPlayer") ? 
                System.Convert.ToInt32(data["totalPlayer"]) : 0;

            if (data.ContainsKey("turretRotation") && data["turretRotation"] is List<object> turretRotationList) {
                ServerDataManager.Turret_Rotation = new float[4];
                for (int i = 0; i < System.Math.Min(turretRotationList.Count, 4); i++) {
                    ServerDataManager.Turret_Rotation[i] = System.Convert.ToSingle(turretRotationList[i]);
                }
            }

            if (data.ContainsKey("turretShoot") && data["turretShoot"] is List<object> turretShootList) {
                ServerDataManager.Turret_Shoot = new bool[4];
                for (int i = 0; i < System.Math.Min(turretShootList.Count, 4); i++) {
                    ServerDataManager.Turret_Shoot[i] = System.Convert.ToBoolean(turretShootList[i]);
                }
            }

            if (data.ContainsKey("turretPlayer") && data["turretPlayer"] is List<object> turretPlayerList) {
                ServerDataManager.Turret_Player = new int[4];
                for (int i = 0; i < System.Math.Min(turretPlayerList.Count, 4); i++) {
                    ServerDataManager.Turret_Player[i] = System.Convert.ToInt32(turretPlayerList[i]);
                }
            }

            Debug.Log($"[GameData] TotalPlayer: {ServerDataManager.TotalPlayer}");
        }
        catch (System.Exception ex) {
            Debug.LogError("[Error] WebGL Game Data Parsing Error: " + ex.Message);
        }
    }

    public void OnPhaseChanged(string phase) {
        Debug.Log($"[PhaseChange] Phase: {phase}");
    }
    
    // ==================== SimpleJSONParser (WebGL 전용) ====================
    
    class SimpleJSONParser {
        private StringReader json;

        enum TOKEN {
            NONE, CURLY_OPEN, CURLY_CLOSE, SQUARED_OPEN, SQUARED_CLOSE,
            COLON, COMMA, STRING, NUMBER, TRUE, FALSE, NULL
        }

        const string WHITE_SPACE = " \t\n\r";
        const string WORD_BREAK = " \t\n\r{}[],:\"";

        public object Parse(string jsonString) {
            json = new StringReader(jsonString);
            return ParseValue();
        }

        Dictionary<string, object> ParseObject() {
            Dictionary<string, object> table = new Dictionary<string, object>();
            json.Read();

            while (true) {
                switch (NextToken) {
                    case TOKEN.NONE:
                        return null;
                    case TOKEN.COMMA:
                        continue;
                    case TOKEN.CURLY_CLOSE:
                        return table;
                    default:
                        string name = ParseString();
                        if (name == null) return null;
                        if (NextToken != TOKEN.COLON) return null;
                        json.Read();
                        table[name] = ParseValue();
                        break;
                }
            }
        }

        List<object> ParseArray() {
            List<object> array = new List<object>();
            json.Read();

            var parsing = true;
            while (parsing) {
                TOKEN nextToken = NextToken;
                switch (nextToken) {
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

        object ParseValue() {
            TOKEN nextToken = NextToken;
            return ParseByToken(nextToken);
        }

        object ParseByToken(TOKEN token) {
            switch (token) {
                case TOKEN.STRING: return ParseString();
                case TOKEN.NUMBER: return ParseNumber();
                case TOKEN.CURLY_OPEN: return ParseObject();
                case TOKEN.SQUARED_OPEN: return ParseArray();
                case TOKEN.TRUE: return true;
                case TOKEN.FALSE: return false;
                case TOKEN.NULL: return null;
                default: return null;
            }
        }

        string ParseString() {
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            char c;
            json.Read();

            bool parsing = true;
            while (parsing) {
                if (json.Peek() == -1) break;
                c = NextChar;
                switch (c) {
                    case '"':
                        parsing = false;
                        break;
                    case '\\':
                        if (json.Peek() == -1) {
                            parsing = false;
                            break;
                        }
                        c = NextChar;
                        switch (c) {
                            case '"':
                            case '\\':
                            case '/':
                                s.Append(c);
                                break;
                            case 'b': s.Append('\b'); break;
                            case 'f': s.Append('\f'); break;
                            case 'n': s.Append('\n'); break;
                            case 'r': s.Append('\r'); break;
                            case 't': s.Append('\t'); break;
                        }
                        break;
                    default:
                        s.Append(c);
                        break;
                }
            }
            return s.ToString();
        }

        object ParseNumber() {
            string number = NextWord;
            if (number.IndexOf('.') == -1) {
                long parsedInt;
                long.TryParse(number, out parsedInt);
                return parsedInt;
            }
            double parsedDouble;
            double.TryParse(number, out parsedDouble);
            return parsedDouble;
        }

        void EatWhitespace() {
            while (WHITE_SPACE.IndexOf(PeekChar) != -1) {
                json.Read();
                if (json.Peek() == -1) break;
            }
        }

        char PeekChar {
            get { return System.Convert.ToChar(json.Peek()); }
        }

        char NextChar {
            get { return System.Convert.ToChar(json.Read()); }
        }

        string NextWord {
            get {
                System.Text.StringBuilder word = new System.Text.StringBuilder();
                while (WORD_BREAK.IndexOf(PeekChar) == -1) {
                    word.Append(NextChar);
                    if (json.Peek() == -1) break;
                }
                return word.ToString();
            }
        }

        TOKEN NextToken {
            get {
                EatWhitespace();
                if (json.Peek() == -1) return TOKEN.NONE;

                char c = PeekChar;
                switch (c) {
                    case '{': return TOKEN.CURLY_OPEN;
                    case '}': json.Read(); return TOKEN.CURLY_CLOSE;
                    case '[': return TOKEN.SQUARED_OPEN;
                    case ']': json.Read(); return TOKEN.SQUARED_CLOSE;
                    case ',': json.Read(); return TOKEN.COMMA;
                    case '"': return TOKEN.STRING;
                    case ':': return TOKEN.COLON;
                    case '0': case '1': case '2': case '3': case '4':
                    case '5': case '6': case '7': case '8': case '9':
                    case '-': return TOKEN.NUMBER;
                }

                string word = NextWord;
                switch (word) {
                    case "false": return TOKEN.FALSE;
                    case "true": return TOKEN.TRUE;
                    case "null": return TOKEN.NULL;
                }
                return TOKEN.NONE;
            }
        }
    }

    class StringReader : System.IDisposable {
        string str;
        int position;

        public StringReader(string s) {
            str = s;
            position = 0;
        }

        public int Peek() {
            if (position >= str.Length) return -1;
            return str[position];
        }

        public int Read() {
            if (position >= str.Length) return -1;
            return str[position++];
        }

        public void Dispose() {
            str = null;
        }
    }
#endif

    // ==================== 공통 코드 ====================

    public void BroadcastPhaseChange(GCEnumManager.GAMEPHASE phaseType)
    {
        var phase = phaseType.ToString().ToLower();
#if UNITY_WEBGL && !UNITY_EDITOR
        EmitToServer("phaseChange", $"\"{phase}\"");
        Debug.Log($"[BroadcastPhase] Phase 전송: {phase} (WebGL)");
#endif
#if UNITY_EDITOR
        if (_socket != null && _isConnected)
        {
            _socket.Emit("phaseChange", phase);
            Debug.Log($"[BroadcastPhase] Phase 전송: {phase} (Editor)");
        }
#endif
    }

    private void OnApplicationQuit()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        DisconnectSocket();
#endif
#if UNITY_EDITOR
        if (_socket != null)
        {
            _socket.Disconnect();
        }
#endif
    }
}