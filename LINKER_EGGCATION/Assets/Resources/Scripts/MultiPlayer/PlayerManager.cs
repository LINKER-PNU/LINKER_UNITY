using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

/// <summary>
/// Player manager.
/// Handles fire Input and Beams.
/// </summary>
public class PlayerManager : MonoBehaviourPunCallbacks
{
    #region IPunObservable implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //if (stream.IsWriting)
        //{
        //    // We own this player: send the others our data
        //    stream.SendNext(IsFiring);
        //    stream.SendNext(Health);
        //}
        //else
        //{
        //    // Network player, receive data
        //    this.IsFiring = (bool)stream.ReceiveNext();
        //    this.Health = (float)stream.ReceiveNext();
        //}
    }

    #endregion


    #region Public Fields

    //[Tooltip("The current Health of our player")]
    //public float Health = 1f;
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;

    public int CamMode;


    #endregion


    #region Private Serialize Field

    //[Tooltip("The Beams GameObject to control")]
    //[SerializeField]
    //private GameObject beams;

    ////True, when the user is firing
    //bool IsFiring;


    [SerializeField]
    private KeyCode jumpKeyCode = KeyCode.Space;
    [SerializeField]
    private KeyCode cameraKeyCode = KeyCode.Tab;

    [SerializeField]
    private GameObject fpCamera;

    [SerializeField]
    private GameObject tpCamera;

    [SerializeField]
    private CameraController fpCameraController;
    
    [SerializeField]
    private CameraController tpCameraController;


    private Movement3D movement3D;

    //[Tooltip("The Player's UI GameObject Prefab")]
    //[SerializeField]
    //private GameObject playerUiPrefab;
    #endregion


    #region Private Field

    RaycastHit hit;
    Ray ray;
    float MaxDistance = 15f;

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
            tpCamera.SetActive(false);
            //PlayerCamera.SetActive(true);
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
        // trigger Beams active state
        //if (beams != null && IsFiring != beams.activeInHierarchy)
        //{
        //    beams.SetActive(IsFiring);
        //}
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
    IEnumerator ExampleCoroutineColor()
    {
        var originColor = hit.transform.GetComponent<MeshRenderer>().material.color;
        hit.transform.GetComponent<MeshRenderer>().material.color = Color.red;

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(1);

        hit.transform.GetComponent<MeshRenderer>().material.color = originColor;
    }

    void ProcessInputs()
    {
        if(Input.GetKeyDown(cameraKeyCode))
        {
          if(CamMode == 1){
            CamMode = 0;
          }else{
            CamMode += 1;
          }
          StartCoroutine(CamChange());
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
        this.fpCameraController.RotateTo(CamMode, mouseX, mouseY);
        this.tpCameraController.RotateTo(CamMode, mouseX, mouseY);
        
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
                    Debug.Log("교");
                    GameManager.ServerCanvasObject.SetActive(!GameManager.ServerCanvasObject.activeInHierarchy);
                }
                if (!GameManager.ServerCanvasObject.activeInHierarchy && isDesk()) // 책상이면
                {
                    Debug.Log("책");
                    GameManager.ClientCanvasObject.SetActive(!GameManager.ClientCanvasObject.activeInHierarchy);
                }
                //if (hit.transform.GetComponent<MeshRenderer>().material.color != Color.red)
                //{
                //    StartCoroutine(ExampleCoroutineColor());
                //}
            }
        }
    }
    bool isTeacherDesk()
    {
        return hit.transform.name == "pCube4";
    }

    bool isDesk()
    {
        return hit.transform.name.Length == 7 && hit.transform.name.Substring(0, 5) == "pCube"
            && 37 <= int.Parse(hit.transform.name.Substring(5, 2))
            && int.Parse(hit.transform.name.Substring(5, 2)) <= 56;
    }
    IEnumerator CamChange(){
        yield return new WaitForSeconds(0.01f);
        if(CamMode == 1){
          fpCamera.SetActive(false);
          tpCamera.SetActive(true);
        }else{
          fpCamera.SetActive(true);
          tpCamera.SetActive(false);
        }
    }
    #endregion


}