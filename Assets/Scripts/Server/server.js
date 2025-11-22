const express = require('express');
const https = require('https');
const socketIo = require('socket.io');
const fs = require('fs');
const path = require('path');

const app = express();

// SSL 인증서 로드
const options = {
    key: fs.readFileSync('/etc/letsencrypt/live/mgtul.duckdns.org/privkey.pem'),
    cert: fs.readFileSync('/etc/letsencrypt/live/mgtul.duckdns.org/fullchain.pem')
};

// HTTPS 서버 생성
const server = https.createServer(options, app);
const io = socketIo(server, {
    cors: {
        origin: "*",
        methods: ["GET", "POST"]
    },
    // ✅ 매우 빠른 연결 해제 감지 설정 (백그라운드 전환 시 즉시 감지)
    pingInterval: 2000,      // 2초마다 ping (더 자주 체크)
    pingTimeout: 3000,       // 3초 안에 응답 없으면 disconnect
    connectTimeout: 5000,    // 연결 타임아웃 5초
    allowEIO3: true          // Engine.IO v3 호환성
});

// public 폴더의 정적 파일 제공
app.use('/cargunship-controller', express.static(path.join(__dirname, 'public')));

// ✅ 플레이어 슬롯 관리 (고정 슬롯 방식)
let playerSlots = {
    1: null,  // Player 1 (Green)
    2: null,  // Player 2 (Blue)
    3: null,  // Player 3 (Purple)
    4: null   // Player 4 (Orange)
};

let unityClient = null; // Unity 클라이언트 추적
let currentPhase = 'prepare'; // ✅ 현재 게임 Phase 추적 (prepare/battle)

// ✅ 게임 데이터 추적
let gameData = {
    totalPlayer: 0,
    turretRotation: [0, 0, 0, 0],  // 4개 터렛 각도
    turretShoot: [false, false, false, false], // 4개 터렛 발사 상태
    turretPlayer: [0, 0, 0, 0]     // 4개 터렛에 할당된 플레이어 번호
};

// ✅ Unity에게 플레이어 목록 전송하는 함수
function sendPlayerListToUnity() {
    if (unityClient) {
        // 슬롯 기반으로 플레이어 목록 생성
        let players = {};
        console.log('=== sendPlayerListToUnity 호출 ===');
        console.log('현재 playerSlots 상태:');
        for (let slot in playerSlots) {
            console.log(`  Slot ${slot}:`, playerSlots[slot]);
            if (playerSlots[slot] !== null) {
                players[playerSlots[slot].id] = playerSlots[slot];
                console.log(`    → Unity 전송: ${playerSlots[slot].nickname}, Slot ${playerSlots[slot].slot}, Color ${playerSlots[slot].color}`);
            }
        }
        
        io.to(unityClient).emit('playerList', players);
        console.log('✅ Unity에 플레이어 목록 전송:', Object.keys(players).length, '명');
        console.log('전송한 데이터:', JSON.stringify(players));
    }
}

// ✅ Unity에게 게임 데이터 전송하는 함수
function sendGameDataToUnity() {
    if (unityClient) {
        io.to(unityClient).emit('gameData', gameData);
        console.log('✅ Unity에 게임 데이터 전송:', gameData);
    }
}

// ✅ 현재 접속 중인 플레이어 수 계산
function getPlayerCount() {
    let count = 0;
    for (let slot in playerSlots) {
        if (playerSlots[slot] !== null) {
            count++;
        }
    }
    return count;
}

// ✅ 빈 슬롯 찾기 (1, 2, 3, 4 순서로)
function findEmptySlot() {
    for (let slot = 1; slot <= 4; slot++) {
        if (playerSlots[slot] === null) {
            return slot;
        }
    }
    return null; // 모든 슬롯이 차있음
}

// ✅ 플레이어 컬러 할당
function getPlayerColor(slot) {
    const colors = {
        1: 'green',
        2: 'blue',
        3: 'purple',
        4: 'orange'
    };
    return colors[slot] || 'green';
}

// ✅ 터렛 할당 (A, B, C, D 중 빈 터렛)
function assignTurret() {
    const turrets = ['A', 'B', 'C', 'D'];
    const usedTurrets = [];
    
    for (let slot in playerSlots) {
        if (playerSlots[slot] !== null) {
            usedTurrets.push(playerSlots[slot].turret);
        }
    }
    
    for (let turret of turrets) {
        if (!usedTurrets.includes(turret)) {
            return turret;
        }
    }
    return 'A'; // 기본값
}

