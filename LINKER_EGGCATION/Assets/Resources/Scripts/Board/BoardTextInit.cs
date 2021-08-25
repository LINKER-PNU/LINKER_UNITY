using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Newtonsoft.Json.Linq;

using eggcation;

public class BoardTextInit : MonoBehaviour
{
    [SerializeField]
    GameObject boardTextObject;

    // Start is called before the first frame update
    void Awake()
    {
        boardInit();
    }

    public void boardInit()
    {
        var json = new JObject();
        string method = "board/list";

        //json.Add("boardRoom", Utility.roomName);
        json.Add("boardRoom", "linker_test");

        var boardList = JObject.Parse(Utility.request_server(json, method));
        boardTextObject.GetComponent<TextMeshProUGUI>().text = boardList["result"].HasValues ? (string)boardList["result"][0]["boardTitle"] : string.Empty;
    }

}
