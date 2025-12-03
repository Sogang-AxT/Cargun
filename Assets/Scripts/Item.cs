using UnityEngine;

public abstract class Item : MonoBehaviour {
    protected void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Bullet")) {
            ItemActivate();
            gameObject.SetActive(false);    // TODO: 풀링
        }
    }
    
    protected abstract void ItemActivate();
}