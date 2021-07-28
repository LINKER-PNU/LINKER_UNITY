using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;

using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

using Newtonsoft.Json.Linq;
public class ControlServerInMain : MonoBehaviourPunCallbacks
{
    #region Private Serializable Fields

    /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
    [SerializeField]
    private byte maxPlayersPerRoom = 20;

    [SerializeField]
    private GameObject JoinCodeInputObject, MainObject, RoomNamePanelObject, RoomNameInputObject, CreateRoomFailObject;

    #endregion


    #region Private Fields

    /// This client's version number.
    /// Users are separated from each other by gameVersion
    string gameVersion = "1";

    #endregion

    #region Public Fields
    static public string roomName = string.Empty, joinCode = string.Empty;
    #endregion


    #region MonoBehaviour Callbacks
    // Start is called before the first frame update
    void Start()
    {
        // #Critical, we must first and foremost connect to Photon Online Server.
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    // Update is called once per frame
    void Update()
    {

    }

    #endregion


    #region Public Methods
    public void CreateNewRoomBttn()
    {
        Debug.Log("Click Create New Room");
        RoomNamePanelObject.SetActive(true);
    }

    public void CreateNewRoomConfirmBttn()
    {
        roomName = RoomNameInputObject.GetComponent<InputField>().text;
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = maxPlayersPerRoom }))
            {
                Debug.Log("CreateRoom is Success");
            } 
            else
            {
                CreateRoomFailObject.SetActive(true);
                System.Threading.Thread.Sleep(1000);
                CreateRoomFailObject.SetActive(false);
            }

        }
        else
        {
            Debug.LogError("PhotonNetwork is not connected!");

            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    public void CreateNewRoomCancelBttn()
    {
        RoomNamePanelObject.SetActive(false);
    }
    public void JoinLinkerRoomBttn()
    {
        Debug.Log("Click Join Room");
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.JoinRoom("linker2"))
            {
                Debug.Log("JoinRoom is Success");
            }
        }
        else
        {
            Debug.LogError("PhotonNetwork is not connected!");
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void JoinRoomByCodeBttn()
    {
        Debug.Log("Click Join Room");

        InputField joinCode = JoinCodeInputObject.GetComponent<InputField>();

        roomName = JoinCode_To_RoomName(joinCode.text);
        if (roomName != string.Empty)
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinRoom(roomName);
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

    private string request_server(JObject req, string method)
    {
        string url = "http://34.64.85.29:8080/";
        var httpWebRequest = (HttpWebRequest)WebRequest.Create(url + method);
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            streamWriter.Write(req.ToString());
        }

        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        string characterSet = httpResponse.CharacterSet;
        Debug.Log(characterSet);
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream(), System.Text.Encoding.UTF8, true))
        {
            var result = streamReader.ReadToEnd();
            Debug.Log(result);
            return result;
        }
    }
    private string JoinCode_To_RoomName(string joinCode)
    {
        var json = new JObject();
        string method = "auth_room";

        json.Add("joinCode", joinCode);

        roomName = request_server(json, method);
        return roomName;

    }
    #endregion


    #region MonoBehaviourPunCallbacks Callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
        //// #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()

        //if (isConnecting)
        //{
        //    PhotonNetwork.JoinRandomRoom();
        //}
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
