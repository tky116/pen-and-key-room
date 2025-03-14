using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class XRInputManager : MonoBehaviour
{
    protected XRDeviceBase _hmd;    // HMD
    protected XRDeviceBase _leftController; // 左コントローラ
    protected XRDeviceBase _rightController;    // 右コントローラ

    protected virtual void OnEnable()
    {
        InitializeDevices();
    }

    protected virtual void Update()
    {
        if (!AreDevicesValid())
            InitializeDevices();

        UpdateDevices();
    }

    /// <summary>
    /// デバイスを初期化する
    /// </summary>
    protected virtual void InitializeDevices()
    {
        if (!_hmd?.IsValid ?? true)
            InitializeDevice(InputDeviceCharacteristics.HeadMounted, ref _hmd);
        if (!_leftController?.IsValid ?? true)
            InitializeDevice(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left, ref _leftController);
        if (!_rightController?.IsValid ?? true)
            InitializeDevice(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right, ref _rightController);
    }

    /// <summary>
    /// デバイスを初期化する
    /// </summary>
    /// <param name="characteristics"></param>
    /// <param name="device"></param>
    protected virtual void InitializeDevice(InputDeviceCharacteristics characteristics, ref XRDeviceBase device)
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(characteristics, devices);
        if (devices.Count > 0)
        {
            device?.Initialize(devices[0]);
        }
    }

    /// <summary>
    /// デバイスの状態を更新する
    /// </summary>
    protected virtual void UpdateDevices()
    {
        _hmd?.UpdateState();
        _leftController?.UpdateState();
        _rightController?.UpdateState();
    }

    /// <summary>
    /// デバイスが有効かどうかを返す
    /// </summary>
    /// <returns></returns>
    protected virtual bool AreDevicesValid()
    {
        return (_hmd?.IsValid ?? false) &&
               (_leftController?.IsValid ?? false) &&
               (_rightController?.IsValid ?? false);
    }
}
