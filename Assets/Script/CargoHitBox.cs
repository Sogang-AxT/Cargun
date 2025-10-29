using UnityEngine;

public class CargoHitBox : MonoBehaviour
{
    // Enemy客 面倒 矫
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Cargo 皑家
            if (Cargo.Instance != null)
            {
                Cargo.Instance.DecreaseCargo();
            }

            // Enemy 力芭
            Destroy(other.gameObject);
        }
    }
}