using System.Collections;
using UnityEngine;
using Cinemachine;



public class ShakeScript : MonoBehaviour
{
    private CinemachineVirtualCamera _cinemachineVirtualCamera;
    [SerializeField] private Settings _settings;

    private void Awake()
    {
        _cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    public void ShakeCamera(float shakeİntensity)
    {
        if (_settings.isJuice)
        {
            CinemachineBasicMultiChannelPerlin cbmp =
                _cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            cbmp.m_AmplitudeGain = shakeİntensity;

            StartCoroutine(StopShake());
        }
    }

    IEnumerator StopShake()
    {
        CinemachineBasicMultiChannelPerlin cbmp=_cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        yield return new WaitForSeconds(0.2f);
        cbmp.m_AmplitudeGain = 0f;
    }
}
