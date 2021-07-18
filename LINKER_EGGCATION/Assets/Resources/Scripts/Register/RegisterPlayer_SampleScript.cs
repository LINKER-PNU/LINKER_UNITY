using System.Collections;
using System.Collections.Generic;
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

public class RegisterPlayer_SampleScript : MonoBehaviour
{
  public InputField displayNameInput, userNameInput, passwordInput;

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
            // GSData scriptData = response.ScriptData;
            // AuthenticationResponse._Player switchSummary = response.SwitchSummary;
            string userId = response.UserId;

            Debug.Log("authToken " + authToken);
            Debug.Log("displayName " + displayName);
            Debug.Log("newPlayer " + newPlayer);
            // Debug.Log("scriptData" + scriptData);
            // Debug.Log("switchSummary" + switchSummary);
            Debug.Log("userId " + userId);

            string url = "http://192.168.219.181:8080/login";
            json.Add("authToken", authToken);
            json.Add("displayName", displayName);
            json.Add("newPlayer", newPlayer);
            json.Add("userId", userId);

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
            // var res = wb.UploadValues(url, "POST", json);

          }
          else
          {

            Debug.Log("회원가입 실패" + response.Errors.JSON.ToString());
          }
        }
    );
  }
}
