using UnityEngine;

public class GCPooledObject : MonoBehaviour {
    public GCObjectPoolingController PoolingController { get; set; }

    
    public void Release() {
        this.PoolingController.ReturnToPool(this);
    }
}