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
    public State state { get; private set; } // ���� ���� ����

    //�߻�� ��ġ
    public Transform fireTransform;

    public ParticleSystem muzzleFlashEffect; //�ѱ����� ����Ʈ
    public ParticleSystem shellEjectEffect; //ź�� ����Ʈ

    private LineRenderer bulletLineRenderer; // ź�� ������ �׸��� ���� ������;
    private AudioSource gunAudioPlayer; //�ѼҸ� ����� �׸�
    private AudioClip shotClip; // ��� �Ҹ�
    private AudioClip reloadClip; // ���ε� �Ҹ�

    public float damage = 25; // ���ݷ�
    private float fireDistance = 50f; // �����Ÿ�

    public int ammoRemain = 100; //��ü ���� ź��.
    public int magCapacity = 25; //źâ �뷮
    public int magAmmo; //���� źâ�� ���� ź��


    public float timeBetFire = 0.12f; //ź�� �߻� ����
    public float reloadTime = 1.8f; // �������ð�
    private float lastFireTime; //���� ���������� �߻��� ����

    private void Awake()
    {
        gunAudioPlayer = GetComponent<AudioSource>();
        bulletLineRenderer = GetComponent<LineRenderer>();

        //����� ���� 2���� ����
        bulletLineRenderer.positionCount = 2;
        bulletLineRenderer.enabled = false;
    }

    //�� ���� �ʱ�ȭ , ������Ʈ�� Ȱ��ȭ �ɶ����� �Ź� �����.
    private void OnEnable()
    {
        //���� źâ�� ����ä���. 
        magAmmo = magCapacity;
        state = State.Ready;
        lastFireTime = 0;
    }

    private void Fire()
    { // ����ð��� ���� �ֱٿ� �߻��� ���� + �߻� ���� �������� �˻�
        if (state == State.Ready && Time.time >= lastFireTime + timeBetFire)
        {
            lastFireTime = Time.time;
            Shot();
        }
    }

    //���� �߻� ó��
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

    //�߻� ����Ʈ�� �Ҹ��� ����ϰ� ź�� ������ �׸�
    private IEnumerator ShotEffectCo(Vector3 hitPosition)
    {
        muzzleFlashEffect.Play();
        shellEjectEffect.Play();
        gunAudioPlayer.PlayOneShot(shotClip);

        //���η������� �������� �ѱ��� ��ġ.
        bulletLineRenderer.SetPosition(0, fireTransform.position);
        //�� ���� �Է����� ���� �浹 ��ġ
        bulletLineRenderer.SetPosition(1, hitPosition);


        //���η����� Ȱ��ȭ �ϰ� ź�� ������ �׸� ->0.03�� ���
        bulletLineRenderer.enabled = true;
        yield return new WaitForSeconds(0.03f);
        bulletLineRenderer.enabled = false;
    }

    //������ �õ�
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
        //���� ���¸� ������ �� ���·� ��ȯ
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

        //�߻��غ�
        state = State.Ready;
    }
}