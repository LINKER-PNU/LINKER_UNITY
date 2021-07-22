using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private KeyCode     jumpKeyCode = KeyCode.Space;
    
    [SerializeField]
    private CameraController    cameraController;
    private Movement3D          movement3D;

    private void Awake()
    {
        movement3D = GetComponent<Movement3D>();
    }

    private void Update()
    {
        // // 마우스 왼쪽버튼을 눌렀을 때
        // if (Input.GetMouseButtonDown(0))
        // {
        //     RaycastHit hit;
        //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //     if (Physics.Raycast(ray, out hit, Mathf.Infinity) ) {
        //         movement3D.MoveTo(hit.point);
        //     }
        // }

        //x, z 방향이동
        float x = Input.GetAxisRaw("Horizontal");   // 방향키 좌/우 움직임
        float z = Input.GetAxisRaw("Vertical");     // 방향키 위/아래 움직임

        movement3D.MoveTo(new Vector3(x, 0, z));
        
        if (Input.GetKeyDown(jumpKeyCode))
        {
            movement3D.JumpTo();
        }

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        cameraController.RotateTo(mouseX, mouseY);
    }
}
