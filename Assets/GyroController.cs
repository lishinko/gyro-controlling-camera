//#define TEST_WITH_UI
#define USE_VELOCITY
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if TEST_WITH_UI
using UnityEngine.UI;
#endif
public class GyroController : MonoBehaviour
{

    public Camera SceneCamera;
    private Quaternion _startCamera;
    private Quaternion _startGyro;
    public float SlerpValue = 0.05f;

    public Vector2 LeftRight;
    public Vector2 BottomTop;

#if USE_VELOCITY
    private Vector3 _accuRotation = new Vector3();//累积角度变化.
#endif

#if TEST_WITH_UI
    public Text StartCam;
    public Text Gyro;
    public Text Intended;
    public Text Delta;
    public Text StartGyroInverse;
    public Text Clamped;
    public Text Velocity;

    //public InputField SlerpV;

    //public void SetSlerpValue()
    //{
    //    float v;
    //    if (float.TryParse(SlerpV.text, out v))
    //    {
    //        SlerpValue = v;
    //    }
    //}
#endif

    //一个不代表旋转的quaternion,因为我发现不能在start函数里面获得陀螺仪的数值...
    private static readonly Quaternion InvalidQuaternion = new Quaternion(0, 0, 0, 0);

    void Awake()
    {
        _startGyro = InvalidQuaternion;
    }
    // Use this for initialization
    void Start()
    {
        _startCamera = SceneCamera.transform.localRotation;
#if TEST_WITH_UI
        var euler = _startCamera.eulerAngles;
        StartCam.text = euler.ToString();
        //SlerpV.text = SlerpValue.ToString();
#endif
        Input.gyro.enabled = true;
    }
    private static readonly Quaternion _NegZ = Quaternion.Euler(0.0f, 0.0f, 180.0f);
    private static Quaternion ConvertRotation(Quaternion q)
    {
        return q;// * _NegZ;
        //return new Quaternion(q.x, q.y, -q.z, -q.w);
    }

    private Quaternion CurrentRotation
    {
        get
        {
            //return ConvertRotation(Quaternion.Euler(100, -100, 100));
            return ConvertRotation(Input.gyro.attitude);
        }
    }
    void Update()
    {
#if USE_ABSOLUTE_ROTATION
        if (_startGyro.Equals(InvalidQuaternion))
        {
            var start = Input.gyro.attitude;
            if (start.Equals(InvalidQuaternion))
            {
                return;
            }
            else
            {
                //_startGyro = Quaternion.Inverse(ConvertRotation(start));
                _startGyro = Quaternion.Inverse(CurrentRotation);
#if TEST_WITH_UI
                StartGyroInverse.text = Input.gyro.attitude.ToString();
#endif
            }

        }


        var gyro = CurrentRotation;
        var euler = gyro.eulerAngles;
#if TEST_WITH_UI
        Gyro.text = euler.ToString();
        var v = Input.gyro.rotationRateUnbiased;
        Velocity.text = v.ToString();
#endif

        var delta = gyro * _startGyro;

        euler = delta.eulerAngles;
#if TEST_WITH_UI
        Delta.text = euler.ToString();
#endif
        euler.z = _startCamera.eulerAngles.z;
        euler.x = Mathf.Clamp(CommonAngleDegreeOf(euler.x), LeftRight.x, LeftRight.y);
        euler.y = Mathf.Clamp(CommonAngleDegreeOf(euler.y), BottomTop.x, BottomTop.y );
#if TEST_WITH_UI
        Clamped.text = euler.ToString();
#endif
        var newDelta = Quaternion.Euler(euler);
        var intended = newDelta * _startCamera;
        //var delta = _startCameraTransform.InverseTransformDirection(Vector3.forward);
        //Assert.AreEqual(delta, Vector3.zero);
        //SceneCamera.transform.localRotation = intended;
        //李兴钢:不知道为什么,总感觉计算出来的摄像机坐标,x,y是反着的
        var decreasedSpeed = Quaternion.Slerp(SceneCamera.transform.localRotation, intended, SlerpValue);

        SceneCamera.transform.localRotation = decreasedSpeed;
#if TEST_WITH_UI
        Intended.text = intended.eulerAngles.ToString();
#endif
#endif
        //#if  USE_VELOCITY
        var v = Input.gyro.rotationRateUnbiased;

#if TEST_WITH_UI
        Velocity.text = v.ToString();
#endif
        var startCameraEuler = _startCamera.eulerAngles;
        _accuRotation += v;

        var x = Mathf.Clamp(CommonAngleDegreeOf(_accuRotation.x), LeftRight.x, LeftRight.y);
        var y = Mathf.Clamp(CommonAngleDegreeOf(_accuRotation.y), BottomTop.x, BottomTop.y);
        var newRotation = new Vector3(startCameraEuler.x - x, startCameraEuler.y - y, 0.0f);

        _accuRotation.x = x;
        _accuRotation.y = y;

        var intended = Quaternion.Euler(newRotation);
        var decreasedSpeed = Quaternion.Slerp(SceneCamera.transform.localRotation, intended, SlerpValue);
        SceneCamera.transform.localRotation = decreasedSpeed;
        //#endif

    }

    private static float CommonAngleDegreeOf(float angle)
    {
        var a = angle % 360.0f;
        if (a > 180.0f)
        {
            a -= 360.0f;
        }
        return a;
    }
}
