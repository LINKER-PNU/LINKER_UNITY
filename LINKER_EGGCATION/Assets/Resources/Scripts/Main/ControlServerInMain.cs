using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class ControlServerInMain : MonoBehaviourPunCallbacks
{
    #region Private Serializable Fields

    /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
    [SerializeField]
    private byte maxPlayersPerRoom = 20;

    [SerializeField]
    private GameObject MainObject;

    #endregion


    #region Private Fields

    /// This client's version number.
    /// Users are separated from each other by gameVersion
    string gameVersion = "1";

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
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.CreateRoom("linker", new RoomOptions { MaxPlayers = maxPlayersPerRoom }))
            {
                Debug.Log("CreateRoom is Success");
            }
            //if (PhotonNetwork.JoinRoom("linker"))
            //{
            //    Debug.Log("JoinRoom is Success");
            //}

        }
        else
        {
            Debug.LogError("PhotonNetwork is not connected!");

            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    public void JoinLinkerRoomBttn()
    {
        Debug.Log("Click Join Room");
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.JoinRoom("linker"))
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
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
        // #Critical: We only load if we are the first player, else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene.
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 1)
        {
                Debug.Log("We load the 'ClassScene' ");

                PhotonNetwork.LoadLevel("MoClassScene");
        }
    }

    #endregion
}
