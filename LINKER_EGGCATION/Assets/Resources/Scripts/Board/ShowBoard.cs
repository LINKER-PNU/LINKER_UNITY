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

    [SerializeField]
    GameObject EditBttnObject;

    Toggle NoticeToggle;
    Toggle HomeworkToggle;

    TMP_InputField TitleInput;
    TMP_InputField ContentInput;

    int currentBoardId;

    // Start is called before the first frame update
    void Awake()
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
    void Start()
    {
        getBoardList();
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
        string method = "board/edit";

        string boardTitle = TitleInput.text;
        string boardContent = ContentInput.text;
        string boardDeadline = HomeworkToggle.isOn ?
                            Year.GetComponent<TMP_InputField>().text + "-" +
                            Month.GetComponent<TMP_InputField>().text + "-" +
                            Day.GetComponent<TMP_InputField>().text + " " +
                            Hour.GetComponent<TMP_InputField>().text + ":" +
                            Minutes.GetComponent<TMP_InputField>().text + ":00"
                            : "2001-01-01 00:00:00";
        bool boardNotice = NoticeToggle.isOn;
        bool boardAssignment = HomeworkToggle.isOn;

        Debug.LogFormat("boardWriterId", Utility.userId);
        Debug.LogFormat("boardTitle", boardTitle);
        Debug.LogFormat("boardContent", boardContent);
        Debug.LogFormat("boardDeadline", boardDeadline);
        Debug.LogFormat("boardNotice", boardNotice);
        Debug.LogFormat("boardAssignment", boardAssignment);


        //json.Add("boardRoom", Utility.roomName);
        json.Add("boardId", currentBoardId);
        json.Add("boardTitle", boardTitle);
        json.Add("boardContent", boardContent);
        json.Add("boardDeadline", boardDeadline);
        json.Add("boardNotice", boardNotice);
        json.Add("boardAssignment", boardAssignment);

        json.Add("boardRoom", Utility.roomName);

        JObject.Parse(Utility.request_server(json, method));
        EditBttnObject.SetActive(false);
    }
    
    public void getBoardList()
    {
        var json = new JObject();
        string method = "board/list";

        //json.Add("boardRoom", Utility.roomName);
        json.Add("boardRoom", "linker_test");

        var boardList = JObject.Parse(Utility.request_server(json, method));
        createBoardButton(boardList["result"]);
    }
    public void createBoardButton(JToken boardList)
    {
        int count = 0;
        foreach (JToken board in boardList)
        {
            // 버튼 추가
            GameObject BoardBttn = Instantiate(BoardBttnObject);
            BoardBttn.transform.SetParent(BoardBttnContentObject.transform);
            TMP_Text BoardBttnText = BoardBttn.GetComponentInChildren<TMP_Text>();
            Debug.Log(board["boardTitle"]);
            BoardBttnText.text = (string)board["boardTitle"];
            RectTransform BoardBttnTranform = BoardBttn.GetComponent<RectTransform>();
            Vector3 BoardBttnNewPosition = new Vector3(BoardBttnTranform.position.x, BoardBttnTranform.position.y - count * y_offset, BoardBttnTranform.position.z);
            Debug.Log(y_offset);
            BoardBttnTranform.localPosition = BoardBttnNewPosition;
            BoardBttnTranform.localScale = new Vector3(1, 1, 1);
            Debug.Log(BoardBttnTranform.localPosition);
            Debug.Log(BoardBttnTranform.position);
            count += 1;

            // onClick 추가
            Button BoardBttnComp = BoardBttn.GetComponent<Button>();
            BoardBttnComp.onClick.AddListener(() => ShowBoardById(board));

            Debug.LogFormat("방 목록에 추가 : {0}", (string)board["boardTitle"]);
        }
        RectTransform ContentTransform = BoardBttnContentObject.GetComponent<RectTransform>();
        ContentTransform.sizeDelta = new Vector2(0, offset * 2 + y_offset * (count - 1));
    }
    
    public void ShowBoardById(JToken board)
    {
        currentBoardId = (int)board["boardId"];
        ShowBoardObject.SetActive(true);

        var json = new JObject();
        string method = "board/content";

        json.Add("boardId", currentBoardId);

        var contents = Utility.request_server(json, method);

        TitleInput.text = (string)board["boardTitle"];
        ContentInput.text = contents;
        string[] boardDeadline = ((string)board["boardDeadline"]).Split(' ');
        string[] boardDate = boardDeadline[0].Split('-');
        string[] boardTime = boardDeadline[1].Split(':');
        Year.GetComponent<TMP_InputField>().text = boardDate[0];
        Month.GetComponent<TMP_InputField>().text = boardDate[1];
        Day.GetComponent<TMP_InputField>().text = boardDate[2];
        Hour.GetComponent<TMP_InputField>().text = boardTime[0];
        Minutes.GetComponent<TMP_InputField>().text = boardTime[1];

        NoticeToggle.isOn = (bool)board["boardNotice"];
        HomeworkToggle.isOn = (bool)board["boardAssignment"];

        NoticeToggle.interactable = false;
        HomeworkToggle.interactable = false;
        TitleInput.interactable = false;
        ContentInput.interactable = false;
        Year.GetComponent<TMP_InputField>().interactable = false;
        Month.GetComponent<TMP_InputField>().interactable = false;
        Day.GetComponent<TMP_InputField>().interactable = false;
        Hour.GetComponent<TMP_InputField>().interactable = false;
        Minutes.GetComponent<TMP_InputField>().interactable = false;
        if ((string)board["boardWriterId"] == Utility.userId)
        {
            EditBttnObject.SetActive(true);
            NoticeToggle.interactable = true;
            HomeworkToggle.interactable = true;
            TitleInput.interactable = true;
            ContentInput.interactable = true;
            Year.GetComponent<TMP_InputField>().interactable = true;
            Month.GetComponent<TMP_InputField>().interactable = true;
            Day.GetComponent<TMP_InputField>().interactable = true;
            Hour.GetComponent<TMP_InputField>().interactable = true;
            Minutes.GetComponent<TMP_InputField>().interactable = true;
        }

    }
}
