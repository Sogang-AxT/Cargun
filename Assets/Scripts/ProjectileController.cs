using UnityEngine;
using UnityEngine.Pool;

public class ProjectileController : MonoBehaviour {
    public IObjectPool<ProjectileController> ProjectileSpawnPool { get; set; }
    public float Dmg { get; private set; } = 5;


    public void Deactivate() {
        var rBody = this.gameObject.GetComponent<Rigidbody2D>();
        rBody.linearVelocity = Vector2.zero;
        rBody.angularVelocity = 0;
        
        
        this.ProjectileSpawnPool.Release(this);
    }
}