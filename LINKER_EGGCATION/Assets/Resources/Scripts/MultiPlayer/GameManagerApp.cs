using System;
using System.Collections;
using System.Net;
using System.IO;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using Photon.Realtime;

using Newtonsoft.Json.Linq;
using eggcation;

public class GameManagerApp : MonoBehaviourPunCallbacks
{

    #region Private Fields

    [SerializeField]
    private GameObject emptyObject;

    [SerializeField]
    private GameObject canvasObject;

    [SerializeField]
    private GameObject RoomNameObject;

    [SerializeField]
    private GameObject JoinCodeTextObject;

    RaycastHit hit;
    Ray ray;
    float MaxDistance = 15f;
    Vector3 vel = Vector3.zero;


    #endregion


    #region Public Fields

    static public GameObject topPanelObject;

    static public GameObject escPanelObject;

    static public GameObject boardPanelObject;

    static public GameObject createClassPanel;

    static public GameObject ServerCanvasObject;
    
    static public GameObject ClientCanvasObject;

    static public GameObject isNotExistObject;

    static public GameObject classCreateFaildInMobileObject;
    
    static public GameObject DeskModeObject;

    static public GameObject AimObject;

    static public GameObject JoyStickObject;

    static public GameObject JoyStickCameraObject;

    static public GameObject MicBtnObject;

    static public GameObject VoiceBtnObject;

    static public GameObject BoardTextObject;

    static public GameObject BoardBtnObject;

    static public GameObject ClickBtnObject;

    static public GameObject JumpBtnObject;

    static public GameObject EmotionPanelObject;


    static public GameObject TeacherChairObject;


    static public bool isMouseMode = false;

    public static GameObject fpCamera;

    public static CameraController fpCameraController;


    public static GameManagerApp Instance;

    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;


    #endregion

    #region MonoBehaviour CallBacks

    void Start()
    {
        
        Instance = this;
        if (playerPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
        }
        else
        {
            if (PlayerManagerApp.LocalPlayerInstance == null)
            {
                Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }

        RoomNameObject.GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.Name;

        JoinCodeTextObject.SetActive(true);
        string joinCode = RoomName_To_JoinCode(PhotonNetwork.CurrentRoom.Name);
        Text JoinCodeText = JoinCodeTextObject.GetComponent<Text>();
        JoinCodeText.text = joinCode;

        // Find 연산은 자원을 많이 먹으므로 Awake에서 한번 실행해줍니다.
        createClassPanel = canvasObject.transform.Find("createClass_panel").gameObject;
        topPanelObject = canvasObject.transform.Find("Top_panel").gameObject;
        escPanelObject = canvasObject.transform.Find("ESC_panel").gameObject;
        boardPanelObject = canvasObject.transform.Find("Board_panel").gameObject;
        ServerCanvasObject = emptyObject.transform.Find("ServerVideoCanvas").gameObject;
        ClientCanvasObject = emptyObject.transform.Find("ClientVideoCanvas").gameObject;
        isNotExistObject = canvasObject.transform.Find("isNotExist_text").gameObject;
        classCreateFaildInMobileObject = canvasObject.transform.Find("classCreateFaildInMobile_text").gameObject;
        DeskModeObject = canvasObject.transform.Find("DeskMode").gameObject;
        AimObject = canvasObject.transform.Find("Aim").gameObject;

        JoyStickObject = canvasObject.transform.Find("JoyStickBackground(Move)").gameObject;
        JoyStickCameraObject = canvasObject.transform.Find("JoyStickBackground(Camera)").gameObject;
        MicBtnObject = canvasObject.transform.Find("mic_btn").gameObject;
        VoiceBtnObject = canvasObject.transform.Find("voice_btn").gameObject;
        BoardTextObject = canvasObject.transform.Find("board_text").gameObject;
        BoardBtnObject = canvasObject.transform.Find("board_btn").gameObject;
        ClickBtnObject = canvasObject.transform.Find("click_btn").gameObject;
        JumpBtnObject = canvasObject.transform.Find("jump_btn").gameObject;
        EmotionPanelObject = canvasObject.transform.Find("Emotion_panel").gameObject;
        TeacherChairObject = GameObject.Find("teacher_chair").gameObject;
        // timerObject = deskModeObject.transform.Find("Timer").gameObject;
        // lessonObject = deskModeObject.transform.Find("Lesson").gameObject;
        Debug.Log(this.name,DeskModeObject);

    }

   
    #endregion

    // #region BellAlarm

    // IEnumerator CountTime() {
    
    //     while (true)
    //     {
    //         audioSource = Instantiate(playerPrefab.AddComponent<AudioSource>());
    //         audioSource.clip = sound;
    //         audioSource.playOnAwake = false;
    //         audioSource.mute = false;
    //         audioSource.loop = false;
    //         audioSource.PlayOneShot(sound);
    //         DestroyObject(audioSource, 1f);
    //         Debug.Log("1분 주기 : 벨 알람");
    //         yield return new WaitForSeconds(30.0f);
    //     }
    // }  

    // #endregion

    #region Photon Callbacks

    /// Called when the local player left the room. We need to load the launcher scene.
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("MoMainScene");
    }

