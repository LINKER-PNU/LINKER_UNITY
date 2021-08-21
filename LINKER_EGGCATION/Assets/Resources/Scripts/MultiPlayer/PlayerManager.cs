using UnityEngine;
using UnityEngine.UI;
using TMPro;

using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

/// <summary>
/// Player manager.
/// Handles fire Input and Beams.
/// </summary>
public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
{
    #region IPunObservable implementation
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(IsEmotionsActive);
            // We own this player: send the others our data
            //for (int i = 0; i < Emotions.Length; i++)
            //{
            //    Debug.LogFormat("{0} send", IsEmotionsActive[i]);
            //}
        }
        else
        {
            Debug.Log("!!!receive!!!");
            IsEmotionsActive = (bool[])stream.ReceiveNext();
            // Network player, receive data
            //for (int i = 0; i < Emotions.Length; i++)
            //{
            //    Debug.LogFormat("{0} receive", IsEmotionsActive[i]);
            //}
        }
    }

    #endregion


    #region Public Fields

    //[Tooltip("The current Health of our player")]
    //public float Health = 1f;
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;

    public int CamMode;

    static public Camera MainCamera;

    #endregion


    #region Private Serialize Field

    //[Tooltip("The Beams GameObject to control")]
    //[SerializeField]
    //private GameObject beams;

    ////True, when the user is firing
    bool[] IsEmotionsActive;

    [SerializeField]
    const int MaximumEmotionCount = 5;

    [SerializeField]
    private GameObject[] Emotions;

    [SerializeField]
    private KeyCode jumpKeyCode = KeyCode.Space;
    [SerializeField]
    private KeyCode CAMERA_KEY_CODE = KeyCode.Tab;
    [SerializeField]
    private KeyCode EMOTION1_KEYCODE = KeyCode.Alpha1;
    [SerializeField]
    private KeyCode EMOTION2_KEYCODE = KeyCode.Alpha2;
    [SerializeField]
    private KeyCode EMOTION3_KEYCODE = KeyCode.Alpha3;
    [SerializeField]
    private KeyCode NOTICE_KEYCODE = KeyCode.I;
    [SerializeField]
    private KeyCode ESC_KEYCODE = KeyCode.Escape;

    [SerializeField]
    private GameObject fpCamera;

    [SerializeField]
    private GameObject tpCamera;

    [SerializeField]
    private CameraController fpCameraController;
    
    [SerializeField]
    private CameraController tpCameraController;

    [SerializeField]
    private TextMeshProUGUI nameText;

    private Movement3D movement3D;


    //[Tooltip("The Player's UI GameObject Prefab")]
    //[SerializeField]
    //private GameObject playerUiPrefab;
    #endregion


    #region Private Field

    RaycastHit hit;
    Ray ray;
    float MaxDistance = 15f;
    Vector3 vel = Vector3.zero;
    

    #endregion

    #region MonoBehaviour CallBacks


    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
    /// </summary>
    void Awake()
    {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine)
        {
            PlayerManager.LocalPlayerInstance = this.gameObject;
            Debug.Log("Control My Camera");
            LocalPlayerInstance.GetComponent<Movement3D>().enabled = true;
            CamMode = 1;
            fpCamera.SetActive(true);
            MainCamera = fpCamera.GetComponent<Camera>();
            tpCamera.SetActive(false);


            // 감정표현 개수가 MaximumEmotionCount 이상이면 MaximumEmotionCount를 수정해줘야합니다.
            IsEmotionsActive = new bool[MaximumEmotionCount] {false, false, false, false, false};

        }
        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);

        movement3D = GetComponent<Movement3D>();

        //if (beams == null)
        //{
        //    Debug.LogError("<Color=Red><a>Missing</a></Color> Beams Reference.", this);
        //}
        //else
        //{
        //    beams.SetActive(false);
        //}
    }
    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    void Start()
    {
        SetName();
        //if (playerUiPrefab != null)
        //{
        //    GameObject _uiGo = Instantiate(playerUiPrefab);
        //    _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        //}
        //else
        //{
        //    Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
        //}

        //Photon.Pun.Demo.PunBasics.CameraWork _cameraWork = this.gameObject.GetComponent<Photon.Pun.Demo.PunBasics.CameraWork>();
        //if (_cameraWork != null)
        //{
        //    if (photonView.IsMine)
        //    {
        //        _cameraWork.OnStartFollowing();
        //    }
        //}
        //else
        //{
        //    Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
        //}
    }
    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity on every frame.
    /// </summary>
    void Update()
    {
        if (photonView.IsMine)
        {
            ProcessInputs();
      
              
            
            
        }
        //if (Health <= 0f)
        //{
        //    GameManager.Instance.LeaveRoom();
        //}
        for (int i = 0; i < Emotions.Length; i++)
        {
            if (IsEmotionsActive != null && IsEmotionsActive[i] != Emotions[i].activeInHierarchy)
            {
                Emotions[i].SetActive(IsEmotionsActive[i]);
            }
        }
    }

    /// <summary>
    /// MonoBehaviour method called when the Collider 'other' enters the trigger.
    /// Affect Health of the Player if the collider is a beam
    /// Note: when jumping and firing at the same, you'll find that the player's own beam intersects with itself
    /// One could move the collider further away to prevent this or check if the beam belongs to the player.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        //if (!photonView.IsMine)
        //{
        //    return;
        //}
        //// We are only interested in Beamers
        //// we should be using tags but for the sake of distribution, let's simply check by name.
        //if (!other.name.Contains("Beam"))
        //{
        //    return;
        //}
    }
    /// <summary>
    /// MonoBehaviour method called once per frame for every Collider 'other' that is touching the trigger.
    /// We're going to affect health while the beams are touching the player
    /// </summary>
    /// <param name="other">Other.</param>
    void OnTriggerStay(Collider other)
    {
        // we dont' do anything if we are not the local player.
        //if (!photonView.IsMine)
        //{
        //    return;
        //}
        //if (!other.name.Contains("Beam"))
        //{
        //    return;
        //}
    }
    IEnumerator CoroutineEmotion(int i)
    {
        IsEmotionsActive[i] = true;

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(1f);

        IsEmotionsActive[i] = false;
    }

    void SetName()
    {
        nameText.text = photonView.Owner.NickName;
    }

    bool IsAllEmotionInactive()
    {
        foreach(var EmotionActive in IsEmotionsActive)
        {
            Debug.Log(EmotionActive);
            if (EmotionActive) return false;
        }
        return true;
    }

  

    void ProcessInputs()
    {
        if(!GameManager.isDeskMode){
          if(Input.GetKeyDown(CAMERA_KEY_CODE))
          {
              StartCoroutine(CamChange());
              if (CamMode == 1){
              CamMode = 0;
              }else{
                CamMode += 1;
              }
          }          
          //x, z 방향이동
          float x = Input.GetAxisRaw("Horizontal");   // 방향키 좌/우 움직임
          float z = Input.GetAxisRaw("Vertical");     // 방향키 위/아래 움직임

          this.movement3D.MoveTo(CamMode,new Vector3(x, 0, z));

          if (Input.GetKeyDown(jumpKeyCode))
          {
              this.movement3D.JumpTo();
          }
          float mouseX = Input.GetAxis("Mouse X");
          float mouseY = Input.GetAxis("Mouse Y");
          fpCameraController.RotateTo(CamMode, mouseX, mouseY);
          tpCameraController.RotateTo(CamMode, mouseX, mouseY);
        } 
       
        
        // 감정표현 부분입니다.
        if (Input.GetKeyDown(EMOTION1_KEYCODE))
        {
            if (IsAllEmotionInactive())
            {
                StartCoroutine(CoroutineEmotion(0));
            }
        }
        if (Input.GetKeyDown(EMOTION2_KEYCODE))
        {
            if (IsAllEmotionInactive())
            {
                StartCoroutine(CoroutineEmotion(1));
            }
        }
        if (Input.GetKeyDown(EMOTION3_KEYCODE))
        {
            if (IsAllEmotionInactive())
            {
                StartCoroutine(CoroutineEmotion(2));
            }
        }

        // 공지기능 부분입니다.
        if (Input.GetKeyDown(NOTICE_KEYCODE))
        {
        }

        // ESC기능 부분입니다.
        if (Input.GetKeyDown(ESC_KEYCODE))
        {
        }

        // 상호작용 부분입니다
        if (Input.GetMouseButtonDown(0)) // 마우스 좌클릭시
        {
            ray = fpCamera.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            //var cameraController = CamMode == 1 ? tpCamera : fpCamera;
            Debug.DrawRay(ray.origin, ray.direction, Color.blue, 0.3f);
            // 클릭 시 앞에 물건이 있을 때
            if (Physics.Raycast(ray, out hit, MaxDistance))
            {
                Debug.Log(hit.transform?.name);
                if (!GameManager.ClientCanvasObject.activeInHierarchy && isTeacherDesk()) // 교탁이면
                {
                    if (!GameManager.createClassPanel.activeInHierarchy)
                    {
                        GameManager.createClassPanel.SetActive(true);
                    }
                }
                // if (!GameManager.ServerCanvasObject.activeInHierarchy && isDesk()) // 책상이면
                // {
                //     Debug.Log("책");
                //     if (GameManager.checkClassExist())
                //     {
                //         GameManager.ClientCanvasObject.SetActive(true);
                //         GameManager.leaveClassBtn.SetActive(true);
                //     }
                //     else
                //     {
                //         GameManager.Instance.StartCoroutineIsNotExist();
                //     }
                GameObject tempChair = null;
                
                tempChair = GameObject.Find("chair"+hit.transform.name.Substring(4));
                // Debug.Log(int.Parse(hit.transform.name.Substring(4)), tempChair);

                if(isDesk()){
                  GameManager.AimObject.SetActive(false);
                  Vector3 newPos = new Vector3(tempChair.transform.position.x,tempChair.transform.position.y + 5f,tempChair.transform.position.z);
                  this.transform.position = newPos;
                  StartCoroutine(CamChange());
                  if (CamMode == 1){
                    CamMode = 0;
                  }
                  fpCameraController.RotateDeskMode();
                  Cursor.visible = true;
                  Cursor.lockState = CursorLockMode.None;
                  GameManager.isDeskMode = true;
                  GameManager.DeskModeObject.SetActive(true);
                }

                // if (!GameManager.ClientCanvasObject.activeInHierarchy && isTeacherDesk()) // 교탁이면
                // {
                //     Debug.Log("교");
                //     GameManager.ServerCanvasObject.SetActive(!GameManager.ServerCanvasObject.activeInHierarchy);
                // }
                // if (!GameManager.ServerCanvasObject.activeInHierarchy && isDesk()) // 책상이면
                // {
                //     Debug.Log("책");
                    
                //     // GameManager.ClientCanvasObject.SetActive(!GameManager.ClientCanvasObject.activeInHierarchy);
                // }
            }
        }
    }

    public void OnCreateClassConfirm()
    {
        Debug.Log("교");
        if (GameManager.checkClassExist())
        {
            GameManager.Instance.StartCoroutineAlreadyExist();
            GameManager.createClassPanel.SetActive(false);
        }
        else
        {
            GameManager.ServerCanvasObject.SetActive(true);
            GameManager.createClassPanel.SetActive(false);
            GameManager.leaveClassBtn.SetActive(true);
        }
    }
    public void OnCreateClassCancle()
    {
        GameManager.createClassPanel.SetActive(false);
    }

    static public void OnLeaveClass()
    {
        GameManager.ServerCanvasObject.SetActive(false);
        GameManager.ClientCanvasObject.SetActive(false);
        GameManager.leaveClassBtn.SetActive(false);
    }

    bool isTeacherDesk()
    {
        return hit.transform.name == "pCube4" || hit.transform.name =="TeacherDesk";
    }

    bool isDesk()
    {
        return hit.transform.name.Substring(0, 4) == "desk" ;
        // return (hit.transform.name.Length == 7 && hit.transform.name.Substring(0, 5) == "pCube"
        //     && 37 <= int.Parse(hit.transform.name.Substring(5, 2))
        //     && int.Parse(hit.transform.name.Substring(5, 2)) <= 56)|| hit.transform.name.Substring(0, 4) == "desk" ;
    }
    IEnumerator CamChange(){
        yield return new WaitForSeconds(0.01f);
        if(CamMode == 1){
            fpCamera.SetActive(false);
            tpCamera.SetActive(true);
            MainCamera = tpCamera.GetComponent<Camera>();
        }
        else{
            fpCamera.SetActive(true);
            tpCamera.SetActive(false);
            MainCamera = fpCamera.GetComponent<Camera>();
        }
    }
    #endregion


}