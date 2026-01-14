using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canon : MonoBehaviour
{
    [SerializeField] private GameObject canonBallPrefab;
    [SerializeField] private Transform shootPoint; // 대포알이 발사되는 위치
    [SerializeField] private float shootInterval = 2f; // 발사 간격
    [SerializeField] private bool shootLeft = true; // 발사 방향

    private GameObject currentCanonBall;
    private bool canShoot = true;

    private void Start()
    {
        // 대포알 미리 생성
        currentCanonBall = Instantiate(canonBallPrefab, shootPoint.position, Quaternion.identity);
        currentCanonBall.SetActive(false);

        StartCoroutine(ShootRoutine());
    }

    private IEnumerator ShootRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(shootInterval);

            // 대포알이 비활성화 상태일 때만 발사
            if (canShoot && !currentCanonBall.activeInHierarchy)
            {
                ShootCanonBall();
            }
        }
    }

    private void ShootCanonBall()
    {
        currentCanonBall.transform.position = shootPoint.position;

        CanonBall canonBall = currentCanonBall.GetComponent<CanonBall>();
        if (canonBall != null)
        {
            canonBall.SetDirection(shootLeft);
        }

        currentCanonBall.SetActive(true);
    }
}
