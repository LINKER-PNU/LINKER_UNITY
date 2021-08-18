using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class CameraController : MonoBehaviour
{
    public GameObject player;

    private float   rotateSpeedX  = 3;
    private float   rotateSpeedY  = 5;
    private float   limitMinY = -30;
    private float   limitMaxY= 30;
    private float   eulerAngleX = 3;
    private float   eulerAngleY = 3;
    
    
    public void RotateDeskMode(){
        // float rotHorizontal = -1 * rotateSpeedX * Time.deltaTime;
        transform.rotation = Quaternion.Euler(transform.rotation.x, 90, 0);
    }

    public void RotateTo(int CamMode, float mouseX, float mouseY)
    {
        eulerAngleY += mouseX  * rotateSpeedX;
        eulerAngleX -= mouseY  * rotateSpeedY;

        eulerAngleX = ClampAngle(eulerAngleX, limitMinY, limitMaxY);

        transform.rotation = Quaternion.Euler(eulerAngleX, eulerAngleY, 0);
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)   angle += 360;
        if (angle > 360)    angle -= 360;

        // Mathf.Clamp()를 이용해 angle이 min <= angle <= max를 유지하도록 함.
        return Mathf.Clamp(angle, min, max);
    }
}
