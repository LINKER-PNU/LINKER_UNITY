using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using Photon.Voice.PUN;


public class PushToTalk : MonoBehaviourPun
{
    public KeyCode PushButton = KeyCode.P;
    public Recorder VoiceRecorder;

    // Start is called before the first frame update
    void Start()
    {
        VoiceRecorder.TransmitEnabled = false;
    }

    // Update is called once per frame
    void Update()
    {
      if(Input.GetKeyDown(PushButton))
      {
        if(photonView.IsMine)
        {
          VoiceRecorder.TransmitEnabled = true;
        }
      }
      else if(Input.GetKeyUp(PushButton)){
        if(photonView.IsMine)
        {
          VoiceRecorder.TransmitEnabled = false;
        }
      }
        
    }
}
