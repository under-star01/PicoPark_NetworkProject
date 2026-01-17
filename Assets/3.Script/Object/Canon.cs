using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canon : NetworkBehaviour
{
    [SerializeField] private GameObject canonBallPrefab;
    [SerializeField] private Transform shootPoint; // 대포알이 발사되는 위치
    [SerializeField] private bool shootLeft = true; // 발사 방향

    private CanonBall currentCanonBall;

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(ShootRoutine());
    }

    [Server]
    private IEnumerator ShootRoutine()
    {
        while (true)
        {
            // 대포알이 없을 때만 대기 후 생성
            if (currentCanonBall == null)
            {
                yield return new WaitForSeconds(1f);
                SpawnCanonBall();
            }

            yield return null;
        }
    }

    [Server]
    private void SpawnCanonBall()
    {
        GameObject ballObj = Instantiate(canonBallPrefab, shootPoint.position, Quaternion.identity);
        CanonBall ball = ballObj.GetComponent<CanonBall>();

        currentCanonBall = ball;

        ball.Init(shootLeft, this);
        NetworkServer.Spawn(ballObj);
    }

    [Server]
    public void OnCanonBallDestroyed()
    {
        currentCanonBall = null;
    }
}
