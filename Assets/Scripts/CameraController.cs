using UnityEngine;
using Cinemachine;

public class CameraSetup : MonoBehaviour
{
    public CinemachineFreeLook freeLookCam;
    public Transform character;
    public float distanceBack = 10f;  // 相机后退的距离
    public float height = 5f;         // 相机相对角色的高度

    void Start()
    {
        freeLookCam = GetComponent<CinemachineFreeLook>();
        freeLookCam.m_YAxis.Value = 0.45f;
        freeLookCam.m_XAxis.Value = -35f;
    }
}