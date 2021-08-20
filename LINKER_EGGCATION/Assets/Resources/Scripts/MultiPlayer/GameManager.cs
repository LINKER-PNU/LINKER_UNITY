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

public class GameManager : MonoBehaviourPunCallbacks
{

    #region Private Fields

    [SerializeField]
    private GameObject emptyObject;
    [SerializeField]
    private GameObject canvasObject;

    [SerializeField]
    private GameObject topPanelObject;

    [SerializeField]
    private GameObject JoinCodeTextObject;

    

    #endregion


    #region Public Fields

    static public GameObject createClassPanel;

    static public GameObject leaveRoomBtn;

    static public GameObject leaveClassBtn;

    static public GameObject ServerCanvasObject;
    
    static public GameObject ClientCanvasObject;

    static public GameObject isNotExistObject;

    static public GameObject alreadyExistObject;
    
    static public GameObject DeskModeObject;
    
    static public bool isDeskMode = false;
    
    


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
        string joinCode = RoomName_To_JoinCode(PhotonNetwork.CurrentRoom.Name);
        Text JoinCodeText = JoinCodeTextObject.GetComponent<Text>();
        JoinCodeText.text = joinCode;

        // Find 연산은 자원을 많이 먹으므로 Awake에서 한번 실행해줍니다.
        createClassPanel = canvasObject.transform.Find("createClass_panel").gameObject;
        leaveRoomBtn = topPanelObject.transform.Find("leaveRoom_btn").gameObject;
        leaveClassBtn = topPanelObject.transform.Find("leaveClass_btn").gameObject;
        ServerCanvasObject = emptyObject.transform.Find("ServerVideoCanvas").gameObject;
        ClientCanvasObject = emptyObject.transform.Find("ClientVideoCanvas").gameObject;
        isNotExistObject = canvasObject.transform.Find("isNotExist_text").gameObject;
        alreadyExistObject = canvasObject.transform.Find("alreadyExist_text").gameObject;
        Debug.Log(ServerCanvasObject.name);
        DeskModeObject = canvasObject.transform.Find("DeskMode").gameObject;
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
    public void onDeskMode(){
      isDeskMode = false;
      DeskModeObject.SetActive(false);
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