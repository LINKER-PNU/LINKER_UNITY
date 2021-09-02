using System;
using System.Collections;
using System.Collections.Generic;
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
    private GameObject soundObject;

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

    static public GameObject timerObject;

    public GameObject TimerBtnContentObject, TimerBtnObject, newTimerPanelObject, SubjectTimerObject, SubjectObject, TotalTimeObject, SubjectTimeObject, CreateBtnObject, DeleteBtnObject, EditBtnObject, SubjectInputObject;

       
    static public GameObject micOnObject;
    static public GameObject micOffObject;
    static public GameObject headsetOnObject;
    static public GameObject headsetOffObject;


    static public bool isDeskMode = false;


    [SerializeField]
    public int y_offset = 320;

    [SerializeField]
    public int offset = 150;

    static public string roomCode;

    public List<string> deleteTimerlist;
    public float totalTime = 0.0f;
    public string currentTimerId;
    public float currentTime;

    public bool isTimerOn = false;
    public bool isDeleteMode = false;
    public bool isEditMode = false;
    string newSubjectName = "";


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
        topPanelObject = canvasObject.transform.Find("Top_panel").gameObject;
        escPanelObject = canvasObject.transform.Find("ESC_panel").gameObject;
        boardPanelObject = canvasObject.transform.Find("Board_panel").gameObject;
        ServerCanvasObject = emptyObject.transform.Find("ServerVideoCanvas").gameObject;
        ClientCanvasObject = emptyObject.transform.Find("ClientVideoCanvas").gameObject;
        isNotExistObject = canvasObject.transform.Find("isNotExist_text").gameObject;
        classCreateFaildInMobileObject = canvasObject.transform.Find("classCreateFaildInMobile_text").gameObject;
        DeskModeObject = canvasObject.transform.Find("DeskMode").gameObject;
        AimObject = canvasObject.transform.Find("Aim").gameObject;

        timerObject = canvasObject.transform.Find("Timer").gameObject;
        newTimerPanelObject = canvasObject.transform.Find("NewTimerPanel").gameObject;
        Debug.Log(this.name, DeskModeObject);
        deleteTimerlist = new List<string>();
        micOnObject = canvasObject.transform.Find("MicOn").gameObject;
        micOffObject = canvasObject.transform.Find("MicOff").gameObject;
        headsetOnObject = canvasObject.transform.Find("HeadsetOn").gameObject;
        headsetOffObject = canvasObject.transform.Find("HeadsetOff").gameObject;

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

    void Update()
    {
        if (isTimerOn)
        {
            currentTime += Time.deltaTime;
            totalTime += Time.deltaTime;
            TotalTimeObject.GetComponent<Text>().text = FormatTime(Mathf.FloorToInt(totalTime));
            SubjectTimeObject.GetComponent<Text>().text = FormatTime(Mathf.FloorToInt(currentTime));
            Debug.Log(currentTime);
        }
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

    public string FormatTime(int time)
    {
        int hour = time / 3600;
        int min = (time / 60) % 60;
        int sec = time % 60;

        return string.Format("{0:00}:{1:00}:{2:00}", hour, min, sec);
    }

    public void fetchTimerList()
    {

        var json = new JObject();
        string method = "timer/list";

        json.Add("timerUser", Utility.userId);
        json.Add("timerRoom", PhotonNetwork.CurrentRoom.Name);

        var result = JArray.Parse(Utility.request_server(json, method));
        Debug.Log(Utility.request_server(json, method));

        foreach (Transform child in TimerBtnContentObject.transform)
        {
            if (child.gameObject != CreateBtnObject)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        totalTime = 0.0f;
        foreach (JObject timer in result)
        {
            string timerId = timer["timerId"].ToString();
            int time = int.Parse(timer["timerStudyTime"].ToString());
            string subject = timer["timerSubject"].ToString();
            totalTime += time;
            GameObject timerBtn = Instantiate(TimerBtnObject);
            timerBtn.transform.SetParent(TimerBtnContentObject.transform);
            Text timerBtnText = timerBtn.GetComponentInChildren<Text>();
            timerBtnText.text = subject + "\n" + FormatTime(time);
            Button timerBtnComp = timerBtn.GetComponentInChildren<Button>();
            timerBtnComp.onClick.AddListener(() => { currentTime = time; currentTimerId = timerId; OnShowTimer(subject, time); });
            Toggle toggle = timerBtn.transform.Find("Toggle").GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(delegate {
                if (toggle.isOn && !deleteTimerlist.Contains(timerId))
                {
                    deleteTimerlist.Add(timerId);
                }
                else if (!toggle.isOn)
                {
                    deleteTimerlist.Remove(timerId);
                }
            });
        }

    }

    public void OnTimerMode()
    {
        DeskModeObject.SetActive(false);
        timerObject.SetActive(true);
        fetchTimerList();
        DisplayCanvas(false, "timer");
    }

    public void OnTimerExit()
    {
        timerObject.SetActive(false);
        DeskModeObject.SetActive(true);
        DisplayCanvas(true, "timer");
    }

    public void OnShowNewTimerPanel()
    {
        newTimerPanelObject.SetActive(true);
    }

    public void OnCreateTimer()
    {
        string newTimerTitle = newTimerPanelObject.GetComponentInChildren<InputField>().text;
        var json = new JObject();
        string method = "timer/add";

        json.Add("timerUser", Utility.userId);
        json.Add("timerRoom", PhotonNetwork.CurrentRoom.Name);
        json.Add("timerSubject", newTimerTitle);

        var result = JObject.Parse(Utility.request_server(json, method));
        fetchTimerList();
        newTimerPanelObject.SetActive(false);

    }

    public void OnCancelCreateTimer()
    {
        newTimerPanelObject.SetActive(false);
    }

    public void OnDeleteTimer()
    {
        if (!isDeleteMode)
        {
            DeleteBtnObject.GetComponentInChildren<Text>().text = "완료";
            isDeleteMode = true;
            foreach (Transform child in TimerBtnContentObject.transform)
            {
                if (child.gameObject != CreateBtnObject)
                {
                    child.gameObject.GetComponentInChildren<Button>().interactable = false;
                    child.gameObject.transform.Find("Toggle").gameObject.SetActive(true);

                }
            }
        }
        else
        {
            string method = "timer/remove";
            deleteTimerlist.ForEach(delegate (string id)
            {
                var json = new JObject();
                json.Add("timerId", id);
                var result = JObject.Parse(Utility.request_server(json, method));

            });
            foreach (Transform child in TimerBtnContentObject.transform)
            {
                if (child.gameObject != CreateBtnObject)
                {
                    child.gameObject.GetComponentInChildren<Button>().interactable = true;
                    child.gameObject.transform.Find("Toggle").gameObject.SetActive(false);
                    child.gameObject.transform.Find("Toggle").GetComponent<Toggle>().isOn = false;
                }
            }
            fetchTimerList();
            DeleteBtnObject.GetComponentInChildren<Text>().text = "삭제";
            isDeleteMode = false;
            deleteTimerlist.Clear();

        }
    }
    public void OnEditTimer()
    {
        if (!isEditMode)
        {
            EditBtnObject.GetComponentInChildren<Text>().text = "완료";
            isEditMode = true;
            SubjectInputObject.SetActive(true);
            SubjectInputObject.GetComponent<InputField>().text = SubjectObject.GetComponent<Text>().text;
        }
        else
        {
            newSubjectName = SubjectInputObject.GetComponent<InputField>().text;
            string method = "timer/edit";
            var json = new JObject();
            json.Add("timerId", currentTimerId);
            json.Add("timerSubject", newSubjectName);
            var result = JObject.Parse(Utility.request_server(json, method));

            SubjectObject.GetComponent<Text>().text = newSubjectName;

            SubjectInputObject.SetActive(false);
            EditBtnObject.GetComponentInChildren<Text>().text = "수정";
            isEditMode = false;
            newSubjectName = "";
            SubjectInputObject.GetComponent<InputField>().text = "";
        }
    }

    // 과목 타이머 보여줌
    public void OnShowTimer(string timerTitle, int time)
    {
        Debug.Log("show timer" + timerTitle);
        SubjectTimerObject.SetActive(true);
        SubjectObject.GetComponent<Text>().text = timerTitle;
        TotalTimeObject.GetComponent<Text>().text = FormatTime(Mathf.FloorToInt(totalTime));
        SubjectTimeObject.GetComponent<Text>().text = FormatTime(time);
    }
    // 과목 타이머에서 나옴
    public void LeaveTimer()
    {
        SubjectTimerObject.SetActive(false);
        fetchTimerList();
    }

    public void resumeTimer()
    {
        isTimerOn = true;
    }

    public void stopTimer()
    {
        isTimerOn = false;
        var json = new JObject();
        string method = "timer/stop";
        json.Add("timerId", currentTimerId);
        json.Add("timerStudyTime", (Mathf.FloorToInt(currentTime)).ToString());
        var result = JObject.Parse(Utility.request_server(json, method));
        Debug.Log("timer 저장" + currentTimerId + ": " + (Mathf.FloorToInt(currentTime)).ToString());
    }

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
    // 마이크, 보이스 부분입니다.

    public void OnClickListen()
    {
        if (AudioListener.volume == 0)
        {
            headsetOnObject.SetActive(true);
            headsetOffObject.SetActive(false);
            AudioListener.volume = 1;
        }
        else
        {
            headsetOnObject.SetActive(false);
            headsetOffObject.SetActive(true);
            AudioListener.volume = 0;
        }
    }
    public void OnClickSpeak()
    {
        if (PlayerManagerApp.VoiceRecorder.TransmitEnabled == true)
        {
            PlayerManagerApp.VoiceRecorder.TransmitEnabled = false;
            PlayerManagerApp.VoiceAudioSource.volume = 0;
            micOnObject.SetActive(false);
            micOffObject.SetActive(true);
        }
        else
        {
            PlayerManagerApp.VoiceRecorder.TransmitEnabled = true;
            PlayerManagerApp.VoiceAudioSource.volume = 1;
            micOnObject.SetActive(true);
            micOffObject.SetActive(false);
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

    // 감정표현 부분입니다.
    public void OnClickEmotion1()
    {
        if (PlayerManagerApp.LocalPlayerInstance.GetComponent<PlayerManagerApp>().IsAllEmotionInactive())
        {
            StartCoroutine(PlayerManagerApp.LocalPlayerInstance.GetComponent<PlayerManagerApp>().CoroutineEmotion(0));
        }
    }
    public void OnClickEmotion2()
    {
        if (PlayerManagerApp.LocalPlayerInstance.GetComponent<PlayerManagerApp>().IsAllEmotionInactive())
        {
            StartCoroutine(PlayerManagerApp.LocalPlayerInstance.GetComponent<PlayerManagerApp>().CoroutineEmotion(1));
        }
    }
    public void OnClickEmotion3()
    {
        if (PlayerManagerApp.LocalPlayerInstance.GetComponent<PlayerManagerApp>().IsAllEmotionInactive())
        {
            StartCoroutine(PlayerManagerApp.LocalPlayerInstance.GetComponent<PlayerManagerApp>().CoroutineEmotion(2));
        }
    }
    public void OnClickEmotion4()
    {
        if (PlayerManagerApp.LocalPlayerInstance.GetComponent<PlayerManagerApp>().IsAllEmotionInactive())
        {
            StartCoroutine(PlayerManagerApp.LocalPlayerInstance.GetComponent<PlayerManagerApp>().CoroutineEmotion(3));
        }
    }
    public void OnClickEmotion5()
    {
        if (PlayerManagerApp.LocalPlayerInstance.GetComponent<PlayerManagerApp>().IsAllEmotionInactive())
        {
            StartCoroutine(PlayerManagerApp.LocalPlayerInstance.GetComponent<PlayerManagerApp>().CoroutineEmotion(4));
        }
    }
    public void OnClickEmotion6()
    {
        if (PlayerManagerApp.LocalPlayerInstance.GetComponent<PlayerManagerApp>().IsAllEmotionInactive())
        {
            StartCoroutine(PlayerManagerApp.LocalPlayerInstance.GetComponent<PlayerManagerApp>().CoroutineEmotion(5));
        }
    }
    public void OnClickEmotion7()
    {
        if (PlayerManagerApp.LocalPlayerInstance.GetComponent<PlayerManagerApp>().IsAllEmotionInactive())
        {
            StartCoroutine(PlayerManagerApp.LocalPlayerInstance.GetComponent<PlayerManagerApp>().CoroutineEmotion(6));
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
        else if (type == "esc")
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
        else if (type == "board")
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
        else if (type == "timer")
        {
            EmotionPanelObject.SetActive(active);
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