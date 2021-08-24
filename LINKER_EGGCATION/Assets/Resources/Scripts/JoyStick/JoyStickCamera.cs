using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoyStickCamera : MonoBehaviour
{

    // 공개
    public static GameObject Player;
    public Transform Stick;         // 조이스틱.

    // 비공개
    private Vector3 StickFirstPos;  // 조이스틱의 처음 위치.
    private Vector3 JoyVec;         // 조이스틱의 벡터(방향)
    private Vector3 PlayerEulerAngle;
    private float Radius;           // 조이스틱 배경의 반 지름.
    private bool MoveFlag;          // 플레이어 움직임 스위치.

    private float rotateSpeedX = 0.5f;
    private float rotateSpeedY = 50f;
    private float limitMinY = -30;
    private float limitMaxY = 30;
    private float eulerAngleX = 3;
    private float eulerAngleY = 3;

    void Start()
    {
        Radius = GetComponent<RectTransform>().sizeDelta.y * 0.5f;
        StickFirstPos = Stick.transform.position;

        // 캔버스 크기에대한 반지름 조절.
        float Can = transform.parent.GetComponent<RectTransform>().localScale.x;
        Radius *= Can;

        MoveFlag = false;
    }

    void Update()
    {
        if (PlayerEulerAngle.x != 0 ||
            PlayerEulerAngle.y != 0 ||
            PlayerEulerAngle.z != 0)
        {
            Vector3 rotDelta = Player.transform.localEulerAngles + PlayerEulerAngle;
            Player.transform.rotation = Quaternion.Euler(rotDelta);
            //Debug.Log(Player.transform.localEulerAngles);
        }
    }

    // 드래그
    public void Drag(BaseEventData _Data)
    {
        MoveFlag = true;
        PointerEventData Data = _Data as PointerEventData;
        Vector3 Pos = Data.position;

        // 조이스틱을 이동시킬 방향을 구함.(오른쪽,왼쪽,위,아래)
        JoyVec = (Pos - StickFirstPos).normalized;

        // 조이스틱의 처음 위치와 현재 내가 터치하고있는 위치의 거리를 구한다.
        float Dis = Vector3.Distance(Pos, StickFirstPos);

        // 거리가 반지름보다 작으면 조이스틱을 현재 터치하고 있는 곳으로 이동.
        if (Dis < Radius)
            Stick.position = StickFirstPos + JoyVec * Dis;
        // 거리가 반지름보다 커지면 조이스틱을 반지름의 크기만큼만 이동.
        else
            Stick.position = StickFirstPos + JoyVec * Radius;

        float X = ClampAngle(JoyVec.y, limitMinY, limitMaxY);

        //PlayerEulerAngle = new Vector3(JoyVec.x, Mathf.Atan2(JoyVec.x, JoyVec.y) * Mathf.Rad2Deg, 0);
        PlayerEulerAngle = new Vector3(-(X * Mathf.Rad2Deg * rotateSpeedX * Time.deltaTime),
                                       JoyVec.x * rotateSpeedY * Time.deltaTime,
                                       0);
    }

    // 드래그 끝.
    public void DragEnd()
    {
        Stick.position = StickFirstPos; // 스틱을 원래의 위치로.
        JoyVec = Vector3.zero;          // 방향을 0으로.
        MoveFlag = false;
        PlayerEulerAngle = new Vector3(0, 0, 0);
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;

        // Mathf.Clamp()를 이용해 angle이 min <= angle <= max를 유지하도록 함.
        return Mathf.Clamp(angle, min, max);
    }
}

