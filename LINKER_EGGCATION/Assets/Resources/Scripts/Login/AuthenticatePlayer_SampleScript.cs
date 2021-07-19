using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
 
public class AuthenticatePlayer_SampleScript : MonoBehaviour
{
    public InputField displayNameInput, userNameInput, passwordInput;
 
 
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
                }
                else
                {
                    Debug.Log("로그인 실패..." + response.Errors.JSON.ToString());
                }
            });
    }
 
    // DisplayName 으로 로그인
    public void AuthenticateDeviceBttn()
    {
        new GameSparks.Api.Requests.DeviceAuthenticationRequest()
            .SetDisplayName(displayNameInput.text)
            .Send((response) => {
                if (!response.HasErrors)
                {
                    Debug.Log("Device 로그인 성공...");
                }
                else
                {
                    Debug.Log("Device 로그인 실패..." + response.Errors.JSON.ToString());
                }
            });
    }
}