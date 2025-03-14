using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class ControllerUIManager : MonoBehaviour
{
    [Serializable]
    private struct ControllerReferences
    {
        public Transform controllerTransform;
        public GameObject controllerUI;
    }

    [Serializable]
    private struct ButtonReferences
    {
        public GameObject buttonObject;    // 親オブジェクト
        public Text buttonText;           // テキストコンポーネント
    }

    [Serializable]
    private struct LeftControllerButtons
    {
        public ButtonReferences primary;    // Primary_X
        public ButtonReferences secondary;  // Secondary_Y
        public ButtonReferences menu;       // Menu
        public ButtonReferences grip;       // Grip
        public ButtonReferences trigger;    // Trigger
    }

    [Serializable]
    private struct RightControllerButtons
    {
        public ButtonReferences primary;    // Primary_A
        public ButtonReferences secondary;  // Secondary_B
        public ButtonReferences oculus;     // Oculus
        public ButtonReferences grip;       // Grip
        public ButtonReferences trigger;    // Trigger
    }

    [Header("Controllers")]
    [SerializeField] private ControllerReferences leftController;
    [SerializeField] private ControllerReferences rightController;

    [Header("Controller Buttons")]
    [SerializeField] private LeftControllerButtons leftButtons;
    [SerializeField] private RightControllerButtons rightButtons;

    [Header("Settings")]
    [SerializeField] private float activationAngleThreshold = 30.0f;

    private Camera mainCamera;
    private bool isLeftHanded = false;
    private bool isDrawingMode = false;

    private void Start()
    {
        mainCamera = Camera.main;
        UpdateControllerUI(isLeftHanded, isDrawingMode);
    }

    private void Update()
    {
        if (mainCamera == null) return;

        bool leftVisible = IsControllerVisible(mainCamera, leftController.controllerTransform);
        bool rightVisible = IsControllerVisible(mainCamera, rightController.controllerTransform);

        // 描画モード時は利き手側のUIを完全に非表示
        if (isDrawingMode)
        {
            leftController.controllerUI.SetActive(!isLeftHanded && leftVisible);
            rightController.controllerUI.SetActive(isLeftHanded && rightVisible);
        }
        else
        {
            // 通常モード時は両方のコントローラーUIを視界に応じて表示
            leftController.controllerUI.SetActive(leftVisible);
            rightController.controllerUI.SetActive(rightVisible);
        }
    }

    private bool IsControllerVisible(Camera cam, Transform controllerTransform)
    {
        if (controllerTransform == null || cam == null) return false;

        Vector3 directionToController = (controllerTransform.position - cam.transform.position).normalized;
        float angleToController = Vector3.Angle(cam.transform.forward, directionToController);
        return angleToController <= activationAngleThreshold;
    }

    public void UpdateControllerUI(bool leftHanded, bool drawingMode)
    {
        isLeftHanded = leftHanded;
        isDrawingMode = drawingMode;

        var mode = drawingMode ? ControllerUIConfig.Mode.Drawing : ControllerUIConfig.Mode.Normal;
        var visibleButtons = ControllerUIConfig.GetVisibleButtons(mode, isLeftHanded);

        // 左コントローラーのボタン更新
        UpdateButtonUI(leftButtons.primary.buttonObject, leftButtons.primary.buttonText,
            ControllerUIConfig.LeftControllerButton.Primary, visibleButtons, mode);
        UpdateButtonUI(leftButtons.secondary.buttonObject, leftButtons.secondary.buttonText,
            ControllerUIConfig.LeftControllerButton.Secondary, visibleButtons, mode);
        UpdateButtonUI(leftButtons.menu.buttonObject, leftButtons.menu.buttonText,
            ControllerUIConfig.LeftControllerButton.Menu, visibleButtons, mode);
        UpdateButtonUI(leftButtons.grip.buttonObject, leftButtons.grip.buttonText,
            ControllerUIConfig.LeftControllerButton.Grip, visibleButtons, mode);
        UpdateButtonUI(leftButtons.trigger.buttonObject, leftButtons.trigger.buttonText,
            ControllerUIConfig.LeftControllerButton.Trigger, visibleButtons, mode);

        // 右コントローラーのボタン更新
        UpdateButtonUI(rightButtons.primary.buttonObject, rightButtons.primary.buttonText,
            ControllerUIConfig.RightControllerButton.Primary, visibleButtons, mode);
        UpdateButtonUI(rightButtons.secondary.buttonObject, rightButtons.secondary.buttonText,
            ControllerUIConfig.RightControllerButton.Secondary, visibleButtons, mode);
        UpdateButtonUI(rightButtons.oculus.buttonObject, rightButtons.oculus.buttonText,
            ControllerUIConfig.RightControllerButton.Oculus, visibleButtons, mode);
        UpdateButtonUI(rightButtons.grip.buttonObject, rightButtons.grip.buttonText,
            ControllerUIConfig.RightControllerButton.Grip, visibleButtons, mode);
        UpdateButtonUI(rightButtons.trigger.buttonObject, rightButtons.trigger.buttonText,
            ControllerUIConfig.RightControllerButton.Trigger, visibleButtons, mode);
    }

    private void UpdateButtonUI(GameObject buttonObject, Text buttonText, object buttonType,
        List<object> visibleButtons, ControllerUIConfig.Mode mode)
    {
        if (buttonObject == null || buttonText == null) return;

        bool isVisible = visibleButtons.Contains(buttonType);
        buttonObject.SetActive(isVisible);

        if (isVisible)
        {
            string label = ControllerUIConfig.GetButtonLabel(mode, isLeftHanded, buttonType);
            buttonText.text = label;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateComponents();
    }

    private void ValidateComponents()
    {
        // コントローラーの検証
        Debug.AssertFormat(leftController.controllerTransform != null,
            "Left Controller Transform is missing on {0}", gameObject.name);
        Debug.AssertFormat(rightController.controllerTransform != null,
            "Right Controller Transform is missing on {0}", gameObject.name);
        Debug.AssertFormat(leftController.controllerUI != null,
            "Left Controller UI is missing on {0}", gameObject.name);
        Debug.AssertFormat(rightController.controllerUI != null,
            "Right Controller UI is missing on {0}", gameObject.name);

        // 左コントローラーのボタンを検証
        ValidateButtonReferences(leftButtons.primary, "Left Primary");
        ValidateButtonReferences(leftButtons.secondary, "Left Secondary");
        ValidateButtonReferences(leftButtons.menu, "Left Menu");
        ValidateButtonReferences(leftButtons.grip, "Left Grip");
        ValidateButtonReferences(leftButtons.trigger, "Left Trigger");

        // 右コントローラーのボタンを検証
        ValidateButtonReferences(rightButtons.primary, "Right Primary");
        ValidateButtonReferences(rightButtons.secondary, "Right Secondary");
        ValidateButtonReferences(rightButtons.oculus, "Right Oculus");
        ValidateButtonReferences(rightButtons.grip, "Right Grip");
        ValidateButtonReferences(rightButtons.trigger, "Right Trigger");
    }

    private void ValidateButtonReferences(ButtonReferences button, string buttonName)
    {
        Debug.AssertFormat(button.buttonObject != null && button.buttonText != null,
            "{0} Button components missing on {1}", buttonName, gameObject.name);
    }
#endif
}
