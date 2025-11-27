using System;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectileController : MonoBehaviour {
    public IObjectPool<ProjectileController> ProjectileSpawnPool { get; set; }
    public float Dmg { get; private set; } = 5;

    private float _projectileVelocity;
    
    
    private void Init() {
        this._projectileVelocity = 10f;
    }

    private void Awake() {
        Init();
    }
    
    private void Update() {
        // 매 프레임마다 앞 방향으로 이동
        this.transform.position += this.transform.up * (this._projectileVelocity * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Wall")) {
            Deactivate();
        }
    }

    private void Deactivate() {
        // 오브젝트 풀에 반환
        ProjectileSpawnPool.Release(this);
    }
}
