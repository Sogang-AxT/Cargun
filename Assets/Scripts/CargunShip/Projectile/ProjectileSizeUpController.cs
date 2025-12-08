using UnityEngine;

public class ProjectileSizeUpController : Projectile
{
    private Camera mainCamera;
    private float cameraHeight;
    private float cameraWidth;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraHeight = mainCamera.orthographicSize;
            cameraWidth = cameraHeight * mainCamera.aspect;
        }
    }

    private void LateUpdate()  // Update 대신 LateUpdate 사용!
    {
        // 카메라 경계 밖으로 나갔는지 체크
        if (IsOutsideCamera())
        {
            Deactivate();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("Enemy"))
        {
            Deactivate();
        }
    }

    protected override void Shoot()
    {
        this.transform.position += this.transform.up * (this.Velocity * Time.deltaTime);
    }

    private bool IsOutsideCamera()
    {
        if (mainCamera == null) return false;

        Vector3 cameraPos = mainCamera.transform.position;
        Vector3 bulletPos = transform.position;

        // 카메라 경계 체크
        if (bulletPos.x < cameraPos.x - cameraWidth ||
            bulletPos.x > cameraPos.x + cameraWidth ||
            bulletPos.y < cameraPos.y - cameraHeight ||
            bulletPos.y > cameraPos.y + cameraHeight)
        {
            return true;
        }

        return false;
    }
}