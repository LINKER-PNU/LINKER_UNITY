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

public class RegisterPlayer_SampleScript : MonoBehaviour
{
    public InputField displayNameInput, userNameInput, passwordInput;

    private static string ReadStreamFromResponse(WebResponse response)
    {
        using (Stream responseStream = response.GetResponseStream())
        using (StreamReader sr = new StreamReader(responseStream))
        {
            //Need to return this response 
            string strContent = sr.ReadToEnd();
            return strContent;
        }
    }
    void Main(RegistrationResponse response)
    {
        string url = "http://192.168.219.165:8080/login";

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

        json.Add("authToken", authToken);
        json.Add("displayName", displayName);
        json.Add("newPlayer", newPlayer);
        json.Add("userId", userId);

        var task = MakeAsyncRequest(url, "application/json", json);
        Debug.Log(task.Result);
    }

    // Define other methods and classes here
    public static Task<string> MakeAsyncRequest(string url, string contentType, JObject json)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.ContentType = contentType;
        request.Method = "POST";
        request.Timeout = 2147483647;
        request.Proxy = null;

        using (var streamWriter = new StreamWriter(request.GetRequestStream()))
        {
            streamWriter.Write(json.ToString());
        }

        Task<WebResponse> task = Task.Factory.FromAsync(
            request.BeginGetResponse,
            asyncResult => request.EndGetResponse(asyncResult),
            (object)null);

        return task.ContinueWith(t => ReadStreamFromResponse(t.Result));
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
                    Main(response);
                }
                else
                {

                    Debug.Log("회원가입 실패" + response.Errors.JSON.ToString());
                }
            }
        );
    }
}
