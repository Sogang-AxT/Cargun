using UnityEngine;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using UnityEngine.Events;

#if !UNITY_WEBGL || UNITY_EDITOR
using SocketIOClient;
using Newtonsoft.Json.Linq;
#endif

// // 플레이어 데이터 구조
// [System.Serializable]
// public class PlayerData {
//     public string id;
//     public string nickname;
//     public string color;
//     public string turret;
//     public int slot; // 플레이어 슬롯 번호 (1~4)
// }

// // 터렛 각도 업데이트 데이터
// [System.Serializable]
// public class TurretAngleData {
//     public string playerId;
//     public string turret;
//     public float angle;
// }
//
// // 업그레이드 데이터
// [System.Serializable]
// public class UpgradeData {
//     public string turret;
//     public string upgradeType;
//     public int level;
// }

public class ServerManager : GC_SingletonImplementer<ServerManager> {
    public static UnityEvent<GC_EnumManager.GAMEPHASE> OnBroadcastPhaseChange;
    
    private readonly string _serverURL = "https://mgtul.duckdns.org";
    
    public Dictionary<string, PlayerData> players = new Dictionary<string, PlayerData>();
    public int playerCount = 0;
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

    void Awake() {
        // 업그레이드 초기화
        upgradeStates["A"] = new int[4] { 0, 0, 0, 0 };
        upgradeStates["B"] = new int[4] { 0, 0, 0, 0 };
        upgradeStates["C"] = new int[4] { 0, 0, 0, 0 };
        upgradeStates["D"] = new int[4] { 0, 0, 0, 0 };
    }

    void Start() {
        ConnectToServer();
    }

    void Update() {
#if !UNITY_WEBGL || UNITY_EDITOR
        // 에디터: 메인 스레드에서 큐 처리
        lock (queueLock) {
            while (mainThreadActions.Count > 0) {
                var action = mainThreadActions.Dequeue();
                action?.Invoke();
            }
        }
#endif
    }

