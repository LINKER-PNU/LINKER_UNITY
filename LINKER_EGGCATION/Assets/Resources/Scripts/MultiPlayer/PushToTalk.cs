using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using Photon.Voice.PUN;


public class PushToTalk : MonoBehaviourPun
{
    public KeyCode SpeakBtn = KeyCode.M;
    public KeyCode ListenBtn = KeyCode.V;
    public Recorder VoiceRecorder;
    public AudioSource VoiceAudioSource;


    // Start is called before the first frame update
    void Start()
    {
        VoiceRecorder.TransmitEnabled = false;
        VoiceAudioSource.mute = false;
    }

    // Update is called once per frame
    void Update()
    {
      if(Input.GetKeyDown(SpeakBtn))
      {
        if(photonView.IsMine)
        {
          VoiceRecorder.TransmitEnabled = !VoiceRecorder.TransmitEnabled;
          if(VoiceRecorder.TransmitEnabled == true){
            Debug.Log(GameManager.voiceObject);
            GameManager.voiceObject.SetActive(true);
            GameManager.noVoiceObject.SetActive(false);
          }else{
            GameManager.voiceObject.SetActive(false);
            GameManager.noVoiceObject.SetActive(true);
          }
        }
      }
      if(Input.GetKeyDown(ListenBtn))
      {
        if(photonView.IsMine)
        {
          VoiceAudioSource.mute = !VoiceAudioSource.mute;
          if(VoiceAudioSource.mute == true){
            GameManager.muteObject.SetActive(true);
            GameManager.noMuteObject.SetActive(false);
          }else{
            GameManager.muteObject.SetActive(false);
            GameManager.noMuteObject.SetActive(true);
          }
        }
      }    
    }
}
