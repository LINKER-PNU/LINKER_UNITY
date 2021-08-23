using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

using Newtonsoft.Json.Linq;
using eggcation;

public class GameManager : MonoBehaviourPunCallbacks
{

    #region Private Fields

    [SerializeField]
    private GameObject emptyObject;
    [SerializeField]
    private GameObject canvasObject;


    [SerializeField]
    private GameObject JoinCodeTextObject;




    

    #endregion


    #region Public Fields

    static public GameObject topPanelObject;

    static public GameObject escPanelObject;

    static public GameObject boardPanelObject;

    static public GameObject createClassPanel;

    static public GameObject ServerCanvasObject;
    
    static public GameObject ClientCanvasObject;

    static public GameObject isNotExistObject;

    static public GameObject alreadyExistObject;
    
    static public GameObject DeskModeObject;

    static public GameObject AimObject;

    static public GameObject timerObject;



    public GameObject TimerBtnContentObject, TimerBtnObject, newTimerPanelObject, SubjectTimerObject, SubjectObject, TotalTimeObject, SubjectTimeObject,CreateBtnObject,DeleteBtnObject,EditBtnObject,SubjectInputObject;
    
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
    
    


    public static GameManager Instance;

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
            if (PlayerManager.LocalPlayerInstance == null)
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

        JoinCodeTextObject.SetActive(true);
        roomCode = RoomName_To_JoinCode(PhotonNetwork.CurrentRoom.Name);
        Text JoinCodeText = JoinCodeTextObject.GetComponent<Text>();
        JoinCodeText.text = roomCode;

