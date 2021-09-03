using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using GameSparks.Api;
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using eggcation;

public class RegisterPlayer_SampleScript : MonoBehaviour
{
    public GameObject LoginObject, RegisterObject, ErrorObject;
    public InputField displayNameInput, userNameInput, passwordInput;
    public Toggle teacherToggle, studentToggle;
    

    

    public void Update()
    {
      if(displayNameInput.isFocused == true) {
        if(Input.GetKeyDown(KeyCode.Tab)) {
            userNameInput.Select();
        }
      }
      else if(userNameInput.isFocused == true) {
        if(Input.GetKeyDown(KeyCode.Tab)) {
            passwordInput.Select();
        }
      }
      if (Input.GetKeyUp(KeyCode.Return)) { RegisterPlayerBttn(); }
    }


    // 회원가입
    public void RegisterPlayerBttn()
    {
        new GameSparks.Api.Requests.RegistrationRequest()
            .SetDisplayName(displayNameInput.text) // DisplayName 이 닉네임일듯
            .SetPassword(passwordInput.text) // 비밀번호
            .SetUserName(userNameInput.text) // 계정아이디
            .Send((response) =>
            {
                if (!response.HasErrors)
                {
                    Debug.Log("회원가입 완료");
                    var json = new JObject();

                    string authToken = response.AuthToken;
                    string displayName = response.DisplayName;
                    bool? newPlayer = response.NewPlayer;
                    string userId = response.UserId;
                    string userRole;
                    if(studentToggle.isOn){
                      userRole ="S";
                    }else{
                      userRole ="T";
                    }

                    json.Add("authToken", authToken);
                    json.Add("displayName", displayName);
                    json.Add("newPlayer", newPlayer);
                    json.Add("userId", userId);
                    json.Add("skinRole", userRole);

                    Utility.request_server(json, "login");
                    LoginObject.SetActive(true);
                    RegisterObject.SetActive(false);
                    
                }
                else
                {
                    StartCoroutine(Error());
                    Debug.Log("회원가입 실패" + response.Errors.JSON.ToString());
                }
            }
        );
    }

    IEnumerator Error(){
      ErrorObject.SetActive(true);
      yield return new WaitForSeconds(2f);
      ErrorObject.SetActive(false);
    }
}
