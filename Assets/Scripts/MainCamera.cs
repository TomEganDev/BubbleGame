using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Camera))]
public class MainCamera : SingletonComponent<MainCamera>
{
    public Camera CameraComponent;
    public CinemachineBasicMultiChannelPerlin ShakeComponent;

    public float ShakeLength = 0.5f;
    public float ShakePower = 1f;
    public AnimationCurve ShakeStrengthCurve = AnimationCurve.Constant(0f, 1f, 1f);

    private void Reset()
    {
        CameraComponent = GetComponent<Camera>();
    }

    protected override void Awake()
    {
        base.Awake();
        Assert.IsNotNull(CameraComponent);
    }

    public void ScreenShake()
    {
        _ = AdjustShakeStrength();
    }
    
    private async Awaitable AdjustShakeStrength()
    {
        var beginTime = Time.time;
        var time = beginTime;
        while (time-beginTime < ShakeLength)
        {
            var t = (time - beginTime) / ShakeLength;
            var strength = ShakeStrengthCurve.Evaluate(t) * ShakePower;
            ShakeComponent.AmplitudeGain = strength;
            
            await Awaitable.NextFrameAsync();
            
            time = Time.time;
        }

        ShakeComponent.AmplitudeGain = 0f;
    }
}
