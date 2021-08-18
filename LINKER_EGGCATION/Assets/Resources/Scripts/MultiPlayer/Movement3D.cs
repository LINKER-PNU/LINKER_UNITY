using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Movement3D : MonoBehaviour
{
    [SerializeField]
    private float           moveSpeed = 5.0f;
    [SerializeField]
    private float           jumpForce = 3.0f;
    private float           gravity = -9.81f;
    private Vector3         moveDirection;

    [SerializeField]
    private Transform           fpCameraTransform;

    [SerializeField]
    private Transform           tpCameraTransform;
    
    private CharacterController characterController;
    // private NavMeshAgent    navMeshAgent;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        // navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if ( characterController.isGrounded == false) {
            moveDirection.y += gravity * Time.deltaTime;
        }
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    public void MoveTo(int CamMode,Vector3 direction)
    {
        Vector3 movedis;
        if(CamMode == 1){
          movedis = fpCameraTransform.rotation * direction;
          // lookForward = new Vector3(fpCameraTransform.forward.x, 0f, fpCameraTransform.forward.z).normalized;
          // lookRight = new Vector3(fpCameraTransform.right.x, 0f, fpCameraTransform.right.z).normalized;
          // movedis = lookForward*direction.y + lookRight*direction.x;
          
        }
        else{
          movedis =  tpCameraTransform.rotation * direction;
        }
        moveDirection = new Vector3(movedis.x, moveDirection.y, movedis.z);
    }

    // public void sitChair(int CamMode,Vector3 direction)
    // {
    //     Vector3 movedis;
    //     if(CamMode == 1){
    //       movedis = fpCameraTransform.rotation * direction;
    //       // lookForward = new Vector3(fpCameraTransform.forward.x, 0f, fpCameraTransform.forward.z).normalized;
    //       // lookRight = new Vector3(fpCameraTransform.right.x, 0f, fpCameraTransform.right.z).normalized;
    //       // movedis = lookForward*direction.y + lookRight*direction.x;
          
    //     }
    //     else{
    //       movedis =  tpCameraTransform.rotation * direction;
    //     }
    //     moveDirection = new Vector3(movedis.x, moveDirection.y, movedis.z);
    // }

    public void JumpTo()
    {
        if (characterController.isGrounded == true) {
            moveDirection.y = jumpForce;
        }
    }
}