    void ConnectToServer() {
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
    void ConnectToServerEditor() {
        var uri = new System.Uri(_serverURL);
        socket = new SocketIOUnity(uri);

        socket.OnConnected += (sender, e) => {
            isConnected = true;
            Debug.Log("✅ 서버 연결 성공! (에디터)");

            // ✅ Unity의 현재 Phase를 서버에 전송
            string currentGamePhase = "ready";
            if (GameManager.Instance != null) {
                currentGamePhase = GameManager.Instance.currentPhase;
            }

            var unityClient = new JObject();
            unityClient["type"] = "unity";
            unityClient["currentPhase"] = currentGamePhase; // ✅ 이 줄 추가!

            socket.Emit("registerUnity", unityClient);
            Debug.Log($"Unity 클라이언트 등록 완료 (Phase: {currentGamePhase})");
        };

        socket.OnDisconnected += (sender, e) => {
            isConnected = false;
            Debug.Log("❌ 서버 연결 끊김 (에디터)");
        };

        socket.On("joined", (response) => {
            RunOnMainThread(() => {
                try {
                    var data = response.GetValue<JObject>();
                    PlayerData player = new PlayerData {
                        id = data["id"].ToString(),
                        nickname = data["nickname"].ToString(),
                        color = data["color"].ToString(),
                        turret = data["turret"].ToString()
                    };

                    if (!players.ContainsKey(player.id)) {
                        players[player.id] = player;
                        playerCount++;
                    }

                    if (Interface.Instance != null) {
                        Interface.Instance.UpdatePlayerList();
                    }
                }
                catch (System.Exception ex) {
                    Debug.LogError("플레이어 입장 파싱 오류: " + ex.Message);
                }
            });
        });

        socket.On("roomFull", (response) => {
            Debug.Log("방 만원: " + response);
        });

        socket.On("playerList", (response) => {
            RunOnMainThread(() => {
                try {
                    string rawJson = response.ToString();

                    if (rawJson.StartsWith("[") && rawJson.EndsWith("]")) {
                        rawJson = rawJson.Substring(1, rawJson.Length - 2);
                    }

                    players.Clear();
                    playerCount = 0;

                    if (string.IsNullOrEmpty(rawJson) || rawJson == "{}") {
                        if (Interface.Instance != null) {
                            Interface.Instance.UpdatePlayerList();
                            // ✅ Ready Phase일 때만 ShowReadyPhase 호출
                            if (GameManager.Instance != null && GameManager.Instance.currentPhase == "ready") {
                                Interface.Instance.ShowReadyPhase();
                            }
                        }
                        return;
                    }

                    JObject data = JObject.Parse(rawJson);

                    foreach (var kvp in data) {
                        var playerObj = kvp.Value as JObject;
                        if (playerObj != null) {
                            PlayerData player = new PlayerData {
                                id = playerObj["id"]?.ToString() ?? "",
                                nickname = playerObj["nickname"]?.ToString() ?? "Unknown",
                                color = playerObj["color"]?.ToString() ?? "green",
                                turret = playerObj["turret"]?.ToString() ?? "A",
                                slot = int.Parse(playerObj["slot"]?.ToString() ?? "1")
                            };

                            players[player.id] = player;
                            playerCount++;
                        }
                    }

                    if (Interface.Instance != null) {
                        Interface.Instance.UpdatePlayerList();
                        // ✅ Ready Phase일 때만 ShowReadyPhase 호출
                        if (GameManager.Instance != null && GameManager.Instance.currentPhase == "ready") {
                            Interface.Instance.ShowReadyPhase();
                        }
                    }
                }
                catch (System.Exception ex) {
                    Debug.LogError($"플레이어 목록 파싱 오류: {ex.Message}");
                }
            });
        });

        // ✅ updateTurret 이벤트 수정 - JSON 파싱 방식 변경
        socket.On("updateTurret", (response) => {
            RunOnMainThread(() => {
                try {
                    string rawJson = response.ToString();
                    if (rawJson.StartsWith("[") && rawJson.EndsWith("]")) {
                        rawJson = rawJson.Substring(1, rawJson.Length - 2);
                    }

                    var data = JObject.Parse(rawJson);
                    string turret = data["turret"]?.ToString() ?? "";
                    float angle = float.Parse(data["angle"]?.ToString() ?? "0");

                    if (Turret.Instance != null) {
                        Turret.Instance.UpdateTurretAngle(turret, angle);
                    }
                }
                catch (System.Exception ex) {
                    Debug.LogError("터렛 각도 업데이트 오류: " + ex.Message);
                }
            });
        });

        socket.On("upgradeApplied", (response) => {
            RunOnMainThread(() => {
                try {
                    var data = response.GetValue<JObject>();
                    string turret = data["turret"].ToString();
                    string upgradeType = data["upgradeType"].ToString();
                    int level = int.Parse(data["level"].ToString());

                    int upgradeIndex = upgradeType[0] - 'A';
                    if (upgradeIndex >= 0 && upgradeIndex < 4) {
                        upgradeStates[turret][upgradeIndex] = level;
                    }

                    Debug.Log($"업그레이드 적용: Turret {turret} - {upgradeType} Lv.{level}");
                }
                catch (System.Exception ex) {
                    Debug.LogError("업그레이드 적용 오류: " + ex.Message);
                }
            });
        });

        socket.On("phaseChange", (response) => {
            RunOnMainThread(() => {
                try {
                    string phase = response.ToString().Trim('"');
                    Debug.Log($"Phase 변경 수신: {phase}");

                    if (GameManager.Instance != null) {
                        // GameManager.Instance.currentPhase = phase; // ✅ 제거: GameManager가 자체 관리
                    }

                    if (Interface.Instance != null) {
                        if (phase == "ready") {
                            Interface.Instance.ShowReadyPhase();
                        }
                        else if (phase == "combat") {
                            Interface.Instance.ShowCombatPhase();
                        }
                    }
                }
                catch (System.Exception ex) {
                    Debug.LogError("Phase 변경 오류: " + ex.Message);
                }
            });
        });

        socket.Connect();
    }

    void RunOnMainThread(Action action) {
        lock (queueLock) {
            mainThreadActions.Enqueue(action);
        }
    }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL 빌드용 함수들
    public void OnSocketConnected() {
        isConnected = true;
        Debug.Log("✅ 서버 연결 성공! (WebGL)");
        RegisterUnityClient();
    }

    public void OnSocketDisconnected() {
        isConnected = false;
        Debug.Log("❌ 서버 연결 끊김 (WebGL)");
    }

    public void OnPlayerJoined(string jsonData) {
        try {
            var parser = new SimpleJSONParser();
            var data = parser.Parse(jsonData) as Dictionary<string, object>;

            if (data == null) return;

            PlayerData player = new PlayerData {
                id = data.ContainsKey("id") ? (string)data["id"] : "",
                nickname = data.ContainsKey("nickname") ? (string)data["nickname"] : "Unknown",
                color = data.ContainsKey("color") ? (string)data["color"] : "green",
                turret = data.ContainsKey("turret") ? (string)data["turret"] : "A"
            };

            if (!players.ContainsKey(player.id)) {
                players[player.id] = player;
                playerCount++;
            }

            if (Interface.Instance != null) {
                Interface.Instance.UpdatePlayerList();
            }
        }
        catch (System.Exception ex) {
            Debug.LogError("플레이어 입장 파싱 오류: " + ex.Message);
        }
    }

    public void OnRoomFull(string message) {
        Debug.Log("방 만원: " + message);
    }

    public void OnPlayerListUpdated(string jsonData) {
        try {
            var parser = new SimpleJSONParser();
            var data = parser.Parse(jsonData);

            players.Clear();
            playerCount = 0;

            if (data is Dictionary<string, object> playerDict) {
                foreach (var kvp in playerDict) {
                    var playerData = kvp.Value as Dictionary<string, object>;
                    if (playerData != null) {
                        PlayerData player = new PlayerData {
                            id = playerData.ContainsKey("id") ? (string)playerData["id"] : "",
                            nickname = playerData.ContainsKey("nickname") ? (string)playerData["nickname"] : "Unknown",
                            color = playerData.ContainsKey("color") ? (string)playerData["color"] : "green",
                            turret = playerData.ContainsKey("turret") ? (string)playerData["turret"] : "A",
                            slot = playerData.ContainsKey("slot") ? System.Convert.ToInt32(playerData["slot"]) : 1
                        };

                        players[player.id] = player;
                        playerCount++;
                    }
                }
            }

            if (Interface.Instance != null) {
                Interface.Instance.UpdatePlayerList();
                // ✅ Ready Phase일 때만 ShowReadyPhase 호출
                if (GameManager.Instance != null && GameManager.Instance.currentPhase == "ready") {
                    Interface.Instance.ShowReadyPhase();
                }
            }
        }
        catch (System.Exception ex) {
            Debug.LogError($"플레이어 목록 파싱 오류: {ex.Message}");
        }
    }

    public void OnTurretAngleUpdated(string jsonData) {
        try {
            var parser = new SimpleJSONParser();
            var data = parser.Parse(jsonData) as Dictionary<string, object>;

            if (data == null) return;

            string turret = data.ContainsKey("turret") ? (string)data["turret"] : "";
            float angle = data.ContainsKey("angle") ? System.Convert.ToSingle(data["angle"]) : 0f;

            if (Turret.Instance != null) {
                Turret.Instance.UpdateTurretAngle(turret, angle);
            }
        }
        catch (System.Exception ex) {
            Debug.LogError("터렛 각도 업데이트 오류: " + ex.Message);
        }
    }

    public void OnUpgradeApplied(string jsonData) {
        try {
            var parser = new SimpleJSONParser();
            var data = parser.Parse(jsonData) as Dictionary<string, object>;

            if (data == null) return;

            string turret = data.ContainsKey("turret") ? (string)data["turret"] : "";
            string upgradeType = data.ContainsKey("upgradeType") ? (string)data["upgradeType"] : "";
            int level = data.ContainsKey("level") ? System.Convert.ToInt32(data["level"]) : 0;

            int upgradeIndex = upgradeType.Length > 0 ? upgradeType[0] - 'A' : -1;
            if (upgradeIndex >= 0 && upgradeIndex < 4) {
                upgradeStates[turret][upgradeIndex] = level;
            }

            Debug.Log($"업그레이드 적용: Turret {turret} - {upgradeType} Lv.{level}");
        }
        catch (System.Exception ex) {
            Debug.LogError("업그레이드 적용 오류: " + ex.Message);
        }
    }

    public void OnPhaseChanged(string phase) {
        try {
            Debug.Log($"Phase 변경 수신: {phase}");

            if (GameManager.Instance != null) {
            // GameManager.Instance.currentPhase = phase; // ✅ 제거: GameManager가 자체 관리
            }

            if (Interface.Instance != null) {
                if (phase == "ready") {
                    Interface.Instance.ShowReadyPhase();
                }
                else if (phase == "combat") {
                    Interface.Instance.ShowCombatPhase();
                }
            }
        }
        catch (System.Exception ex) {
            Debug.LogError("Phase 변경 오류: " + ex.Message);
        }
    }
#endif

    // 서버로 메시지 전송
    public void SendToServer(string eventName, string jsonData) {
#if UNITY_WEBGL && !UNITY_EDITOR
        EmitToServer(eventName, jsonData);
#else
        if (socket != null && isConnected) {
            socket.Emit(eventName, jsonData);
        }
#endif
    }

    // Phase 변경 브로드캐스트
    public void BroadcastPhaseChange(string phase) {
#if UNITY_WEBGL && !UNITY_EDITOR
        EmitToServer("phaseChange", $"\"{phase}\"");
#else
        if (socket != null && isConnected) {
            socket.Emit("phaseChange", phase);
        }
#endif
    }

    void OnApplicationQuit() {
#if UNITY_WEBGL && !UNITY_EDITOR
        DisconnectSocket();
#else
        if (socket != null) {
            socket.Disconnect();
        }
#endif
    }

    // 간단한 JSON 파서 (WebGL용)
    class SimpleJSONParser {
        private StringReader json;

        enum TOKEN {
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

        string ParseString() {
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            char c;

            json.Read();

            bool parsing = true;
            while (parsing) {
                if (json.Peek() == -1)
                    break;

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

                if (json.Peek() == -1)
                    break;
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

                    if (json.Peek() == -1)
                        break;
                }

                return word.ToString();
            }
        }

        TOKEN NextToken {
            get {
                EatWhitespace();

                if (json.Peek() == -1)
                    return TOKEN.NONE;

                char c = PeekChar;
                switch (c) {
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

                switch (word) {
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

    class StringReader : System.IDisposable {
        string str;
        int position;

        public StringReader(string s) {
            str = s;
            position = 0;
        }

        public int Peek() {
            if (position >= str.Length)
                return -1;

            return str[position];
        }

        public int Read() {
            if (position >= str.Length)
                return -1;

            return str[position++];
        }

        public void Dispose() {
            str = null;
        }
    }
}