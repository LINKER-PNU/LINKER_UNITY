using System;
using System.Collections;
using System.Net;
using System.IO;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    static public GameObject classCreateFaildInMobileObject;
    
    static public GameObject DeskModeObject;

    static public GameObject AimObject;

    static public GameObject TeacherChairObject;

    static public bool isMouseMode = false;
    
    


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

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    public void LeaveDeskMode(){
        isMouseMode = false; 
        DeskModeObject.SetActive(false);
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
    static public void OnCreateClassCreateFaildInMobile()
    {
        Instance.StartCoroutineClassCreateFaildInMobile();
    }
    public void OnCreateClassCancle()
    {
        isMouseMode = false;
        AimObject.SetActive(true);

        createClassPanel.SetActive(false);
    }

    static public void OnLeaveClass()
    {
        isMouseMode = false;
        AimObject.SetActive(true);

        PlayerManagerApp.LocalPlayerInstance.GetComponent<PlayerManagerApp>().fpCameraController.PositionNormalMode();
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