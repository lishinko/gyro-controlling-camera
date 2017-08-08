using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class NewMainScene : MonoBehaviour
{
    public Camera SceneCamera;
    public GyroController Gyro;

    void Awake()
    {
        Gyro.SceneCamera = SceneCamera;
    }
}
