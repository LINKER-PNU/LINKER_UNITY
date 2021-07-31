using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.IO;
using System;

using Newtonsoft.Json.Linq;
using Photon.Pun;
using Photon.Realtime;

public class AuthenticatePlayer_SampleScript : MonoBehaviour
{
    static public string gameSparkUserId = "N";

    public InputField userNameInput, passwordInput;
    public GameObject LoginObject, RegisterObject, MainObject, ClassBttnContentObject, ClassBttnObject;

    [SerializeField]
    public int y_offset = 220;

    [SerializeField]
    public int offset = 150;

    private byte maxPlayersPerRoom = 20;

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
    private bool IsRoomExist(string roomName)
    {
        var json = new JObject();
        string method = "room_exist";

        json.Add("roomName", roomName);

        return Convert.ToBoolean(request_server(json, method));
    }

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

    // 계정이름과 비밀번호로 로그인
    public void AuthorizePlayerBttn()
    {
        new GameSparks.Api.Requests.AuthenticationRequest()
            .SetUserName(userNameInput.text)
            .SetPassword(passwordInput.text)
            .Send((response) => {
                if (!response.HasErrors)
                {
                    LoginObject.SetActive(false);
                    RegisterObject.SetActive(false);
                    MainObject.SetActive(true);

                    var json = new JObject();

                    string url = "http://34.64.85.29:8080/login";

                    string authToken = response.AuthToken;
                    string displayName = response.DisplayName;
                    bool? newPlayer = response.NewPlayer;
                    // GSData scriptData = response.ScriptData;
                    // AuthenticationResponse._Player switchSummary = response.SwitchSummary;
                    gameSparkUserId = response.UserId;

                    PhotonNetwork.LocalPlayer.NickName = displayName; 
                    PhotonNetwork.AuthValues = new Photon.Realtime.AuthenticationValues(gameSparkUserId);

                    json.Add("authToken", authToken);
                    json.Add("displayName", displayName);
                    json.Add("newPlayer", newPlayer);
                    json.Add("userId", gameSparkUserId);

                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";

                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        streamWriter.Write(json.ToString());
                    }

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        var user_room = JObject.Parse(result);
                        createClassBttn(user_room["user_room"]);
                        //Debug.Log(user_room["user_room"]);
                    }
                    Debug.Log("로그인 성공...");
                }
                else
                {
                    Debug.Log("로그인 실패..." + response.Errors.JSON.ToString());
                }
            });
    }
 
    // DisplayName 으로 로그인
    //public void AuthenticateDeviceBttn()
    //{
    //    new GameSparks.Api.Requests.DeviceAuthenticationRequest()
    //        .SetDisplayName(displayNameInput.text)
    //        .Send((response) => {
    //            if (!response.HasErrors)
    //            {
    //                Debug.Log("Device 로그인 성공...");
    //            }
    //            else
    //            {
    //                Debug.Log("Device 로그인 실패..." + response.Errors.JSON.ToString());
    //            }
    //        });
    //}
}