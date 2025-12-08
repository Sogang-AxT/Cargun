using UnityEngine;

public class WarningScript : MonoBehaviour
{
    private float duration = 5f; // 총 지속 시간
    private int repeatCount = 5; // 반복 횟수
    private float cycleTime; // 한 사이클당 시간
    private float timer = 0f;

    private Vector3 minScale = new Vector3(0.1f, 0.1f, 0.1f);
    private Vector3 maxScale = new Vector3(0.15f, 0.15f, 0.15f);

    private float minAlpha = 0.5f;
    private float maxAlpha = 0.8f;

    private SpriteRenderer spriteRenderer;
    private int currentCycle = 0;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cycleTime = duration / repeatCount; // 5초 / 5번 = 1초당 1사이클
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (currentCycle < repeatCount)
        {
            // 현재 사이클 내에서의 진행도 (0~1)
            float cycleProgress = (timer % cycleTime) / cycleTime;

            // PingPong으로 0->1->0 반복 (줄었다 커졌다)
            float pingPong = Mathf.PingPong(cycleProgress * 2, 1);

            // Scale 조절
            transform.localScale = Vector3.Lerp(minScale, maxScale, pingPong);

            // Alpha 조절
            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(minAlpha, maxAlpha, pingPong);
            spriteRenderer.color = color;

            // 사이클 카운트
            currentCycle = Mathf.FloorToInt(timer / cycleTime);
        }
        else
        {
            // 5번 반복 완료 후 오브젝트 삭제
            Destroy(gameObject);
        }
    }
}