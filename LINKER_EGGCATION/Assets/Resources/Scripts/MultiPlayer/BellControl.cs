using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BellControl : MonoBehaviour
{
    // public Text text_date;
    // public Text text_time;
    // public Text text_get;
    // public Button button_get;
    [SerializeField]
    public AudioSource m_MyAudioSource;

    private void Start()
    {
        // Init_UI();
        m_MyAudioSource = GetComponent<AudioSource>();
        Init_Time();
    }

    private void Init_Time()
    {
        if (IsInvoking("Update_Time"))
            CancelInvoke("Update_Time");
        InvokeRepeating("Update_Time", 0, 60f);
    }
    // private void Init_UI()
    // {
    //     button_get.onClick.RemoveAllListeners();
    //     button_get.onClick.AddListener(Get_Time);
    // }
    private  void Update_Time()
    {
        string date = DateTime.Now.ToString("yyyy.MM.dd") + DateTime.Now.DayOfWeek.ToString().ToUpper().Substring(0, 3);
        string time = DateTime.Now.ToString("mm");

        
        // text_date.text = date;
        // text_time.text = time;

        if(time == "00" || time =="50"){
          m_MyAudioSource.Play();
        }

    }
    // private void Get_Time()
    // {
    //     text_get.text = text_date.text + "\n" + text_time.text;
    //     Debug.Log(text_get.text);
    // }
}