        // Find 연산은 자원을 많이 먹으므로 Awake에서 한번 실행해줍니다.
        createClassPanel = canvasObject.transform.Find("createClass_panel").gameObject;
        topPanelObject = canvasObject.transform.Find("Top_panel").gameObject;
        escPanelObject = canvasObject.transform.Find("ESC_panel").gameObject;
        boardPanelObject = canvasObject.transform.Find("Board_panel").gameObject;
        ServerCanvasObject = emptyObject.transform.Find("ServerVideoCanvas").gameObject;
        ClientCanvasObject = emptyObject.transform.Find("ClientVideoCanvas").gameObject;
        isNotExistObject = canvasObject.transform.Find("isNotExist_text").gameObject;
        alreadyExistObject = canvasObject.transform.Find("alreadyExist_text").gameObject;
        DeskModeObject = canvasObject.transform.Find("DeskMode").gameObject;
        AimObject= canvasObject.transform.Find("Aim").gameObject;
        timerObject = canvasObject.transform.Find("Timer").gameObject;
        newTimerPanelObject = canvasObject.transform.Find("NewTimerPanel").gameObject;
        AimObject = canvasObject.transform.Find("Aim").gameObject;
        TeacherChairObject = GameObject.Find("teacher_chair").gameObject;
        Debug.Log(this.name,DeskModeObject);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        deleteTimerlist = new List<string>();


    }
    void Update(){
      if(isTimerOn){
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

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void LeaveDeskMode(){
      AimObject.SetActive(true);
      isDeskMode = false; 
      DeskModeObject.SetActive(false);
      Cursor.visible = false;
      Cursor.lockState = CursorLockMode.Locked;
      
    }

    public string FormatTime(int time){
        int hour = time/3600;
        int min = (time/60)%60;
        int sec = time%60;
      
        return string.Format("{0:00}:{1:00}:{2:00}",hour, min, sec);
    }

    public void fetchTimerList(){
      
      var json = new JObject();
      string method = "timer/list";
      
      json.Add("timerUser", Utility.userId);
      json.Add("timerRoom", PhotonNetwork.CurrentRoom.Name);

      var result = JArray.Parse(Utility.request_server(json, method));
      Debug.Log(Utility.request_server(json, method));

      foreach (Transform child in TimerBtnContentObject.transform) {
        if(child.gameObject != CreateBtnObject){
          GameObject.Destroy(child.gameObject);
        }
      }
      totalTime = 0.0f;
      foreach(JObject timer in result){
        string timerId = timer["timerId"].ToString();
        int time = int.Parse(timer["timerStudyTime"].ToString());
        string subject =  timer["timerSubject"].ToString();
        totalTime += time;
        GameObject timerBtn = Instantiate(TimerBtnObject);
        timerBtn.transform.SetParent(TimerBtnContentObject.transform);
        Text timerBtnText = timerBtn.GetComponentInChildren<Text>();
        timerBtnText.text =subject+"\n"+FormatTime(time);
        Button timerBtnComp = timerBtn.GetComponentInChildren<Button>();
        timerBtnComp.onClick.AddListener(() => {currentTime = time; currentTimerId = timerId; OnShowTimer(subject,time);});
        Toggle toggle = timerBtn.transform.Find("Toggle").GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(delegate{
          if(toggle.isOn && !deleteTimerlist.Contains(timerId)){
            deleteTimerlist.Add(timerId);
          }else if(!toggle.isOn){
            deleteTimerlist.Remove(timerId);
          }   
        });
      }

    }


    public void OnTimerMode(){
      DeskModeObject.SetActive(false);
      timerObject.SetActive(true);
      fetchTimerList();
    }

    public void OnTimerExit(){
      timerObject.SetActive(false);
      DeskModeObject.SetActive(true);
    }

    public void OnShowNewTimerPanel(){
      newTimerPanelObject.SetActive(true);
    }

    public void OnCreateTimer(){
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

    public void OnCancelCreateTimer(){
      newTimerPanelObject.SetActive(false);
    }

    public void OnDeleteTimer(){
      if(!isDeleteMode){
        DeleteBtnObject.GetComponentInChildren<Text>().text = "완료";
        isDeleteMode = true;
        foreach (Transform child in TimerBtnContentObject.transform) {
          if(child.gameObject != CreateBtnObject){
            child.gameObject.GetComponentInChildren<Button>().interactable = false;
            child.gameObject.transform.Find("Toggle").gameObject.SetActive(true);
            
          }
        } 
      }else{
        string method = "timer/remove";
        deleteTimerlist.ForEach(delegate(string id)
        {
            var json = new JObject();
            json.Add("timerId",id);
            var result = JObject.Parse(Utility.request_server(json, method));

        });
        foreach (Transform child in TimerBtnContentObject.transform) {
          if(child.gameObject != CreateBtnObject){
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
    public void OnEditTimer(){
      if(!isEditMode){
        EditBtnObject.GetComponentInChildren<Text>().text = "완료";
        isEditMode = true;
        SubjectInputObject.SetActive(true);
        SubjectInputObject.GetComponent<InputField>().text = SubjectObject.GetComponent<Text>().text;
      }else{
        newSubjectName = SubjectInputObject.GetComponent<InputField>().text;
        string method = "timer/edit";
        var json = new JObject();
        json.Add("timerId",currentTimerId);
        json.Add("timerSubject",newSubjectName);
        var result = JObject.Parse(Utility.request_server(json, method));
        
        SubjectObject.GetComponent<Text>().text = newSubjectName;
        
        SubjectInputObject.SetActive(false);
        EditBtnObject.GetComponentInChildren<Text>().text = "수정";
        isEditMode = false;
        newSubjectName = "";
        SubjectInputObject.GetComponent<InputField>().text="";
      }
    }

    // 과목 타이머 보여줌
    public void OnShowTimer(string timerTitle,int time){
      Debug.Log("show timer"+timerTitle);
      SubjectTimerObject.SetActive(true);
      SubjectObject.GetComponent<Text>().text = timerTitle;
      TotalTimeObject.GetComponent<Text>().text = FormatTime(Mathf.FloorToInt(totalTime));
      SubjectTimeObject.GetComponent<Text>().text = FormatTime(time);
    }
    // 과목 타이머에서 나옴
    public void LeaveTimer(){
      SubjectTimerObject.SetActive(false);
      fetchTimerList();
    }

    public void resumeTimer(){
      isTimerOn = true;
    }

    public void stopTimer(){
      isTimerOn = false;
      var json = new JObject();
      string method = "timer/stop";
      json.Add("timerId", currentTimerId);
      json.Add("timerStudyTime",(Mathf.FloorToInt(currentTime)).ToString());
      var result = JObject.Parse(Utility.request_server(json, method));
      Debug.Log("timer 저장"+currentTimerId+": " +(Mathf.FloorToInt(currentTime)).ToString());
    }

    


    #endregion


    #region Private Methods

    void PrintCurrentPlayerCount()
    public void LeaveDeskMode(){
        isMouseMode = false; 
        DeskModeObject.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        AimObject.SetActive(true);
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
    public void OnCreateClassConfirm()
    {
        Debug.Log("교");
        if (checkClassExist())
        {
            Instance.StartCoroutineAlreadyExist();
            createClassPanel.SetActive(false);
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            isMouseMode = true;
            AimObject.SetActive(false);
            if (PlayerManager.LocalPlayerInstance != null)
            {
                Vector3 newPos = new Vector3(TeacherChairObject.transform.position.x,
                                                TeacherChairObject.transform.position.y + 5f,
                                                TeacherChairObject.transform.position.z);
                PlayerManager.LocalPlayerInstance.GetComponent<CharacterController>().enabled = false;
                PlayerManager.LocalPlayerInstance.transform.position = newPos;
                PlayerManager.LocalPlayerInstance.GetComponent<CharacterController>().enabled = true;
                if (PlayerManager.LocalPlayerInstance.GetComponent< PlayerManager>().CamMode == 1)
                {
                    PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager>().CamMode = 0;
                }
                PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager>().fpCameraController.PositionTeacherDeskMode();
            }

            ServerCanvasObject.SetActive(true);
            createClassPanel.SetActive(false);
            topPanelObject.SetActive(true);
        }
    }
    public void OnCreateClassCancle()
    {
        isMouseMode = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        AimObject.SetActive(true);

        createClassPanel.SetActive(false);
    }

    static public void OnLeaveClass()
    {
        isMouseMode = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        AimObject.SetActive(true);

        PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager>().fpCameraController.PositionNormalMode();
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

    public void StartCoroutineAlreadyExist()
    {
        StartCoroutine(CoroutineAlreadyExist());
    }
    public void StartCoroutineIsNotExist()
    {
        StartCoroutine(CoroutineIsNotExist());
    }
    #endregion


    #region Private Methods

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

    private IEnumerator CoroutineAlreadyExist()
    {
        alreadyExistObject.SetActive(true);

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(1f);

        alreadyExistObject.SetActive(false);
    }

    private IEnumerator CoroutineIsNotExist()
    {
        isNotExistObject.SetActive(true);

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(1f);

        isNotExistObject.SetActive(false);
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