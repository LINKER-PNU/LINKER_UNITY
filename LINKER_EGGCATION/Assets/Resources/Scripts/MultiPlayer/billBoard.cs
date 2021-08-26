using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class billBoard : MonoBehaviour
{
    private Transform mainCameraTransform;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_ANDROID
        mainCameraTransform = PlayerManagerApp.MainCamera.transform;
#elif UNITY_IOS
        mainCameraTransform = PlayerManagerApp.MainCamera.transform;
#else
        mainCameraTransform = PlayerManager.MainCamera.transform;
#endif
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward,
            mainCameraTransform.rotation * Vector3.up);
    }
}
