using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Newtonsoft.Json.Linq;

using eggcation;

public class ShowBoard : MonoBehaviour
{
    public GameObject BoardBttnContentObject, BoardBttnObject;
    [SerializeField]
    public int y_offset = 180;

    [SerializeField]
    public int offset = 150;

    [SerializeField]
    GameObject ShowBoardObject;

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

    [SerializeField]
    GameObject CreateBttnObject;

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

        getBoardList();
    }

    // Update is called once per frame
    void Update()
    {
        NoticeToggle.interactable = true;
        HomeworkToggle.interactable = true; 
        TitleInput.interactable = true;
        ContentInput.interactable = true;

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

    public void OnEditClicked()
    {
        var json = new JObject();
        string method = "board/insert";

        string boardTitle = TitleInput.text;

        json.Add("boardRoom", Utility.roomName);

        var result = JObject.Parse(Utility.request_server(json, method));
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
    
    public void getBoardList()
    {
        var json = new JObject();
        string method = "board/list";

        //json.Add("boardRoom", Utility.roomName);
        json.Add("boardRoom", "linker_test");

        var boardList = JObject.Parse(Utility.request_server(json, method));
        createBoardButton(boardList);
    }
    public void createBoardButton(JToken boardList)
    {
        int count = 0;
        foreach (JToken board in boardList)
        {
            // 버튼 추가
            GameObject BoardBttn = Instantiate(BoardBttnObject);
            BoardBttn.transform.SetParent(BoardBttnContentObject.transform);
            Text BoardBttnText = BoardBttn.GetComponentInChildren<Text>();
            BoardBttnText.text = board["board_name"].ToString() + " ●" + "(" +
                board["board_present"].ToString() + " / " +
                board["board_max"].ToString() + ")";
            RectTransform BoardBttnTranform = BoardBttn.GetComponent<RectTransform>();
            Vector3 BoardBttnNewPosition = new Vector3(BoardBttnTranform.position.x, BoardBttnTranform.position.y - count * y_offset, BoardBttnTranform.position.z);
            BoardBttnTranform.localPosition = BoardBttnNewPosition;
            BoardBttnTranform.localScale = new Vector3(1, 1, 1);
            Debug.Log(BoardBttnTranform.localScale);
            count += 1;

            // onClick 추가
            Button BoardBttnComp = BoardBttn.GetComponent<Button>();
            BoardBttnComp.onClick.AddListener(() => ShowBoardById((int)board["boardId"]));

            Debug.LogFormat("방 목록에 추가 : {0}", board["board_name"].ToString());
        }
        RectTransform ContentTransform = BoardBttnContentObject.GetComponent<RectTransform>();
        ContentTransform.sizeDelta = new Vector2(0, offset * 2 + y_offset * (count - 1));
    }
    
    void ShowBoardById(int boardId)
    {
        ShowBoardObject.SetActive(true);

    }
}
