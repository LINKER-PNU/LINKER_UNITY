using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMainController : MonoBehaviour
{
    /***********************************************************************
    *                               Definitions
    ***********************************************************************/
    #region .
    public enum CameraType { FpCamera, TpCamera };

    [Serializable]
    public class Components
    {
        public Camera tpCamera;
        public Camera fpCamera;

        [HideInInspector] public Transform tpRig;
        [HideInInspector] public Transform tpRoot;
        [HideInInspector] public Transform walker;
        [HideInInspector] public Transform fpRig;

        [HideInInspector] public GameObject tpCamObject;
        [HideInInspector] public GameObject fpCamObject;

        // [HideInInspector] public IMovement3D movement3D;
        [HideInInspector] public Movement3D  movement3D;


        // [HideInInspector] public Rigidbody rBody;
    }
    [Serializable]
    public class KeyOption
    {
        public KeyCode moveForward  = KeyCode.W;
        public KeyCode moveBackward = KeyCode.S;
        public KeyCode moveLeft     = KeyCode.A;
        public KeyCode moveRight    = KeyCode.D;
        public KeyCode run  = KeyCode.LeftShift;
        public KeyCode jump = KeyCode.Space;
        public KeyCode switchCamera = KeyCode.Tab;
        public KeyCode showCursor = KeyCode.LeftAlt;
    }

    [Serializable]
    public class CameraOption
    {
        [Tooltip("게임 시작 시 카메라")]
        public CameraType initialCamera;
        [Range(1f, 10f), Tooltip("카메라 상하좌우 회전 속도")]
        public float rotationSpeed = 2f;
        [Range(-90f, 0f), Tooltip("올려다보기 제한 각도")]
        public float lookUpDegree = -60f;
        [Range(0f, 75f), Tooltip("내려다보기 제한 각도")]
        public float lookDownDegree = 75f;
        [Range(0f, 3.5f), Space, Tooltip("줌 확대 최대 거리")]
        public float zoomInDistance = 3f;

        [Range(0f, 5f), Tooltip("줌 축소 최대 거리")]
        public float zoomOutDistance = 3f;

        [Range(1f, 20f), Tooltip("줌 속도")]
        public float zoomSpeed = 10f;

        [Range(0.01f, 0.5f), Tooltip("줌 가속")]
          public float zoomAccel = 0.1f;
    }
        [Serializable]
    public class MovementOption
    {
        [Range(1f, 10f), Tooltip("이동속도")]
        public float speed = 3f;
        [Range(1f, 3f), Tooltip("달리기 이동속도 증가 계수")]
        public float runningCoef = 1.5f;
        [Range(1f, 10f), Tooltip("점프 강도")]
        public float jumpForce = 5.5f;
        [Tooltip("지면으로 체크할 레이어 설정")]
        public LayerMask groundLayerMask = -1;
        [Range(0.0f, 2.0f), Tooltip("점프 쿨타임")]
        public float jumpCooldown = 1.0f;
        
    }
    [Serializable]
    public class CharacterState
    {
        public bool isCurrentFp;
        public bool isMoving;
        public bool isRunning;
        public bool isGrounded;
    }


    #endregion
    /***********************************************************************
    *                               Fields, Properties
    ***********************************************************************/
    #region .

    [SerializeField] private Components _components = new Components();
    [Space,SerializeField] private KeyOption _keyOption = new KeyOption();
    [Space,SerializeField] private MovementOption _movementOption = new MovementOption();
    
    [Space,SerializeField] private CameraOption   _cameraOption   = new CameraOption();
    [Space,SerializeField] private CharacterState _state = new CharacterState();

    public Components Com => _components;
    public KeyOption Key => _keyOption;
    public MovementOption MoveOption => _movementOption;
    public CameraOption   CamOption  => _cameraOption;
    public CharacterState State => _state;

  
    private float _deltaTime;
    private Vector2 _rotation;

    [SerializeField]
    private float _distFromGround;


    private float _tpCamZoomInitialDistance;
    private float _tpCameraWheelInput = 0;
    private float _currentWheel;
 
    [SerializeField]
    private Vector3 _moveDir;

    [SerializeField]
    private Vector3 _worldMoveDir;



    
    
    private float _groundCheckRadius;

    //movement

    private float _currentJumpCooldown;
    private Vector3 _worldMove;
    #endregion

    /***********************************************************************
    *                               Unity Events
    ***********************************************************************/
    #region .
    private void Start()
    {
        InitComponents();
        InitSettings();
    }


    private void Update()
    {
        _deltaTime = Time.deltaTime;
        CameraViewToggle();
        Move();
        Jump();
        SetValuesByKeyInput();
        Rotate();
        TpCameraZoom();
        CheckGroundDistance();


        UpdateCurrentValues();
        
        
    }

    #endregion
    /***********************************************************************
    *                               Init Methods
    ***********************************************************************/
    #region .
    private void InitComponents()
    {
        LogNotInitializedComponentError(Com.tpCamera, "TP Camera");
        LogNotInitializedComponentError(Com.fpCamera, "FP Camera");
        
        // TryGetComponent(out Com.rBody);

        Com.tpCamObject = Com.tpCamera.gameObject;
        Com.tpRig = Com.tpCamera.transform.parent;
        Com.tpRoot = Com.tpRig.parent;

        Com.fpCamObject = Com.fpCamera.gameObject;
        Com.fpRig = Com.fpCamera.transform.parent;
        Com.walker = Com.fpRig.parent;

        TryGetComponent(out Com.movement3D);

        
    }

    private void InitSettings()
    {

        // if (Com.rBody)
        // {
        //     // 회전은 트랜스폼을 통해 직접 제어할 것이기 때문에 리지드바디 회전은 제한
        //     Com.rBody.constraints = RigidbodyConstraints.FreezeRotation;
        // }
        // Camera
        var allCams = FindObjectsOfType<Camera>();
        foreach (var cam in allCams)
        {
            cam.gameObject.SetActive(false);
        }
        // 설정한 카메라 하나만 활성화
        State.isCurrentFp = (CamOption.initialCamera == CameraType.FpCamera);
        Com.fpCamObject.SetActive(State.isCurrentFp);
        Com.tpCamObject.SetActive(!State.isCurrentFp);

        TryGetComponent(out CapsuleCollider cCol);
        _groundCheckRadius = cCol ? cCol.radius : 0.1f;
        _tpCamZoomInitialDistance = Vector3.Distance(Com.tpRig.position, Com.tpCamera.transform.position);

    }

    #endregion
    /***********************************************************************
    *                               Checker Methods
    ***********************************************************************/
    #region .
    private void LogNotInitializedComponentError<T>(T component, string componentName) where T : Component
    {
        if(component == null)
            Debug.LogError($"{componentName} 컴포넌트를 인스펙터에 넣어주세요");
    }

    #endregion
    /***********************************************************************
    *                               Methods
    ***********************************************************************/
    #region .
    private void SetValuesByKeyInput()
    {
        float h = 0f, v = 0f;

        if (Input.GetKey(Key.moveForward)) v += 1.0f;
        if (Input.GetKey(Key.moveBackward)) v -= 1.0f;
        if (Input.GetKey(Key.moveLeft)) h -= 1.0f;
        if (Input.GetKey(Key.moveRight)) h += 1.0f;

        // Move, Rotate
        // SendMoveInfo(h, v);
        // _rotation = new Vector2(Input.GetAxisRaw("Mouse X"), -Input.GetAxisRaw("Mouse Y"));
        // State.isMoving = h != 0 || v != 0;
        // State.isRunning = Input.GetKey(Key.run);

        // float x = Input.GetAxisRaw("Horizontal");   // 방향키 좌/우 움직임
        // float z = Input.GetAxisRaw("Vertical");     // 방향키 위/아래 움직임

        
        
        


        Vector3 moveInput = new Vector3(h, 0f, v).normalized;
        _moveDir = Vector3.Lerp(_moveDir, moveInput, 0.1f); // 가속, 감속
        _rotation = new Vector2(Input.GetAxisRaw("Mouse X"), -Input.GetAxisRaw("Mouse Y"));

        State.isMoving = _moveDir.sqrMagnitude > 0.01f;
        State.isRunning = Input.GetKey(Key.run);

        // Jump
        if (Input.GetKeyDown(Key.jump))
        {
            Debug.Log("Jump");
            Jump();
        }

        // Wheel
        _tpCameraWheelInput = Input.GetAxisRaw("Mouse ScrollWheel");
        _currentWheel = Mathf.Lerp(_currentWheel, _tpCameraWheelInput, CamOption.zoomAccel);
    }
    private void Rotate()
    {
        Transform root, rig;

        // 1인칭
        if (State.isCurrentFp)
        {
            root = Com.walker;
            rig = Com.fpRig;
        }
        // 3인칭
        else
        {
            root = Com.tpRoot;
            rig = Com.tpRig;
            RotateWalker(); // 3인칭일 경우 Walker를 이동방향으로 회전
        }


        // 회전 ==========================================================
        float deltaCoef = _deltaTime * 50f;

        // 상하 : Rig 회전
        float xRotPrev = rig.localEulerAngles.x;
        float xRotNext = xRotPrev + _rotation.y
            * CamOption.rotationSpeed * deltaCoef;

        if (xRotNext > 180f)
            xRotNext -= 360f;

        // 좌우 : Root 회전
        float yRotPrev = root.localEulerAngles.y;
        float yRotNext =
            yRotPrev + _rotation.x
            * CamOption.rotationSpeed * deltaCoef;

        // 상하 회전 가능 여부
        bool xRotatable =
            CamOption.lookUpDegree < xRotNext &&
            CamOption.lookDownDegree > xRotNext;

        // Rig 상하 회전 적용
        rig.localEulerAngles = Vector3.right * (xRotatable ? xRotNext : xRotPrev);

        // Root 좌우 회전 적용
        root.localEulerAngles = Vector3.up * yRotNext;
    }


    
    private void RotateWalker()
    {
        if(State.isMoving == false) return;

        Vector3 dir = Com.tpRig.TransformDirection(_moveDir);
        float currentY = Com.walker.localEulerAngles.y;
        float nextY = Quaternion.LookRotation(dir, Vector3.up).eulerAngles.y;

        if (nextY - currentY > 180f) nextY -= 360f;
        else if (currentY - nextY > 180f) nextY += 360f;

        Com.walker.eulerAngles = Vector3.up * Mathf.Lerp(currentY, nextY, 0.1f);
    }

    private void CameraViewToggle()
    {
        if (Input.GetKeyDown(Key.switchCamera))
        {
            State.isCurrentFp = !State.isCurrentFp;
            Com.fpCamObject.SetActive(State.isCurrentFp);
            Com.tpCamObject.SetActive(!State.isCurrentFp);

            // TP -> FP
            if (State.isCurrentFp)
            {
                Com.walker.localEulerAngles = Vector3.up * Com.tpRoot.localEulerAngles.y;
                Com.fpRig.localEulerAngles = Vector3.right * Com.tpRig.localEulerAngles.x;
            }
            // FP -> TP
            else
            {
                Com.tpRoot.localEulerAngles = Vector3.up * Com.walker.localEulerAngles.y;
                Com.tpRig.localEulerAngles = Vector3.right * Com.fpRig.localEulerAngles.x;
            }
        }
    }
    private void TpCameraZoom()
    {
      if (State.isCurrentFp) return;                // TP 카메라만 가능
      if (Mathf.Abs(_currentWheel) < 0.01f) return; // 휠 입력 있어야 가능

      Transform tpCamTr = Com.tpCamera.transform;
      Transform tpCamRig = Com.tpRig;

      float zoom = _deltaTime * CamOption.zoomSpeed;
      float currentCamToRigDist = Vector3.Distance(tpCamTr.position, tpCamRig.position);
      Vector3 move = Vector3.forward * zoom * _currentWheel * 10f;

      // Zoom In
      if (_currentWheel > 0.01f)
      {
          if (_tpCamZoomInitialDistance - currentCamToRigDist < CamOption.zoomInDistance)
          {
              tpCamTr.Translate(move, Space.Self);
          }
      }
      // Zoom Out
      else if (_currentWheel < -0.01f)
      {

          if (currentCamToRigDist - _tpCamZoomInitialDistance < CamOption.zoomOutDistance)
          {
              tpCamTr.Translate(move, Space.Self);
          }
      }
    }
    #endregion
    #region .
    private void CheckGroundDistance()
    {
        // _distFromGround = Com.movement3D.GetDistanceFromGround();
        // State.isGrounded = Com.movement3D.IsGrounded();

        Vector3 ro = transform.position + Vector3.up;
        Vector3 rd = Vector3.down;
        Ray ray = new Ray(ro, rd);

        const float rayDist = 500f;
        const float threshold = 0.01f;

        bool cast =
            Physics.SphereCast(ray, _groundCheckRadius, out var hit, rayDist, MoveOption.groundLayerMask);

        _distFromGround = cast ? (hit.distance - 1f + _groundCheckRadius) : float.MaxValue;
        State.isGrounded = _distFromGround <= _groundCheckRadius + threshold;

    }
    private void Move()
    {
        // 이동하지 않는 경우, 미끄럼 방지
        if (State.isMoving == false)
        {
            Com.movement3D.MoveTo(new Vector3(0f,0,0f));
            // Com.rBody.velocity = new Vector3(0f, Com.rBody.velocity.y, 0f);
            return;
        }
         if (State.isCurrentFp)
        {
            _worldMove = Com.walker.TransformDirection(_moveDir);
        }
        // 3인칭
        else
        {
            _worldMove = Com.tpRig.TransformDirection(_moveDir);
        }

        _worldMove *= (MoveOption.speed) * (State.isRunning ? MoveOption.runningCoef : 1f);

        Com.movement3D.MoveTo(new Vector3(_worldMove.x, 0, _worldMove.z));
        // Y축 속도는 유지하면서 XZ평면 이동
        // Com.rBody.velocity = new Vector3(_worldMove.x, Com.rBody.velocity.y, _worldMove.z);
        // Debug.Log(Com.rBody);
    }
    private void Jump()
    {
        //  bool jumpSucceeded = Com.movement3D.SetJump();

        // if (jumpSucceeded)
        // {

        //     Debug.Log("JUMP");
        // }
        if (!State.isGrounded) 
        { 
          Debug.Log("그라운드");
          return;
        }
        if (_currentJumpCooldown > 0f) return; // 점프 쿨타임

        if (Input.GetKeyDown(Key.jump))
        {
            Debug.Log("JUMP");

            // 하강 중 점프 시 속도가 합산되지 않도록 속도 초기화
            // Com.rBody.velocity = Vector3.zero;

            // Com.rBody.AddForce(Vector3.up * MoveOption.jumpForce, ForceMode.VelocityChange);

            // 애니메이션 점프 트리거
            // Com.anim.SetTrigger(AnimOption.paramJump);

            // 쿨타임 초기화
            Com.movement3D.JumpTo();
        
            _currentJumpCooldown = MoveOption.jumpCooldown;
        }
    }
    private void UpdateCurrentValues()
    {
        if(_currentJumpCooldown > 0f)
            _currentJumpCooldown -= _deltaTime;
    }

    
    private void SendMoveInfo(float horizontal, float vertical)
    {
        _moveDir = new Vector3(horizontal, 0f, vertical).normalized;

        if (State.isCurrentFp)
        {
            _worldMoveDir = Com.walker.TransformDirection(_moveDir);
        }
        else
        {
            _worldMoveDir = Com.tpRoot.TransformDirection(_moveDir);
        }

        // Com.movement3D.SetMovement(_worldMoveDir, State.isRunning);
  
    }
    #endregion
    #region .
    // public void KnockBack(in Vector3 force, float time)
    // {
    //     Com.movement3D.KnockBack(force, time);
    // }

    

   
    #endregion
}