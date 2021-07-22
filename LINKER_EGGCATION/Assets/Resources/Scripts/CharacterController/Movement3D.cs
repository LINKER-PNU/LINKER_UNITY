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
    private Transform           cameraTransform;
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

    public void MoveTo(Vector3 direction)
    {
        Vector3 movedis = cameraTransform.rotation * direction;
        moveDirection = new Vector3(movedis.x, moveDirection.y, movedis.z);
    }

    public void JumpTo()
    {
        if (characterController.isGrounded == true) {
            moveDirection.y = jumpForce;
        }
    }

    // public void MoveTo(Vector3 goalPosition)
    // {
    //             Debug.Log(goalPosition);
    //     StopCoroutine("OnMove");
    //     navMeshAgent.speed = moveSpeed;
    //     navMeshAgent.SetDestination(goalPosition);
    //     StartCoroutine("OnMove");
    // }

    // IEnumerator OnMove()
    // {
    //     while (true) {
    //         if (Vector3.Distance(navMeshAgent.destination, transform.position) < 0.1f) {
    //             transform.position = navMeshAgent.destination;
    //             navMeshAgent.ResetPath();

    //             break;
    //         }

    //         yield return null;
    //     }
    // }
}
