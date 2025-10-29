using UnityEngine;

public class CargoHitBox : MonoBehaviour
{
    // Enemy�� �浹 ��
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Cargo ����
            if (Cargo.Instance != null)
            {
                Cargo.Instance.DecreaseCargo();
            }

            // Enemy ����
            Destroy(other.gameObject);
        }
    }
}