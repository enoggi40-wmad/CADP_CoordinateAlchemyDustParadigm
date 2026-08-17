using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 현재 선택된 현상과
/// Smart Dust 모드 활성 상태를 기준으로
/// 현상별 Preview 오브젝트를 활성화/비활성화한다.
///
/// 예:
/// Smart Dust 모드 ON
/// + AirPressureGradient 선택
/// → Air Pressure Preview ON
///
/// Smart Dust 모드 OFF
/// → 모든 Preview OFF
///
/// SmartDustPanel / PhenomenonInfoUI 전환만 일어난 경우에는
/// Smart Dust 모드 자체가 유지되므로 Preview도 유지된다.
/// </summary>
public class PhenomenonPreviewController : MonoBehaviour
{
    /// <summary>
    /// PhenomenonType과
    /// 해당 현상의 Preview 오브젝트를 묶는 데이터.
    ///
    /// 나중에 현상이 추가되면
    /// Inspector에서 Entry만 추가하면 된다.
    /// </summary>
    [Serializable]
    private class PreviewEntry
    {
        [Tooltip("이 Preview가 대응하는 현상")]
        public PhenomenonType phenomenonType;

        [Tooltip("해당 현상을 선택했을 때 표시할 Preview 오브젝트")]
        public GameObject previewObject;
    }


    [Header("References")]

    [Tooltip("현재 선택된 현상을 관리하는 컨트롤러")]
    [SerializeField]
    private SmartDustDeploymentController deploymentController;

    [Tooltip("Smart Dust 모드 ON/OFF 상태를 관리하는 컨트롤러")]
    [SerializeField]
    private SmartDustModeController smartDustModeController;


    [Header("Preview Entries")]

    [Tooltip("현상별 Preview 목록")]
    [SerializeField]
    private List<PreviewEntry> previewEntries =
        new List<PreviewEntry>();


    private void Awake()
    {
        /*
         * Inspector에서 참조를 연결하지 않았을 경우
         * 자동으로 찾는다.
         */
        if (deploymentController == null)
        {
            deploymentController =
                FindFirstObjectByType<SmartDustDeploymentController>();
        }

        if (smartDustModeController == null)
        {
            smartDustModeController =
                FindFirstObjectByType<SmartDustModeController>();
        }


        /*
         * 게임 시작 시에는
         * 모든 Preview를 비활성화한다.
         */
        HideAllPreviews();
    }


    private void OnEnable()
    {
        /*
         * 선택된 현상이 변경될 때
         * Preview 상태를 다시 계산한다.
         */
        if (deploymentController != null)
        {
            deploymentController.OnSelectedPhenomenonChanged +=
                HandlePhenomenonChanged;
        }


        /*
         * Smart Dust 모드가 ON/OFF될 때도
         * Preview 상태를 다시 계산한다.
         */
        if (smartDustModeController != null)
        {
            smartDustModeController.OnSmartDustModeChanged +=
                HandleSmartDustModeChanged;
        }


        /*
         * 이 컴포넌트가 활성화되는 순간
         * 현재 상태를 한 번 즉시 반영한다.
         */
        RefreshPreview();
    }


    private void OnDisable()
    {
        /*
         * 이벤트 구독 해제.
         *
         * 오브젝트가 다시 활성화될 때
         * 중복 구독되는 것을 방지한다.
         */
        if (deploymentController != null)
        {
            deploymentController.OnSelectedPhenomenonChanged -=
                HandlePhenomenonChanged;
        }

        if (smartDustModeController != null)
        {
            smartDustModeController.OnSmartDustModeChanged -=
                HandleSmartDustModeChanged;
        }
    }


    /// <summary>
    /// Dropdown에서 선택된 현상이 변경되었을 때 호출된다.
    /// </summary>
    private void HandlePhenomenonChanged(
        PhenomenonType phenomenonType)
    {
        RefreshPreview();
    }


    /// <summary>
    /// Smart Dust 모드가
    /// ON 또는 OFF 되었을 때 호출된다.
    /// </summary>
    private void HandleSmartDustModeChanged(
        bool isActive)
    {
        RefreshPreview();
    }


    /// <summary>
    /// 현재 Smart Dust 모드 상태와
    /// 선택된 현상을 기준으로
    /// Preview 상태를 갱신한다.
    /// </summary>
    private void RefreshPreview()
    {
        /*
         * 필요한 컨트롤러가 없으면
         * Preview를 표시할 수 없으므로
         * 모두 숨긴다.
         */
        if (deploymentController == null ||
            smartDustModeController == null)
        {
            HideAllPreviews();
            return;
        }


        /*
         * Smart Dust 모드가 꺼져 있으면
         * 선택된 현상과 관계없이
         * 모든 Preview를 숨긴다.
         */
        if (!smartDustModeController.IsSmartDustModeActive)
        {
            HideAllPreviews();
            return;
        }


        PhenomenonType selectedPhenomenon =
            deploymentController.SelectedPhenomenon;


        /*
         * 현재 선택된 현상과 일치하는
         * Preview 하나만 활성화한다.
         */
        foreach (PreviewEntry entry
                 in previewEntries)
        {
            if (entry == null ||
                entry.previewObject == null)
            {
                continue;
            }


            bool shouldShow =
                entry.phenomenonType ==
                selectedPhenomenon;


            entry.previewObject.SetActive(
                shouldShow
            );
        }
    }


    /// <summary>
    /// 등록된 모든 Preview를 비활성화한다.
    /// </summary>
    public void HideAllPreviews()
    {
        foreach (PreviewEntry entry
                 in previewEntries)
        {
            if (entry == null ||
                entry.previewObject == null)
            {
                continue;
            }

            entry.previewObject.SetActive(
                false
            );
        }
    }
}