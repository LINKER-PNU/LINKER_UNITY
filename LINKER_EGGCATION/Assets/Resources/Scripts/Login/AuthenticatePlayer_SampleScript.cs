using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Photon.Pun;

public class AuthenticatePlayer_SampleScript : MonoBehaviour
{
    static public string gameSparkUserId = "N";

    public InputField userNameInput, passwordInput;
    public GameObject LoginObject, RegisterObject, MainObject;
 
 
    // 계정이름과 비밀번호로 로그인
    public void AuthorizePlayerBttn()
    {
        new GameSparks.Api.Requests.AuthenticationRequest()
            .SetUserName(userNameInput.text)
            .SetPassword(passwordInput.text)
            .Send((response) => {
                if (!response.HasErrors)
                {
                    Debug.Log("로그인 성공...");
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
                        Debug.Log(result);
                    }
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