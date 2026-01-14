using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canon : MonoBehaviour
{
    [SerializeField] private GameObject canonBallPrefab;
    [SerializeField] private Transform shootPoint; // 대포알이 발사되는 위치
    [SerializeField] private bool shootLeft = true; // 발사 방향

    private GameObject currentCanonBall;

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
            while (true)
            {
                // 대포알이 비활성화될 때까지 대기
                yield return new WaitUntil(() => !currentCanonBall.activeInHierarchy);

                // 대포알이 비활성화되면 1초 대기
                yield return new WaitForSeconds(1f);

                // 발사
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
