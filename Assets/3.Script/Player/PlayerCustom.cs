using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCustom : MonoBehaviour
{
    [Header("모자 설정")]
    [SerializeField] private SpriteRenderer hatSR; // 모자 스프라이트 렌더러
    [SerializeField] private Sprite[] hatSprites; // 사용 가능한 모자들
    [SerializeField] private int selectHat = 0; // 선택한 모자 인덱스

    [Header("플레이어 색상 설정")]
    [SerializeField] private SpriteRenderer playerSR; // 플레이어 스프라이트 렌더러
    [SerializeField] private Animator playerAni; // 플레이어 애니메이터

    [Header("색상별 애니메이터 컨트롤러")]
    [SerializeField] private RuntimeAnimatorController[] colorAni; // 색상별 애니메이터 컨트롤러
    [SerializeField] private Sprite[] colorSprites; // 색상별 기본 스프라이트
    [SerializeField] private int selectColor = 0; // 색상 선택

    private void Awake()
    {
        // 모자 적용
        ApplyHat();

        // 색상 적용
        ApplyColor();
    }

    // 모자 적용
    private void ApplyHat()
    {
        if (hatSR == null || hatSprites == null || hatSprites.Length == 0) return;

        if (selectHat >= 0 && selectHat < hatSprites.Length)
        {
            hatSR.sprite = hatSprites[selectHat];
        }
    }

    // 색상 적용
    private void ApplyColor()
    {
        // 스프라이트 변경
        if (playerSR != null && colorSprites != null && colorSprites.Length > 0)
        {
            if (selectColor >= 0 && selectColor < colorSprites.Length)
            {
                playerSR.sprite = colorSprites[selectColor];
            }
        }

        // 애니메이터 변경
        if (playerAni == null || colorAni == null || colorAni.Length == 0) return;
        if (selectColor >= 0 && selectColor < colorAni.Length)
        {
            playerAni.runtimeAnimatorController = colorAni[selectColor];
        }
    }

    // 모자 끄기
    public void HideHat()
    {
        hatSR.enabled = false;
    }

    // 모자 켜기
    public void ActiveHat()
    {
        hatSR.enabled = true;
    }

    // Inspector에서 미리보기
    private void OnValidate()
    {
        ApplyHat();
        ApplyColor();
    }
}