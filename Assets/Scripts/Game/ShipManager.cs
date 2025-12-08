using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class ShipManager : MonoBehaviour
{
    public int ship_HP;
    private int maxHP;
    public GameObject ship;
    public GameObject explosion;
    public GameObject fire1, fire2, fire3;

    private List<GameObject> fires;
    private List<GameObject> availableFires;
    private int currentFireCount = 0;
    public bool isDestroyed = false;

    void Start()
    {
        maxHP = ship_HP;

        // fire 리스트 초기화
        fires = new List<GameObject> { fire1, fire2, fire3 };
        availableFires = new List<GameObject>(fires);

        // 모든 fire와 자식들 비활성화
        foreach (var fire in fires)
        {
            if (fire != null)
            {
                ParticleSystem[] particleSystems = fire.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in particleSystems)
                {
                    var emission = ps.emission;
                    emission.enabled = false;
                }
            }
        }

        // explosion과 자식들 비활성화
        if (explosion != null)
        {
            explosion.SetActive(false);
        }
    }

    void Update()
    {
        if (!isDestroyed)
        {
            CheckHPAndActivateFires();
        }
    }

    void CheckHPAndActivateFires()
    {
        float hpRatio = (float)ship_HP / maxHP;

        // 체력 0일 때
        if (ship_HP <= 0)
        {
            DestroyShip();
            return;
        }

        // 필요한 fire 개수 계산
        int requiredFires = 0;
        if (hpRatio <= 0.25f)
        {
            requiredFires = 3;
        }
        else if (hpRatio <= 0.5f)
        {
            requiredFires = 2;
        }
        else if (hpRatio <= 0.75f)
        {
            requiredFires = 1;
        }

        // fire 활성화/비활성화
        while (currentFireCount < requiredFires && availableFires.Count > 0)
        {
            ActivateRandomFire();
        }

        while (currentFireCount > requiredFires)
        {
            DeactivateLastFire();
        }
    }

    void ActivateRandomFire()
    {
        if (availableFires.Count == 0) return;

        int randomIndex = Random.Range(0, availableFires.Count);
        GameObject selectedFire = availableFires[randomIndex];

        if (selectedFire != null)
        {
            // 부모와 모든 자식들의 ParticleSystem 활성화
            ParticleSystem[] particleSystems = selectedFire.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                var emission = ps.emission;
                emission.enabled = true;
                if (!ps.isPlaying)
                {
                    ps.Play();
                }
            }
        }

        availableFires.RemoveAt(randomIndex);
        currentFireCount++;
    }

    void DeactivateLastFire()
    {
        foreach (var fire in fires)
        {
            if (fire != null)
            {
                var ps = fire.GetComponent<ParticleSystem>();
                if (ps != null && ps.emission.enabled)
                {
                    // 부모와 모든 자식들의 ParticleSystem 비활성화
                    ParticleSystem[] particleSystems = fire.GetComponentsInChildren<ParticleSystem>();
                    foreach (var particleSystem in particleSystems)
                    {
                        var emission = particleSystem.emission;
                        emission.enabled = false;
                    }

                    if (!availableFires.Contains(fire))
                    {
                        availableFires.Add(fire);
                    }

                    currentFireCount--;
                    return;
                }
            }
        }
    }

    void DestroyShip()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // 모든 fire와 자식들 끄기
        foreach (var fire in fires)
        {
            if (fire != null)
            {
                ParticleSystem[] particleSystems = fire.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in particleSystems)
                {
                    var emission = ps.emission;
                    emission.enabled = false;
                    ps.Stop();
                }
            }
        }

        // explosion과 자식들 활성화
        if (explosion != null)
        {
            explosion.SetActive(true);
            ParticleSystem[] particleSystems = explosion.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                ps.Play();
            }
        }

        // ship 비활성화
        if (ship != null)
        {
            ship.SetActive(false);
        }

        // 8초 후 씬 재시작
        StartCoroutine(RestartSceneAfterDelay(8f));
    }

    IEnumerator RestartSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // HP 원상복구 함수
    public void RestoreHP()
    {
        ship_HP = maxHP;
        isDestroyed = false;

        // 모든 fire와 자식들 끄기
        foreach (var fire in fires)
        {
            if (fire != null)
            {
                ParticleSystem[] particleSystems = fire.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in particleSystems)
                {
                    var emission = ps.emission;
                    emission.enabled = false;
                }
            }
        }

        // availableFires 리스트 초기화
        availableFires = new List<GameObject>(fires);
        currentFireCount = 0;
    }
}