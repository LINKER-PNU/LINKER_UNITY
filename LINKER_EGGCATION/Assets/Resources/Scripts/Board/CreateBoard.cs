using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Newtonsoft.Json.Linq;

using eggcation;

public class CreateBoard : MonoBehaviour
{
    [SerializeField]
    GameObject CreateBoardObject;

    [SerializeField]
    GameObject TitleObject;

    [SerializeField]
    GameObject ContentObject;

    [SerializeField]
    GameObject NoticeObject;

    [SerializeField]
    GameObject HomeworkObject;
    
    [SerializeField]
    GameObject Year;
    
    [SerializeField]
    GameObject Month;
    
    [SerializeField]
    GameObject Day;
    
    [SerializeField]
    GameObject Hour;
    
    [SerializeField]
    GameObject Minutes;

    Toggle NoticeToggle;
    Toggle HomeworkToggle;

    TMP_InputField TitleInput;
    TMP_InputField ContentInput;

    // Start is called before the first frame update
    void Start()
    {
        NoticeToggle = NoticeObject.GetComponent<Toggle>();
        HomeworkToggle = HomeworkObject.GetComponent<Toggle>();

        NoticeToggle.onValueChanged.AddListener(delegate {
            if (NoticeToggle.isOn)
            {
                NoticeToggleClicked();
            }
        });
        HomeworkToggle.onValueChanged.AddListener(delegate {
            if (HomeworkToggle.isOn)
            {
                HomeworkToggleClicked();
            }
        });

        TitleInput = TitleObject.GetComponent<TMP_InputField>();
        ContentInput = ContentObject.GetComponent<TMP_InputField>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void NoticeToggleClicked()
    {
        if (HomeworkToggle.isOn)
        {
            HomeworkToggle.isOn = false;
        }
        Year.SetActive(false);
        Month.SetActive(false);
        Day.SetActive(false);
        Hour.SetActive(false);
        Minutes.SetActive(false);
    }

    void HomeworkToggleClicked()
    {
        if (NoticeToggle.isOn)
        {
            NoticeToggle.isOn = false;
        }
        Year.SetActive(true);
        Month.SetActive(true);
        Day.SetActive(true);
        Hour.SetActive(true);
        Minutes.SetActive(true);
    }

    public void OnCreateClicked()
    {
        var json = new JObject();
        string method = "board/insert";

        string boardTitle = TitleInput.text;
        string boardContent = ContentInput.text;
        string boardDeadline = HomeworkToggle.isOn ? 
                            Year.GetComponent<TMP_InputField>().text + "-" +
                            Month.GetComponent<TMP_InputField>().text + "-" +
                            Day.GetComponent<TMP_InputField>().text + " " +
                            Hour.GetComponent<TMP_InputField>().text + ":" +
                            Minutes.GetComponent<TMP_InputField>().text + ":00"
                            : string.Empty;
        bool boardNotice = NoticeToggle.isOn;
        bool boardAssignment = HomeworkToggle.isOn;

        json.Add("boardRoom", Utility.roomName);
        json.Add("boardWriterId", Utility.userId);
        json.Add("boardTitle", boardTitle);
        json.Add("boardContent", boardContent);
        json.Add("boardDeadline", boardDeadline);
        json.Add("boardNotice", boardNotice);
        json.Add("boardAssignment", boardAssignment);

        var result = JObject.Parse(Utility.request_server(json, method));
    }
}
