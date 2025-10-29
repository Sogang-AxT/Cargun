using UnityEngine;
using System.Collections;

public class Base : MonoBehaviour
{
    public static Base Instance;

    [Header("Base Object")]
    public GameObject BaseObject;

    [Header("Movement Settings")]
    public float moveOutDuration = 5f; // 5�ʿ� ���� �̵�
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

    // ���� ��� (0,0,0 �� 0,-30,0)
    public void MoveOut()
    {
        if (BaseObject != null)
        {
            StartCoroutine(MoveToPosition(outPosition, moveOutDuration));
        }
    }

    // ���� ���� (0,-30,0 �� 0,0,0) - �ڷ�ƾ���� ��ȯ
    public IEnumerator MoveIn()
    {
        if (BaseObject != null)
        {
            // ���� (0,30,0)�� �����̵�
            BaseObject.transform.position = new Vector3(0, 30, 0);

            // 5�ʿ� ���� (0,0,0)���� ����
            yield return StartCoroutine(MoveToPosition(startPosition, moveInDuration, true));
        }
    }

    // ��� ����ġ
    public void ResetPosition()
    {
        if (BaseObject != null)
        {
            BaseObject.transform.position = startPosition;
        }
    }

    // Ư�� ��ġ�� �̵� (�ڷ�ƾ)
    IEnumerator MoveToPosition(Vector3 targetPosition, float duration, bool decelerate = false)
    {
        Vector3 startPos = BaseObject.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // ���� ȿ�� (decelerate true�� ��)
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