// Socket.IO 연결 처리
io.on('connection', (socket) => {
    console.log('새 클라이언트 접속:', socket.id);

    // ✅ 모든 이벤트 감지 (디버깅용)
    socket.onAny((eventName, ...args) => {
        console.log(`📩 이벤트 수신: ${eventName}`, args.length > 0 ? args : '');
    });

    // Unity 클라이언트 등록
    socket.on('registerUnity', (data) => {
        console.log('Unity 클라이언트 등록:', socket.id, data);
        unityClient = socket.id;
        
        // ✅ Unity가 현재 Phase를 보내면 그대로 사용, 아니면 prepare로 초기화
        if (data && data.currentPhase) {
            currentPhase = data.currentPhase;
            console.log(`✅ Unity 현재 Phase 적용: ${currentPhase}`);
        } else {
            currentPhase = 'prepare';
            console.log('✅ Unity 재접속: Phase를 prepare로 초기화');
        }
        
        // ✅ 현재 플레이어 목록을 Unity에 전송
        sendPlayerListToUnity();
        
        // ✅ 모든 모바일 클라이언트에게 현재 Phase 전송
        io.emit('phaseChange', currentPhase);
        console.log(`📱 모든 클라이언트에게 ${currentPhase} Phase 브로드캐스트`);
    });

    // 플레이어 연결
    socket.on('join', (data) => {
        const playerCount = getPlayerCount();
        
        if (playerCount >= 4) {
            socket.emit('roomFull', '최대 인원(4명)이 찼습니다.');
            return;
        }

        // ✅ 빈 슬롯 찾기
        const emptySlot = findEmptySlot();
        if (emptySlot === null) {
            socket.emit('roomFull', '최대 인원(4명)이 찼습니다.');
            return;
        }

        const nickname = data.nickname.substring(0, 10); // 10자 제한
        const playerColor = getPlayerColor(emptySlot);
        const assignedTurret = assignTurret();
        
        // ✅ 슬롯에 플레이어 할당
        playerSlots[emptySlot] = {
            id: socket.id,
            nickname: nickname,
            color: playerColor,
            slot: emptySlot,
            turret: assignedTurret
        };
        
        // ✅ TotalPlayer 업데이트
        gameData.totalPlayer = getPlayerCount();
        
        // ✅ turretPlayer 업데이트 (터렛 인덱스는 A=0, B=1, C=2, D=3)
        const turretIndex = assignedTurret.charCodeAt(0) - 'A'.charCodeAt(0);
        gameData.turretPlayer[turretIndex] = emptySlot;
        
        console.log(`플레이어 입장: ${nickname} (Slot ${emptySlot} - ${playerColor}) - Turret ${assignedTurret}`);
        console.log(`TotalPlayer: ${gameData.totalPlayer}, turretPlayer: ${gameData.turretPlayer}`);
        
        // 입장한 플레이어에게 정보 전송
        socket.emit('joined', playerSlots[emptySlot]);
        
        // ✅ 현재 Phase를 새로 접속한 플레이어에게 전송
        socket.emit('phaseChange', currentPhase);
        console.log(`새 플레이어에게 현재 Phase 전송: ${currentPhase}`);
        
        // ✅ 모든 클라이언트(모바일)에게 브로드캐스트
        let players = {};
        for (let slot in playerSlots) {
            if (playerSlots[slot] !== null) {
                players[playerSlots[slot].id] = playerSlots[slot];
            }
        }
        io.emit('playerList', players);
        
        // ✅ Unity에게도 별도로 전송
        sendPlayerListToUnity();
        sendGameDataToUnity();
    });

    // ✅ 터렛 각도 업데이트
    socket.on('turretAngle', (data) => {
        // 플레이어의 슬롯 찾기
        let playerSlot = null;
        for (let slot in playerSlots) {
            if (playerSlots[slot] && playerSlots[slot].id === socket.id) {
                playerSlot = playerSlots[slot];
                break;
            }
        }
        
        if (playerSlot) {
            // ✅ gameData에 터렛 각도 저장
            const turretIndex = playerSlot.turret.charCodeAt(0) - 'A'.charCodeAt(0);
            gameData.turretRotation[turretIndex] = data.angle;
            
            // Unity와 다른 플레이어들에게 전송
            io.emit('updateTurret', {
                turret: playerSlot.turret,
                angle: data.angle
            });
            
            // ✅ Unity에 게임 데이터 전송
            sendGameDataToUnity();
        }
    });

    // ✅ 터렛 전환
    socket.on('switchTurret', () => {
        console.log('🔄 switchTurret 이벤트 수신:', socket.id);
        
        let playerSlot = null;
        let currentSlotNum = null;
        
        for (let slot in playerSlots) {
            if (playerSlots[slot] && playerSlots[slot].id === socket.id) {
                playerSlot = playerSlots[slot];
                currentSlotNum = slot;
                break;
            }
        }
        
        if (playerSlot) {
            const oldTurret = playerSlot.turret;
            const oldTurretIndex = oldTurret.charCodeAt(0) - 'A'.charCodeAt(0);
            
            playerSlot.turret = getNextAvailableTurret(playerSlot.turret, socket.id);
            const newTurretIndex = playerSlot.turret.charCodeAt(0) - 'A'.charCodeAt(0);
            
            // ✅ turretPlayer 업데이트
            gameData.turretPlayer[oldTurretIndex] = 0; // 이전 터렛 비우기
            gameData.turretPlayer[newTurretIndex] = parseInt(currentSlotNum); // 새 터렛 할당
            
            console.log(`✅ 터렛 전환: ${oldTurret} → ${playerSlot.turret}`);
            console.log(`turretPlayer 업데이트:`, gameData.turretPlayer);
            
            // 해당 플레이어에게만 터렛 전환 확인 전송
            socket.emit('turretSwitched', playerSlot.turret);
            
            // ✅ Unity에게 플레이어 목록과 게임 데이터 전송
            sendPlayerListToUnity();
            sendGameDataToUnity();
            
            console.log('✅ 터렛 전환 완료');
        } else {
            console.warn('❌ 플레이어 슬롯을 찾을 수 없음:', socket.id);
        }
    });

    // ✅ 터렛 발사 상태 업데이트
    socket.on('shootStatus', (data) => {
        // 플레이어의 슬롯 찾기
        let playerSlot = null;
        for (let slot in playerSlots) {
            if (playerSlots[slot] && playerSlots[slot].id === socket.id) {
                playerSlot = playerSlots[slot];
                break;
            }
        }
        
        if (playerSlot) {
            const turretIndex = playerSlot.turret.charCodeAt(0) - 'A'.charCodeAt(0);
            gameData.turretShoot[turretIndex] = data.shooting;
            
            // Unity에 게임 데이터 전송
            sendGameDataToUnity();
        }
    });

    // 업그레이드 요청 (기존 코드 - 추후 제거 가능)
    socket.on('upgrade', (data) => {
        console.log(`업그레이드: Turret ${data.turret} - ${data.upgradeType} Lv.${data.level}`);
        
        // 모든 클라이언트에게 브로드캐스트
        io.emit('upgradeApplied', {
            turret: data.turret,
            upgradeType: data.upgradeType,
            level: data.level
        });
    });

    // Phase 변경 (Unity → 모바일)
    socket.on('phaseChange', (phase) => {
        console.log('Phase 변경:', phase);
        currentPhase = phase; // ✅ 현재 Phase 저장
        io.emit('phaseChange', phase);
    });

    // ✅ 클라이언트 명시적 연결 해제 요청 (백그라운드 전환 시)
    socket.on('forceDisconnect', () => {
        console.log(`강제 연결 해제 요청: ${socket.id}`);
        socket.disconnect(true); // 강제 disconnect
    });

    // 연결 해제
    socket.on('disconnect', () => {
        // ✅ 플레이어 슬롯에서 찾기
        for (let slot in playerSlots) {
            if (playerSlots[slot] && playerSlots[slot].id === socket.id) {
                const disconnectedTurret = playerSlots[slot].turret;
                const turretIndex = disconnectedTurret.charCodeAt(0) - 'A'.charCodeAt(0);
                
                console.log(`플레이어 퇴장: ${playerSlots[slot].nickname} (Slot ${slot})`);
                
                // ✅ 슬롯을 비우지만 슬롯 번호는 유지
                playerSlots[slot] = null;
                
                // ✅ TotalPlayer 업데이트
                gameData.totalPlayer = getPlayerCount();
                
                // ✅ turretPlayer 업데이트 (해당 터렛을 0으로 설정)
                gameData.turretPlayer[turretIndex] = 0;
                
                console.log(`TotalPlayer: ${gameData.totalPlayer}, turretPlayer: ${gameData.turretPlayer}`);
                
                // 모든 클라이언트에게 업데이트된 목록 전송
                let players = {};
                for (let s in playerSlots) {
                    if (playerSlots[s] !== null) {
                        players[playerSlots[s].id] = playerSlots[s];
                    }
                }
                io.emit('playerList', players);
                sendPlayerListToUnity();
                sendGameDataToUnity();
                break;
            }
        }
        
        if (socket.id === unityClient) {
            console.log('Unity 클라이언트 연결 해제');
            unityClient = null;
            
            // ✅ Unity 연결 해제 시 Phase를 prepare로 리셋
            currentPhase = 'prepare';
            console.log('✅ Unity 연결 해제: Phase를 prepare로 리셋');
            
            // ✅ 모든 모바일 클라이언트에게 prepare Phase 전송
            io.emit('phaseChange', 'prepare');
        }
    });
});

// 다음 사용 가능한 터렛 찾기
function getNextAvailableTurret(currentTurret, currentSocketId) {
    const turrets = ['A', 'B', 'C', 'D'];
    const usedTurrets = [];
    
    for (let slot in playerSlots) {
        if (playerSlots[slot] !== null && playerSlots[slot].id !== currentSocketId) {
            usedTurrets.push(playerSlots[slot].turret);
        }
    }
    
    let currentIndex = turrets.indexOf(currentTurret);
    
    for (let i = 1; i <= 4; i++) {
        let nextIndex = (currentIndex + i) % 4;
        if (!usedTurrets.includes(turrets[nextIndex])) {
            return turrets[nextIndex];
        }
    }
    
    return currentTurret;
}

// 서버 시작
const PORT = 443;
server.listen(PORT, () => {
    console.log(`Cargun Ship 서버 실행 중: https://mgtul.duckdns.org:${PORT}`);
    console.log(`모바일 컨트롤러: https://mgtul.duckdns.org/cargunship-controller`);
});