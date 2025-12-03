public static class GCEnumManager {
    public enum GAMEPHASE {
        PREPARE,
        BATTLE,
        ENDING
    }

    public enum CONNECT_TYPE {
        EDITOR,
        WEBGL
    }

    public enum GAMETAG {
        BULLET,
        SHIP,
    }

    public enum ENEMY_TYPE {
        DEFAULT,
        SHIELD,
        GROUP,
        ZIGZAG,
        WEAK,
        PUPA,
        BOMB,
        SPLIT,
    }
    
    public enum TURRET_TYPE {
        A,
        B,
        C,
        D
    }

    public enum PROJECTILE_TYPE {
        DEFAULT,
        LASER,
        ROCKET,
        SHOTGUN
    }

    public enum ITEM_TYPE {
        // LASER,
        // ROCKET,
        // SHOTGUN,
        HEALTH
    }
}