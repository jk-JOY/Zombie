using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum State
{
    Ready,
    Empty,
    Reloading
}

public class Gun : MonoBehaviour
{
    public State state { get; private set; } // 현재 총의 상태

    //발사될 위치
    public Transform fireTransform;

    public ParticleSystem muzzleFlashEffect; //총구연기 이펙트
    public ParticleSystem shellEjectEffect; //탄피 이펙트

    private LineRenderer bulletLineRenderer; // 탄알 궤적을 그리기 위한 렌더러;
    private AudioSource gunAudioPlayer; //총소리 재생기 그릇
    private AudioClip shotClip; // 쏘는 소리
    private AudioClip reloadClip; // 리로드 소리

    public float damage = 25; // 공격력
    private float fireDistance = 50f; // 사정거리

    public int ammoRemain = 100; //전체 남은 탄알.
    public int magCapacity = 25; //탄창 용량
    public int magAmmo; //현재 탄창에 남은 탄알


    public float timeBetFire = 0.12f; //탄알 발사 간격
    public float reloadTime = 1.8f; // 재장전시간
    private float lastFireTime; //총을 마지막으로 발사한 시점

    private void Awake()
    {
        gunAudioPlayer = GetComponent<AudioSource>();
        bulletLineRenderer = GetComponent<LineRenderer>();

        //사용할 점을 2개로 변경
        bulletLineRenderer.positionCount = 2;
        bulletLineRenderer.enabled = false;
    }

    //총 상태 초기화 , 컴포넌트가 활성화 될때마다 매번 실행됨.
    private void OnEnable()
    {
        //현재 탄창을 가득채운다. 
        magAmmo = magCapacity;
        state = State.Ready;
        lastFireTime = 0;
    }

    private void Fire()
    { // 현재시간이 총을 최근에 발사한 시점 + 발사 간격 이후인지 검사
        if (state == State.Ready && Time.time >= lastFireTime + timeBetFire)
        {
            lastFireTime = Time.time;
            Shot();
        }
    }

    //실제 발사 처리
    private void Shot()
    {
        RaycastHit hit;
        Vector3 hitPosition = Vector3.zero;

        if (Physics.Raycast(fireTransform.position, fireTransform.forward, out hit, fireDistance))
        {
            IDamageble target = hit.collider.GetComponent<IDamageble>();
            if (target != null)
            {
                target.OnDamage(damage, hit.point, hit.normal);
            }
            hitPosition = hit.point;
        }
        else
        {
            hitPosition = fireTransform.position + fireTransform.forward * fireDistance;
        }

        StartCoroutine(ShotEffectCo(hitPosition));

        magAmmo--;
        if (magAmmo <= 0)
        {
            state = State.Empty;
        }
    }

    //발사 이펙트와 소리를 재생하고 탄알 궤적을 그림
    private IEnumerator ShotEffectCo(Vector3 hitPosition)
    {
        muzzleFlashEffect.Play();
        shellEjectEffect.Play();
        gunAudioPlayer.PlayOneShot(shotClip);

        //라인렌더러의 시작점은 총구의 위치.
        bulletLineRenderer.SetPosition(0, fireTransform.position);
        //끝 점은 입력으로 들어온 충돌 위치
        bulletLineRenderer.SetPosition(1, hitPosition);


        //라인렌더러 활성화 하고 탄알 궤적을 그림 ->0.03초 대기
        bulletLineRenderer.enabled = true;
        yield return new WaitForSeconds(0.03f);
        bulletLineRenderer.enabled = false;
    }

    //재장전 시도
    public bool Reload()
    {
        if (state == State.Reloading || ammoRemain <= 0 || magAmmo <= magCapacity)
        {
            return false;
        }

        StartCoroutine(ReloadRoutine());
        return true;
    }

    private IEnumerator ReloadRoutine()
    {
        //현재 상태를 재장전 중 상태로 전환
        state = State.Reloading;

        gunAudioPlayer.PlayOneShot(reloadClip);

        yield return new WaitForSeconds(reloadTime);

        int ammoToFill = magCapacity - magAmmo;

        if (ammoRemain < ammoToFill)
        {
            ammoToFill = ammoRemain;
        }

        magAmmo += ammoToFill;
        ammoRemain -= ammoToFill;

        //발사준비
        state = State.Ready;
    }
}