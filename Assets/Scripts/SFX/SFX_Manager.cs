using UnityEngine;

public class SFX_Manager : MonoBehaviour
{
    public static SFX_Manager instance; // 싱글톤 인스턴스
    private Coroutine shootSoundCoroutine = null;

    public AudioClip Hit1, Hit2, Hit3;
    public AudioClip Exp1, Exp2;
    public AudioClip BoxHit, Item, Shoot, Warning;

    // Hit 사운드 재생 제한을 위한 변수
    private float hitSoundCooldown = 0.2f; // 쿨다운 시간
    private float lastHitSoundTime = -1f; // 마지막 재생 시간
    private int hitSoundCount = 0; // 현재 쿨다운 내 재생 횟수
    private int maxHitSounds = 3; // 최대 재생 횟수

    void Awake()
    {
        // 싱글톤 설정
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Turret_Shoot 배열 체크
        bool isAnyShooting = false;
        if (ServerDataManager.Turret_Shoot != null)
        {
            foreach (bool shooting in ServerDataManager.Turret_Shoot)
            {
                if (shooting)
                {
                    isAnyShooting = true;
                    break;
                }
            }
        }

        // Shoot 사운드 시작/중지
        if (isAnyShooting && shootSoundCoroutine == null)
        {
            shootSoundCoroutine = StartCoroutine(ShootSoundLoop());
        }
        else if (!isAnyShooting && shootSoundCoroutine != null)
        {
            StopCoroutine(shootSoundCoroutine);
            shootSoundCoroutine = null;
        }
    }

    // Shoot 사운드 반복 재생 코루틴
    private System.Collections.IEnumerator ShootSoundLoop()
    {
        while (true)
        {
            if (Shoot != null)
            {
                AudioSource.PlayClipAtPoint(Shoot, Camera.main.transform.position, 0.5f);
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    // 랜덤 Hit 사운드 재생 (0.2초 내 최대 3개 제한)
    public void PlayRandomHitSound()
    {
        float currentTime = Time.time;

        // 쿨다운이 지났으면 카운트 리셋
        if (currentTime - lastHitSoundTime > hitSoundCooldown)
        {
            hitSoundCount = 0;
        }

        // 최대 재생 횟수를 초과하면 재생하지 않음
        if (hitSoundCount >= maxHitSounds)
        {
            return;
        }

        // 사운드 재생
        AudioClip[] hitClips = { Hit1, Hit2, Hit3 };
        AudioClip selectedClip = hitClips[Random.Range(0, hitClips.Length)];

        if (selectedClip != null)
        {
            // 중첩 재생을 위해 PlayClipAtPoint 사용
            AudioSource.PlayClipAtPoint(selectedClip, Camera.main.transform.position);

            // 재생 정보 업데이트
            lastHitSoundTime = currentTime;
            hitSoundCount++;
        }
    }

    // Exp1 사운드 재생
    public void PlayExplosion1Sound()
    {
        if (Exp1 != null)
        {
            AudioSource.PlayClipAtPoint(Exp1, Camera.main.transform.position);
        }
    }

    // Exp2 사운드 재생
    public void PlayExplosion2Sound()
    {
        if (Exp2 != null)
        {
            AudioSource.PlayClipAtPoint(Exp2, Camera.main.transform.position);
        }
    }

    // BoxHit 사운드 재생
    public void PlayBoxHitSound()
    {
        if (BoxHit != null)
        {
            AudioSource.PlayClipAtPoint(BoxHit, Camera.main.transform.position);
        }
    }

    // Item 사운드 재생
    public void PlayItemSound()
    {
        if (Item != null)
        {
            AudioSource.PlayClipAtPoint(Item, Camera.main.transform.position);
        }
    }

    // Warning 사운드 재생
    public void PlayWarningSound()
    {
        if (Warning != null)
        {
            AudioSource.PlayClipAtPoint(Warning, Camera.main.transform.position, 0.3f); 
        }
    }
}