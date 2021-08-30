using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoMoveController : MonoBehaviour
{
    [SerializeField]
    private float           moveSpeed = 8.0f;
    [SerializeField]
    private float           jumpForce = 4.0f;
    private float           gravity = -9.81f;
    private Vector3         moveDirection;
    [SerializeField]
    private GameObject Character;
    
    private CharacterController characterController;

    private bool isMove = false;
    
    
    private void Awake()
    {
        characterController = Character.GetComponent<CharacterController>();
    }

    private void Update()
    {

        // isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
 
        //If The Player Is On The Ground Stick To Ground And Reset Vertical Velocity
        // if (isGrounded && verticalVelocity < 0)
        // {
        //     verticalVelocity = -stickToGroundForce;
        // }
 
        // //Jump If The Player Is Grounded
        // if (isGrounded && Input.GetButtonDown("Jump"))
        // {
        //     verticalVelocity += jumpForce * stickToGroundForce;
        // }
        // Debug.Log(characterController.isGrounded);
        // verticalVelocity -= characterGravity * Time.deltaTime;
       
        if (characterController.isGrounded == false) {
            moveDirection.y += gravity * Time.deltaTime;
        }
        
        
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        
    }

    public void MoveForward()
    {
        moveDirection = new Vector3(-characterController.transform.forward.x, moveDirection.y, -characterController.transform.forward.z);
    }
    public void MoveBackward()
    {
        moveDirection = new Vector3(characterController.transform.forward.x, moveDirection.y, characterController.transform.forward.z);
    }
    public void Stop()
    {
        moveDirection = new Vector3(0,moveDirection.y,0);
    }
    public void Rotation()
    {
      characterController.transform.Rotate(0.0f, 90.0f, 0.0f, Space.Self);
    }

    public void JumpTo()
    {   
        if (characterController.isGrounded == true) {
            moveDirection.y = jumpForce;
        }
    }
}
