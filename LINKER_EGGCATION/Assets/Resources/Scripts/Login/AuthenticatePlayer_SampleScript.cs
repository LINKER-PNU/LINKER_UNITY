using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System;

using Newtonsoft.Json.Linq;
using Photon.Pun;
using Photon.Realtime;

using eggcation;

public class AuthenticatePlayer_SampleScript : MonoBehaviour
{
    // static public string gameSparkUserId = "N";

    public InputField userNameInput, passwordInput;
    public GameObject LoginObject, RegisterObject, ErrorObject;

    private void Awake()
    {
        Application.targetFrameRate = 40;
    }

    public void Update()
    {
      if(userNameInput.isFocused == true) {
        if(Input.GetKeyDown(KeyCode.Tab)) {
            passwordInput.Select();
        }
      }
      if (Input.GetKeyUp(KeyCode.Return)) { AuthorizePlayerBttn(); }
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

                    var json = new JObject();
                    

                    string authToken = response.AuthToken;
                    Utility.displayName = response.DisplayName;
                    bool? newPlayer = response.NewPlayer;
                    // gameSparkUserId = response.UserId;
                    Utility.userId = response.UserId;

                    PhotonNetwork.LocalPlayer.NickName = Utility.displayName; 
                    PhotonNetwork.AuthValues = new Photon.Realtime.AuthenticationValues(Utility.userId);

                    json.Add("authToken", authToken);
                    json.Add("displayName", Utility.displayName);
                    json.Add("newPlayer", newPlayer);
                    json.Add("userId", Utility.userId);

                    Utility.request_server(json, "login");
                    SceneManager.LoadScene("MoMainScene");
                }
                else
                {
                    StartCoroutine(Error());
                }
            });
    }
    IEnumerator Error(){
      ErrorObject.SetActive(true);
      yield return new WaitForSeconds(2f);
      ErrorObject.SetActive(false);
    }
}    
 
