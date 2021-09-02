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
    public AudioListener AudioListener;


    // Start is called before the first frame update
    void Start()
    {
        VoiceRecorder.TransmitEnabled = true;
        VoiceAudioSource.mute = false;
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_ANDROID
#else
        if (GameManager.boardPanelObject.activeInHierarchy)
        {
            return;
        }
        if (Input.GetKeyDown(SpeakBtn))
        {
            if (photonView.IsMine)
            {
                if (VoiceRecorder.TransmitEnabled == true)
                {
                    VoiceRecorder.TransmitEnabled = false;
                    VoiceAudioSource.volume = 0;
                    GameManager.micOnObject.SetActive(false);
                    GameManager.micOffObject.SetActive(true);
                }
                else
                {
                    VoiceAudioSource.volume = 1;
                    VoiceRecorder.TransmitEnabled = true;
                    GameManager.micOnObject.SetActive(true);
                    GameManager.micOffObject.SetActive(false);
                }
            }
        }
        if (Input.GetKeyDown(ListenBtn))
        {
            if (photonView.IsMine)
            {
                if (AudioListener.volume == 0)
                {
                    GameManager.headsetOnObject.SetActive(true);
                    GameManager.headsetOffObject.SetActive(false);
                    AudioListener.volume = 1;
                }
                else
                {
                    GameManager.headsetOnObject.SetActive(false);
                    GameManager.headsetOffObject.SetActive(true);
                    AudioListener.volume = 0;
                }
            }
        }
#endif
    }
}