    #endregion


    #region Public Methods

    // 상호작용 부분입니다
    public void OnClickBtn() // 마우스 좌클릭시
    {
        ray = fpCamera.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        //var cameraController = CamMode == 1 ? tpCamera : fpCamera;
        Debug.DrawRay(ray.origin, ray.direction, Color.blue, 0.3f);
        // 클릭 시 앞에 물건이 있을 때
        if (Physics.Raycast(ray, out hit, MaxDistance))
        {
            Debug.Log(hit.transform?.name);
            if (!ClientCanvasObject.activeInHierarchy && isTeacherDesk()) // 교탁이면
            {
                OnCreateClassCreateFaildInMobile();
            }
            GameObject tempChair = null;

            tempChair = GameObject.Find("chair" + hit.transform.name.Substring(4));
            // Debug.Log(int.Parse(hit.transform.name.Substring(4)), tempChair);

            if (isDesk())
            {
                DisplayCanvas(false, "desk");
                Vector3 newPos = new Vector3(tempChair.transform.position.x, tempChair.transform.position.y + 5f, tempChair.transform.position.z);

                PlayerManagerApp.LocalPlayerInstance.GetComponent<CharacterController>().enabled = false;
                PlayerManagerApp.LocalPlayerInstance.transform.position = newPos;
                PlayerManagerApp.LocalPlayerInstance.GetComponent<CharacterController>().enabled = true;
                DeskModeObject.SetActive(true);
                isMouseMode = true;
                fpCameraController.RotateDeskMode();
            }
        }
    }
    // 공지기능 부분입니다.
    public void OnClickNotice()
    {
        bool isBoardActive = boardPanelObject.activeInHierarchy;

        if (escPanelObject.activeInHierarchy)
        {
            return;
        }
        DisplayCanvas(isBoardActive, "board");
        isMouseMode = !isBoardActive;

        boardPanelObject.SetActive(!isBoardActive);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    public void LeaveDeskMode(){
        isMouseMode = false; 
        DeskModeObject.SetActive(false);
        DisplayCanvas(true, "desk");
    }

    public void OnTakeClass()
    {
        if (!ServerCanvasObject.activeInHierarchy) // 책상이면
        {
            Debug.Log("책");
            if (checkClassExist())
            {
                DeskModeObject.SetActive(false);
                ClientCanvasObject.SetActive(true);
                topPanelObject.SetActive(true);
            }
            else
            {
                StartCoroutineIsNotExist();
            }
        }
    }
    IEnumerator CoroutineEmotion(int i)
    {
        PlayerManagerApp.IsEmotionsActive[i] = true;

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(1f);

        PlayerManagerApp.IsEmotionsActive[i] = false;
    }

    bool IsAllEmotionInactive()
    {
        foreach (var EmotionActive in PlayerManagerApp.IsEmotionsActive)
        {
            if (EmotionActive) return false;
        }
        return true;
    }

    // 감정표현 부분입니다.
    public void OnClickEmotion1()
    {
        if (IsAllEmotionInactive())
        {
            StartCoroutine(CoroutineEmotion(0));
        }
    }
    public void OnClickEmotion2()
    {
        if (IsAllEmotionInactive())
        {
            StartCoroutine(CoroutineEmotion(1));
        }
    }
    public void OnClickEmotion3()
    {
        if (IsAllEmotionInactive())
        {
            StartCoroutine(CoroutineEmotion(2));
        }
    }
    public void OnClickEmotion4()
    {
        if (IsAllEmotionInactive())
        {
            StartCoroutine(CoroutineEmotion(3));
        }
    }
    public void OnClickEmotion5()
    {
        if (IsAllEmotionInactive())
        {
            StartCoroutine(CoroutineEmotion(4));
        }
    }
    public void OnClickEmotion6()
    {
        if (IsAllEmotionInactive())
        {
            StartCoroutine(CoroutineEmotion(5));
        }
    }
    public void OnClickEmotion7()
    {
        if (IsAllEmotionInactive())
        {
            StartCoroutine(CoroutineEmotion(6));
        }
    }
    static public void OnCreateClassCreateFaildInMobile()
    {
        Instance.StartCoroutineClassCreateFaildInMobile();
    }

    static public void OnLeaveClass()
    {
        isMouseMode = false;
        DisplayCanvas(true, "desk");

        fpCameraController.PositionNormalMode();
        ServerCanvasObject.SetActive(false);
        ClientCanvasObject.SetActive(false);
        topPanelObject.SetActive(false);
    }

    public static bool checkClassExist()
    {
        var json = new JObject();
        string method = "check_class_exist";

        json.Add("roomName", PhotonNetwork.CurrentRoom.Name);
        return Convert.ToBoolean(Utility.request_server(json, method));
    }

    public void StartCoroutineClassCreateFaildInMobile()
    {
        StartCoroutine(CoroutineClassCreateFaildInMobile());
    }
    public void StartCoroutineIsNotExist()
    {
        StartCoroutine(CoroutineIsNotExist());
    }

    static public void DisplayCanvas(bool active, string type)
    {
        if (type == "desk")
        {
            JoyStickObject.SetActive(active);
            JoyStickCameraObject.SetActive(active);
            MicBtnObject.SetActive(active);
            VoiceBtnObject.SetActive(active);
            BoardTextObject.SetActive(active);
            BoardBtnObject.SetActive(active);
            ClickBtnObject.SetActive(active);
            JumpBtnObject.SetActive(active);
            //EmotionPanelObject.SetActive(active);
            AimObject.SetActive(active);
        }
        if (type == "esc")
        {
            JoyStickObject.SetActive(active);
            JoyStickCameraObject.SetActive(active);
            MicBtnObject.SetActive(active);
            VoiceBtnObject.SetActive(active);
            BoardTextObject.SetActive(active);
            BoardBtnObject.SetActive(active);
            ClickBtnObject.SetActive(active);
            JumpBtnObject.SetActive(active);
            EmotionPanelObject.SetActive(active);
            AimObject.SetActive(active);
        }
        if (type == "board")
        {
            JoyStickObject.SetActive(active);
            JoyStickCameraObject.SetActive(active);
            MicBtnObject.SetActive(active);
            VoiceBtnObject.SetActive(active);
            //BoardTextObject.SetActive(active);
            //BoardBtnObject.SetActive(active);
            ClickBtnObject.SetActive(active);
            JumpBtnObject.SetActive(active);
            EmotionPanelObject.SetActive(active);
            AimObject.SetActive(active);
        }
    }
    #endregion


    #region Private Methods

    bool isTeacherDesk()
    {
        return hit.transform.name == "pCube4" || hit.transform.name == "TeacherDesk";
    }

    bool isDesk()
    {
        return hit.transform.name.Substring(0, 4) == "desk";
        // return (hit.transform.name.Length == 7 && hit.transform.name.Substring(0, 5) == "pCube"
        //     && 37 <= int.Parse(hit.transform.name.Substring(5, 2))
        //     && int.Parse(hit.transform.name.Substring(5, 2)) <= 56)|| hit.transform.name.Substring(0, 4) == "desk" ;
    }
    void PrintCurrentPlayerCount()
    {
        Debug.LogFormat("PhotonNetwork : Current Room Player Count : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
    }

    private string RoomName_To_JoinCode(string RoomName)
    {
        var json = new JObject();
        string method = "room_code";

        json.Add("roomName", RoomName);

        return Utility.request_server(json, method);

    }

    private IEnumerator CoroutineClassCreateFaildInMobile()
    {
        classCreateFaildInMobileObject.SetActive(true);

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(1f);

        classCreateFaildInMobileObject.SetActive(false);
    }

    private IEnumerator CoroutineIsNotExist()
    {
        isNotExistObject.SetActive(true);

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(1f);

        isNotExistObject.SetActive(false);
    }
    private IEnumerator CreateClassFailedInMobile()
    {
        yield return new WaitForSeconds(0.01f);
    }
    #endregion


    #region Photon Callbacks

    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

            PrintCurrentPlayerCount();
        }
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("Left room InstanceId : {0}", other.UserId);
            PhotonNetwork.DestroyPlayerObjects(other);
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

            PrintCurrentPlayerCount();
        }
    }
    
    #endregion
}