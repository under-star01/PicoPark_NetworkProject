using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCustom : MonoBehaviour
{
    [Header("모자 설정")]
    [SerializeField] private SpriteRenderer hatSR; // 모자 스프라이트 렌더러
    [SerializeField] private Sprite[] hatSprites; // 사용 가능한 모자들

    [Header("플레이어 색상 설정")]
    [SerializeField] private SpriteRenderer playerSR; // 플레이어 스프라이트 렌더러
    [SerializeField] private Animator playerAni; // 플레이어 애니메이터

    [Header("색상별 애니메이터 컨트롤러")]
    [SerializeField] private RuntimeAnimatorController[] colorAni; // 색상별 애니메이터 컨트롤러
    [SerializeField] private Sprite[] colorSprites; // 색상별 기본 스프라이트

    // 모자 적용
    public void ApplyHat(int hatIndex)
    {
        if (hatSR == null || hatSprites == null || hatSprites.Length == 0) return;
        if (hatIndex < 0 || hatIndex >= hatSprites.Length) return;

        hatSR.sprite = hatSprites[hatIndex];
        hatSR.enabled = true;
    }

    // 색상 적용
    public void ApplyColor(int colorIndex)
    {
        // 스프라이트 변경
        if (playerSR != null && colorSprites != null &&
            colorIndex >= 0 && colorIndex < colorSprites.Length)
        {
            playerSR.sprite = colorSprites[colorIndex];
        }

        // 애니메이터 변경
        if (playerAni != null && colorAni != null &&
            colorIndex >= 0 && colorIndex < colorAni.Length)
        {
            playerAni.runtimeAnimatorController = colorAni[colorIndex];
        }
    }

    // 모자 끄기
    public void HideHat()
    {
        if (hatSR != null)
            hatSR.enabled = false;
    }

    // 모자 켜기
    public void ActiveHat()
    {
        if (hatSR != null)
            hatSR.enabled = true;
    }

#if UNITY_EDITOR
    // 에디터 미리보기용
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (hatSprites != null && hatSprites.Length > 0 && hatSR != null)
                hatSR.sprite = hatSprites[0];

            if (colorSprites != null && colorSprites.Length > 0 && playerSR != null)
                playerSR.sprite = colorSprites[0];
        }
    }
#endif
}