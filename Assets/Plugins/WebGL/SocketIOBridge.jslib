mergeInto(LibraryManager.library, {
    // Socket.IO 초기화
    InitSocketIO: function(urlPtr) {
        var url = UTF8ToString(urlPtr);
        console.log("🔌 Socket.IO 초기화: " + url);
        
        // Socket.IO 라이브러리 동적 로드
        if (typeof io === 'undefined') {
            var script = document.createElement('script');
            script.src = 'https://cdn.socket.io/4.5.4/socket.io.min.js';
            script.onload = function() {
                console.log("✅ Socket.IO 라이브러리 로드 완료");
                window.unitySocket = io(url, {
                    transports: ['websocket', 'polling']
                });
                setupSocketEvents();
            };
            document.head.appendChild(script);
        } else {
            window.unitySocket = io(url, {
                transports: ['websocket', 'polling']
            });
            setupSocketEvents();
        }
        
        function setupSocketEvents() {
            var socket = window.unitySocket;
            
            // 연결 성공
            socket.on('connect', function() {
                console.log("✅ Socket.IO 연결 성공!");
                
                // 즉시 Unity 클라이언트 등록
                console.log("📤 Unity 클라이언트 등록 전송");
                socket.emit('registerUnity', { type: 'unity' });
                
                // Unity에 연결 성공 알림
                SendMessage('ServerManager', 'OnSocketConnected', '');
            });
            
            // 연결 끊김
            socket.on('disconnect', function() {
                console.log("❌ Socket.IO 연결 끊김");
                SendMessage('ServerManager', 'OnSocketDisconnected', '');
            });
            
            // Unity 클라이언트 등록 확인
            socket.on('registerUnity', function() {
                console.log("✅ Unity 클라이언트 등록 완료");
            });
            
            // 플레이어 입장
            socket.on('joined', function(data) {
                console.log("📥 플레이어 입장:", data);
                SendMessage('ServerManager', 'OnPlayerJoined', JSON.stringify(data));
            });
            
            // 방 꽉참
            socket.on('roomFull', function(message) {
                console.log("⚠️ 방 만원:", message);
                SendMessage('ServerManager', 'OnRoomFull', message);
            });
            
            // 플레이어 목록 업데이트
            socket.on('playerList', function(data) {
                console.log("📥 플레이어 목록:", data);
                SendMessage('ServerManager', 'OnPlayerListUpdated', JSON.stringify(data));
            });
            
            // 터렛 각도 업데이트
            socket.on('updateTurret', function(data) {
                console.log("📥 터렛 각도:", data);
                SendMessage('ServerManager', 'OnTurretAngleUpdated', JSON.stringify(data));
            });
            
            // 터렛 전환
            socket.on('turretSwitched', function(turret) {
                console.log("🔄 터렛 전환:", turret);
                SendMessage('ServerManager', 'OnTurretSwitched', turret);
            });
            
            // 업그레이드 적용
            socket.on('upgradeApplied', function(data) {
                console.log("📥 업그레이드:", data);
                SendMessage('ServerManager', 'OnUpgradeApplied', JSON.stringify(data));
            });
            
            // Phase 변경
            socket.on('phaseChange', function(phase) {
                console.log("📥 Phase 변경:", phase);
                SendMessage('ServerManager', 'OnPhaseChanged', phase);
            });
            
            // ✅ 게임 데이터 수신
            socket.on('gameData', function(data) {
                console.log("📥 게임 데이터:", data);
                SendMessage('ServerManager', 'OnGameDataReceived', JSON.stringify(data));
            });
            
            
            console.log("✅ Socket.IO 이벤트 리스너 등록 완료");
        }
    },
    
    // Unity 클라이언트 등록
    RegisterUnityClient: function() {
        if (window.unitySocket && window.unitySocket.connected) {
            console.log("📤 Unity 클라이언트 등록 전송");
            window.unitySocket.emit('registerUnity', { type: 'unity' });
            return true;
        }
        return false;
    },
    
    // 서버로 데이터 전송
    EmitToServer: function(eventNamePtr, jsonDataPtr) {
        var eventName = UTF8ToString(eventNamePtr);
        var jsonData = UTF8ToString(jsonDataPtr);
        
        if (window.unitySocket && window.unitySocket.connected) {
            try {
                if (jsonData === "" || jsonData === "{}") {
                    console.log("📤 전송:", eventName);
                    window.unitySocket.emit(eventName);
                } else {
                    var data = JSON.parse(jsonData);
                    console.log("📤 전송:", eventName, data);
                    window.unitySocket.emit(eventName, data);
                }
                return true;
            } catch (e) {
                console.error("❌ 전송 오류:", e);
                return false;
            }
        } else {
            console.warn("⚠️ Socket 미연결 상태");
            return false;
        }
    },
    
    // 연결 상태 확인
    IsSocketConnected: function() {
        return window.unitySocket && window.unitySocket.connected;
    },
    
    // 연결 종료
    DisconnectSocket: function() {
        if (window.unitySocket) {
            console.log("🔌 Socket 연결 종료");
            window.unitySocket.disconnect();
            window.unitySocket = null;
        }
    }
});
