using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

using Newtonsoft.Json.Linq;

using eggcation;
public class ControlServerInMain : MonoBehaviourPunCallbacks
{
    #region Private Serializable Fields

    /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
    [SerializeField]
    private byte maxPlayersPerRoom = 20;

    [SerializeField]
    private GameObject JoinCodeInputObject, MainObject, RoomNamePanelObject, RoomNameInputObject, CreateRoomFailObject,NameObject, RoleObject,EggObject;

    private GameObject ColorObject;
    
    private Text userNameText;
    private Text userRoleText;

    

    #endregion


    #region Private Fields

    /// This client's version number.
    /// Users are separated from each other by gameVersion
    string gameVersion = "1";

    #endregion

    #region Public Fields

    public GameObject ClassBttnContentObject, ClassBttnObject;

    [SerializeField]
    public int y_offset = 220;

    [SerializeField]
    public int offset = 150;

    static public string joinCode = string.Empty;
    #endregion


    #region MonoBehaviour Callbacks
    // Start is called before the first frame update
    void Start()
    {
        // #Critical, we must first and foremost connect to Photon Online Server.
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
        
        get_user_info();
    }

    // Update is called once per frame
    void Update()
    {

    }

    #endregion


    #region Public Methods
    public void OnClickColorBttn()
    {
      ColorObject = EventSystem.current.currentSelectedGameObject;
      Color newColor = ColorObject.GetComponentInChildren<Image>().color;

      Debug.Log(newColor);
      Debug.Log("parsed "+ColorUtility.ToHtmlStringRGBA(newColor));

      EggObject.GetComponent<Renderer>().material.color = newColor;

      var json = new JObject();
      string method = "skin";

      json.Add("userId", Utility.userId);
      json.Add("skinColor", ColorUtility.ToHtmlStringRGBA(newColor));
      json.Add("skinRole", "S");

      var result = JObject.Parse(Utility.request_server(json, method));
      // Debug.Log(result);
    }

    public void CreateNewRoomBttn()
    {
        Debug.Log("Click Create New Room");
        RoomNamePanelObject.SetActive(true);
    }

    public void CreateNewRoomConfirmBttn()
    {
        Utility.roomName = RoomNameInputObject.GetComponent<InputField>().text;

        if (!IsRoomExist(Utility.roomName))
        {
            if (PhotonNetwork.IsConnected)
            {
                if (PhotonNetwork.CreateRoom(Utility.roomName, new RoomOptions { MaxPlayers = maxPlayersPerRoom }))
                {
                    Debug.Log("CreateRoom is Success");
                } 
                else
                {
                    // 수정해야하는부분 ㅠㅠ
                    CreateRoomFailObject.SetActive(true);
                    //System.Threading.Thread.Sleep(1000);
                    CreateRoomFailObject.SetActive(false);
                }

            }
            else
            {
                Debug.LogError("PhotonNetwork is not connected!");

                PhotonNetwork.GameVersion = gameVersion;
                PhotonNetwork.ConnectUsingSettings();
            }
        } else
        {
            CreateRoomFailObject.SetActive(true);
            System.Threading.Thread.Sleep(1000);
            CreateRoomFailObject.SetActive(false);
        }
    }
    public void CreateNewRoomCancelBttn()
    {
        RoomNamePanelObject.SetActive(false);
    }

    public void JoinRoomByCodeBttn()
    {
        Debug.Log("Click Join Room");

        InputField joinCode = JoinCodeInputObject.GetComponent<InputField>();

        Utility.roomName = JoinCode_To_RoomName(joinCode.text);
        if (Utility.roomName != string.Empty)
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinOrCreateRoom(Utility.roomName, new RoomOptions { MaxPlayers = maxPlayersPerRoom }, TypedLobby.Default);
            }
            else
            {
                Debug.LogError("PhotonNetwork is not connected!");
                // #Critical, we must first and foremost connect to Photon Online Server.
                PhotonNetwork.GameVersion = gameVersion;
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        else
        {
            Debug.LogError("Invalid Join Code!");
        }

    }

    #endregion

    #region Private Methods

    private void get_user_info()
    {
        var json = new JObject();
        string method = "user";

        json.Add("userId", Utility.userId);

        var user_info = JObject.Parse(Utility.request_server(json, method));

        userNameText = NameObject.GetComponent<Text>(); 
        userRoleText = RoleObject.GetComponent<Text>(); 
        
        userNameText.text = Utility.displayName; 
        if(user_info["user_skin_role"].ToString() == "S"){
          userRoleText.text = "학생";
        }else{
          userRoleText.text = "선생";
        }
        Color myColor;
        ColorUtility.TryParseHtmlString("#"+user_info["user_skin_color"].ToString(), out myColor);
        Debug.Log("userskin" + user_info["user_skin_color"].ToString());
        Debug.Log(myColor);
        EggObject.GetComponent<Renderer>().material.color = myColor;
        
        
      
        createClassBttn(user_info["user_room"]);
    }

    private void createClassBttn(JToken room_list)
    {
        int count = 0;
        foreach (JToken room in room_list)
        {
            // 버튼 추가
            GameObject classBttn = Instantiate(ClassBttnObject);
            classBttn.transform.SetParent(ClassBttnContentObject.transform);
            Text classBttnText = classBttn.GetComponentInChildren<Text>();
            classBttnText.text = room["room_name"].ToString() + " ●" + "(" +
                room["room_present"].ToString() + " / " +
                room["room_max"].ToString() + ")";
            RectTransform classBttnTranform = classBttn.GetComponent<RectTransform>();
            Vector3 classBttnNewPosition = new Vector3(classBttnTranform.position.x, classBttnTranform.position.y - count * y_offset, classBttnTranform.position.z);
            classBttnTranform.localPosition = classBttnNewPosition;
            classBttnTranform.localScale = new Vector3(1, 1, 1);
            Debug.Log(classBttnTranform.localScale);
            count += 1;

            // onClick 추가
            Button classBttnComp = classBttn.GetComponent<Button>();
            classBttnComp.onClick.AddListener(() => JoinRoomByRoomName(room["room_name"].ToString()));

            Debug.LogFormat("방 목록에 추가 : {0}", room["room_name"].ToString());
        }
        RectTransform ContentTransform = ClassBttnContentObject.GetComponent<RectTransform>();
        ContentTransform.sizeDelta = new Vector2(0, offset * 2 + y_offset * (count - 1));
    }

    private void JoinRoomByRoomName(string roomName)
    {
        PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions { MaxPlayers = maxPlayersPerRoom }, TypedLobby.Default);
    }

    private string JoinCode_To_RoomName(string joinCode)
    {
        var json = new JObject();
        string method = "auth_room";

        json.Add("joinCode", joinCode);

        Utility.roomName = Utility.request_server(json, method);
        return Utility.roomName;
    }

    private bool IsRoomExist(string roomName)
    {
        var json = new JObject();
        string method = "room_exist";

        json.Add("roomName", roomName);

        return Convert.ToBoolean(Utility.request_server(json, method));
    }
    #endregion


    #region MonoBehaviourPunCallbacks Callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRoomFailed() was called by PUN.");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        //PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogFormat("방생성 실패: {0} {1}", returnCode, message);
    }

    public override void OnJoinedRoom()
    {

        Debug.Log("JoinRoom is Success");
        // #Critical: We only load if we are the first player, else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene.
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 1)
        {
                Debug.Log("We load the 'ClassScene' ");

                PhotonNetwork.LoadLevel("MoClassScene");
        }
    }

    #endregion
}
