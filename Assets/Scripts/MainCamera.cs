using System;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Camera))]
public class MainCamera : SingletonComponent<MainCamera>
{
    public Camera CameraComponent;

    private void Reset()
    {
        CameraComponent = GetComponent<Camera>();
    }

    protected override void Awake()
    {
        base.Awake();
        Assert.IsNotNull(CameraComponent);
    }
}
