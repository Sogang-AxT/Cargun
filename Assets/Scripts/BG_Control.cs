using UnityEngine;
using System.Collections;

public class BG_Control : MonoBehaviour
{
    public GameObject[] Cloud;
    private float spawnInterval = 5f;

    void Start()
    {
        // 시작 시 3개의 구름을 고정 위치에 생성
        SpawnCloud(-7f);
        SpawnCloud(0f);
        SpawnCloud(7f);

        // 5초마다 새로운 구름 생성
        StartCoroutine(SpawnCloudRoutine());
    }

    void SpawnCloud(float xPosition)
    {
        // Cloud 배열에서 랜덤하게 하나 선택
        GameObject selectedCloud = Cloud[Random.Range(0, Cloud.Length)];

        // 랜덤 위치 설정
        Vector3 spawnPosition = new Vector3(
            xPosition,
            Random.Range(-5f, 5f),
            0f
        );

        // 구름 생성
        GameObject cloud = Instantiate(selectedCloud, spawnPosition, Quaternion.identity);

        // 이 오브젝트의 자식으로 설정
        cloud.transform.SetParent(this.transform);

        // 랜덤 크기 설정 (0.5 ~ 1.0 사이)
        float randomScale = Random.Range(0.5f, 1f);
        cloud.transform.localScale = new Vector3(randomScale, randomScale, 1f);

        // 랜덤 Z축 회전 (0 ~ 360도)
        float randomRotation = Random.Range(0f, 360f);
        cloud.transform.rotation = Quaternion.Euler(0f, 0f, randomRotation);

        // CloudMove 스크립트 추가
        cloud.AddComponent<CloudMove>();
    }

    IEnumerator SpawnCloudRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnCloud(20f); // 오른쪽에서 생성
        }
    }
}

// 구름 이동 및 삭제 관리 클래스
public class CloudMove : MonoBehaviour
{
    private float moveSpeed;

    void Start()
    {
        // 랜덤 이동 속도 설정 (-1 ~ -0.5 사이)
        moveSpeed = Random.Range(-1f, -0.5f);
    }

    void Update()
    {
        // 월드 좌표 기준으로 왼쪽으로 이동
        transform.position += new Vector3(moveSpeed * Time.deltaTime, 0f, 0f);

        // x축이 -20 이하로 떨어지면 삭제
        if (transform.position.x < -20f)
        {
            Destroy(gameObject);
        }
    }
}