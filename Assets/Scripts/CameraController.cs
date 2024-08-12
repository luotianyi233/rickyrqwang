using UnityEngine;
using Cinemachine;

public class CameraSetup : MonoBehaviour
{
    public CinemachineFreeLook freeLookCam;
    public Transform character;
    public float distanceBack = 10f;  // ������˵ľ���
    public float height = 5f;         // �����Խ�ɫ�ĸ߶�

    void Start()
    {
        freeLookCam = GetComponent<CinemachineFreeLook>();
        freeLookCam.m_YAxis.Value = 0.45f;
        freeLookCam.m_XAxis.Value = -35f;
    }
}