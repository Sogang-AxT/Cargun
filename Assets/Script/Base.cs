using UnityEngine;
using System.Collections;

public class Base : MonoBehaviour
{
    public static Base Instance;

    [Header("Base Object")]
    public GameObject BaseObject;

    [Header("Movement Settings")]
    public float moveOutDuration = 5f; // 5초에 걸쳐 이동
    public float moveInDuration = 5f;

    private Vector3 startPosition = new Vector3(0, 0, 0);
    private Vector3 outPosition = new Vector3(0, -30, 0);

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (BaseObject != null)
        {
            BaseObject.transform.position = startPosition;
        }
    }

    // 기지 출발 (0,0,0 → 0,-30,0)
    public void MoveOut()
    {
        if (BaseObject != null)
        {
            StartCoroutine(MoveToPosition(outPosition, moveOutDuration));
        }
    }

    // 기지 복귀 (0,-30,0 → 0,0,0) - 코루틴으로 반환
    public IEnumerator MoveIn()
    {
        if (BaseObject != null)
        {
            // 먼저 (0,30,0)로 순간이동
            BaseObject.transform.position = new Vector3(0, 30, 0);

            // 5초에 걸쳐 (0,0,0)으로 감속
            yield return StartCoroutine(MoveToPosition(startPosition, moveInDuration, true));
        }
    }

    // 즉시 원위치
    public void ResetPosition()
    {
        if (BaseObject != null)
        {
            BaseObject.transform.position = startPosition;
        }
    }

    // 특정 위치로 이동 (코루틴)
    IEnumerator MoveToPosition(Vector3 targetPosition, float duration, bool decelerate = false)
    {
        Vector3 startPos = BaseObject.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // 감속 효과 (decelerate true일 때)
            if (decelerate)
            {
                t = 1 - Mathf.Pow(1 - t, 3); // Ease-out cubic
            }

            BaseObject.transform.position = Vector3.Lerp(startPos, targetPosition, t);
            yield return null;
        }

        BaseObject.transform.position = targetPosition;
    }
}