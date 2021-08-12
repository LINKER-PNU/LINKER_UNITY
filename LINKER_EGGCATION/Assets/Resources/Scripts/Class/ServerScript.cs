using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;
using agora_utilities;
using AgoraNative;
using System.Net;
using System.IO;

using Newtonsoft.Json.Linq;
using Random = UnityEngine.Random;

public class ServerScript : MonoBehaviour
{
    [SerializeField] private string APP_ID = "";

    private string TOKEN = "";

    private string CHANNEL_NAME = "YOUR_CHANNEL_NAME";

    private IRtcEngine mRtcEngine;
    private uint remoteUid = 0;
    private const float Offset = 100;
    public Text logText;
    private Logger _logger;
    private Dropdown _winIdSelect;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    private Dictionary<uint, AgoraNativeBridge.RECT> _dispRect;
#endif

    // Use this for initialization
    void Start()
    {
        CHANNEL_NAME = "linker_test";
        TOKEN = get_token();
        //CHANNEL_NAME = ControlServerInMain.roomName;
        checkClassExist();

        _logger = new Logger(logText);
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        _dispRect = new Dictionary<uint, AgoraNativeBridge.RECT>();
#endif
        if (mRtcEngine != null)
        {
            Debug.Log("Agora engine exists already!!");
            return;
        }
        CheckAppId();
        InitEngine();
        JoinChannel();
        PrepareScreenCapture();
    }

    // Update is called once per frame
    void Update()
    {
    }
    private string get_token()
    {
        var json = new JObject();
        string method = "get_token";

        json.Add("roomName", CHANNEL_NAME);
        return request_server(json, method);
    }
    void checkClassExist()
    {
        var json = new JObject();
        string method = "check_class_exist";

        json.Add("classType", "create");

        json.Add("roomName", CHANNEL_NAME);
        if (Convert.ToBoolean(request_server(json, method)))
        {
            Debug.Log("이미 수업 중입니다!");
            // Scene 로드
        }
    }
    private void CheckAppId()
    {
        _logger.DebugAssert(APP_ID.Length > 10, "Please fill in your appId in VideoCanvas!!!!!");
    }

    private void JoinChannel()
    {
        mRtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME);
    }

    private void InitEngine()
    {
        mRtcEngine = IRtcEngine.GetEngine(APP_ID);
        mRtcEngine.SetLogFile("log.txt");
        //mRtcEngine.EnableAudio();
        mRtcEngine.EnableVideo();
        mRtcEngine.EnableVideoObserver();
        mRtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
        mRtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
        mRtcEngine.OnWarning += OnSDKWarningHandler;
        mRtcEngine.OnError += OnSDKErrorHandler;
        mRtcEngine.OnConnectionLost += OnConnectionLostHandler;
    }

    private void PrepareScreenCapture()
    {
        _winIdSelect = GameObject.Find("winIdSelect").GetComponent<Dropdown>();
        _winIdSelect.interactable = false;

        if (_winIdSelect != null)
        {
            _winIdSelect.ClearOptions();
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            var macDispIdList = AgoraNativeBridge.GetMacDisplayIds();
            if (macDispIdList != null)
            {
                _winIdSelect.AddOptions(macDispIdList.Select(w =>
                    new Dropdown.OptionData(
                        string.Format("Display {0}", w))).ToList());
            }

            var macWinIdList = AgoraNativeBridge.GetMacWindowList();
            if (macWinIdList != null)
            {
                _winIdSelect.AddOptions(macWinIdList.windows.Select(w =>
                        new Dropdown.OptionData(
                            string.Format("{0, -20} | {1}", w.kCGWindowOwnerName, w.kCGWindowNumber)))
                    .ToList());
            }
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            var winDispInfoList = AgoraNativeBridge.GetWinDisplayInfo();
            if (winDispInfoList != null)
            {
                foreach (var dpInfo in winDispInfoList)
                {
                    _dispRect.Add(dpInfo.MonitorInfo.flags, dpInfo.MonitorInfo.monitor);
                }

                _winIdSelect.AddOptions(winDispInfoList.Select(w =>
                    new Dropdown.OptionData(
                        string.Format("Display {0}", w.MonitorInfo.flags))).ToList());
            }

            Dictionary<string, IntPtr> winWinIdList;
            AgoraNativeBridge.GetDesktopWindowHandlesAndTitles(out winWinIdList);
            if (winWinIdList != null)
            {
                _winIdSelect.AddOptions(winWinIdList.Select(w =>
                    new Dropdown.OptionData(string.Format("{0, -20} | {1}",
                        w.Key.Substring(0, Math.Min(w.Key.Length, 20)), w.Value))).ToList());
            }
#endif
        }

        // 준비가 완료되면 바로 공유 시작
        mRtcEngine.StopScreenCapture();

        if (_winIdSelect == null) return;
        var option = _winIdSelect.options[_winIdSelect.value].text;
        if (string.IsNullOrEmpty(option)) return;
        if (option.Contains("|"))
        {
            var windowId = option.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
            _logger.UpdateLog(string.Format(">>>>> Start sharing {0}", windowId));
            mRtcEngine.StartScreenCaptureByWindowId(int.Parse(windowId), default(Rectangle),
                default(ScreenCaptureParameters));
        }
        else
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            var dispId = uint.Parse(option.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1]);
            _logger.UpdateLog(string.Format(">>>>> Start sharing display {0}", dispId));
            mRtcEngine.StartScreenCaptureByDisplayId(dispId, default(Rectangle),
                new ScreenCaptureParameters {captureMouseCursor = true, frameRate = 30});
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            var diapFlag = uint.Parse(option.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1]);
            var screenRect = new Rectangle
            {
                x = _dispRect[diapFlag].left,
                y = _dispRect[diapFlag].top,
                width = _dispRect[diapFlag].right - _dispRect[diapFlag].left,
                height = _dispRect[diapFlag].bottom - _dispRect[diapFlag].top
            };
            _logger.UpdateLog(string.Format(">>>>> Start sharing display {0}: {1} {2} {3} {4}", diapFlag, screenRect.x,
                screenRect.y, screenRect.width, screenRect.height));
            var ret = mRtcEngine.StartScreenCaptureByScreenRect(screenRect,
                new Rectangle { x = 0, y = 0, width = 0, height = 0 }, default(ScreenCaptureParameters));
#endif
        }
    }

    private void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        _logger.UpdateLog(string.Format("sdk version: ${0}", IRtcEngine.GetSdkVersion()));
        _logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName,
            uid, elapsed));

        var json = new JObject();
        string method = "insert_class_master";

        json.Add("classMaster", Convert.ToString(uid));
        json.Add("roomName", CHANNEL_NAME);
        if (Convert.ToBoolean(request_server(json, method)))
        {
            makeVideoView(0);
        }
        else
        {
            if (mRtcEngine != null)
            {
                mRtcEngine.LeaveChannel();
                mRtcEngine.DisableVideoObserver();
                IRtcEngine.Destroy();
            }
            Debug.Log("이미 수업 중입니다!");
            // Scene 돌아가기
        }
    }

    private void OnLeaveChannelHandler(RtcStats stats)
    {
        Debug.Log("OnLeaveChannelSuccess");
        _logger.UpdateLog("OnLeaveChannelSuccess");
        DestroyVideoView(0);
    }

    private void OnSDKWarningHandler(int warn, string msg)
    {
        _logger.UpdateLog(string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, msg));
    }

    private void OnSDKErrorHandler(int error, string msg)
    {
        _logger.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
    }

    private void OnConnectionLostHandler()
    {
        _logger.UpdateLog("OnConnectionLost ");
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        var json = new JObject();
        string method = "delete_class_master";

        json.Add("roomName", CHANNEL_NAME);
        if (Convert.ToBoolean(request_server(json, method)))
        {
            if (mRtcEngine != null)
            {
                mRtcEngine.LeaveChannel();
                mRtcEngine.DisableVideoObserver();
                IRtcEngine.Destroy();
            }
            // Scene 이동
            Debug.Log("EXIT");
        }
        else
        {
            Debug.Log("class master 삭제 에러");
        }
    }

    void OnDisable()
    {
        Debug.Log("OnDisable");
        var json = new JObject();
        string method = "delete_class_master";

        json.Add("roomName", CHANNEL_NAME);
        if (Convert.ToBoolean(request_server(json, method)))
        {
            if (mRtcEngine != null)
            {
                mRtcEngine.LeaveChannel();
                mRtcEngine.DisableVideoObserver();
                IRtcEngine.Destroy();
            }
            // Scene 이동
            Debug.Log("EXIT");
        }
        else
        {
            Debug.Log("class master 삭제 에러");
        }
    }

    private void DestroyVideoView(uint uid)
    {
        var go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            Destroy(go);
        }
    }

    private void makeVideoView(uint uid)
    {
        var go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            return; // reuse
        }

        // create a GameObject and assign to this new user
        var videoSurface = makePlaneSurface(uid.ToString());
        if (!ReferenceEquals(videoSurface, null))
        {
            // configure videoSurface
            videoSurface.SetForUser(uid);
            videoSurface.SetEnable(true);
            videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.Renderer);
            videoSurface.SetGameFps(30);
            videoSurface.EnableFilpTextureApply(true, false);
        }
        Debug.Log("HERE?");
    }

    // VIDEO TYPE 1: 3D Object
    public VideoSurface makePlaneSurface(string goName)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Plane);

        if (go == null)
        {
            return null;
        }

        go.name = goName;
        //// to be renderered onto
        //go.AddComponent<RawImage>();
        // set up transform
        go.transform.Rotate(270.0f, 270.0f, 180.0f);
        //var yPos = Random.Range(3.0f, 5.0f);
        //var xPos = Random.Range(-2.0f, 2.0f);
        go.transform.position = new Vector3(0.9f, 0.09881967f, -0.1224516f);
        go.transform.localScale = new Vector3(0.5f, 0.25f, .5f);

        // configure videoSurface
        var videoSurface = go.AddComponent<VideoSurface>();
        Debug.Log("CREATE");
        return videoSurface;
    }

    // Video TYPE 2: RawImage
    public VideoSurface makeImageSurface(string goName)
    {
        var go = new GameObject();

        if (go == null)
        {
            return null;
        }

        go.name = goName;
        // to be renderered onto
        go.AddComponent<RawImage>();
        // make the object draggable
        go.AddComponent<UIElementDrag>();
        var canvas = GameObject.Find("VideoCanvas");
        if (canvas != null)
        {
            go.transform.parent = canvas.transform;
            Debug.Log("add video view");
        }
        else
        {
            Debug.Log("Canvas is null video view");
        }

        // set up transform
        go.transform.Rotate(0.0f, 0.0f, 180.0f);
        var xPos = Random.Range(Offset - Screen.width / 2f, Screen.width / 2f - Offset);
        var yPos = Random.Range(Offset, Screen.height / 2f - Offset);
        go.transform.localPosition = new Vector3(0, 0, 0f);
        go.transform.localScale = new Vector3(8f, 4.5f, 1f);


        //var goTransform = go.GetComponent<RectTransform>();
        //goTransform.localPosition = new Vector3(0, -100, 0);
        //Debug.LogFormat("{0}, {1}", Screen.width, Screen.height);
        //goTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
        ////go.transform.localScale = new Vector3(Screen.width, Screen.height - 10f, 0);

        // configure videoSurface
        var videoSurface = go.AddComponent<VideoSurface>();
        return videoSurface;
    }
    
    public void onClickExit()
    {
        var json = new JObject();
        string method = "delete_class_master";

        json.Add("roomName", CHANNEL_NAME);
        if (Convert.ToBoolean(request_server(json, method)))
        {
            if (mRtcEngine != null)
            {
                mRtcEngine.LeaveChannel();
                mRtcEngine.DisableVideoObserver();
                IRtcEngine.Destroy();
            }
            // Scene 이동
            Debug.Log("EXIT");
        }
        else
        {
            Debug.Log("class master 삭제 에러");
        }

    }

    private string request_server(JObject req, string method)
    {
        string url = "http://34.64.85.29:8080/";
        var httpWebRequest = (HttpWebRequest)WebRequest.Create(url + method);
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            streamWriter.Write(req.ToString());
        }

        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        string characterSet = httpResponse.CharacterSet;
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream(), System.Text.Encoding.UTF8, true))
        {
            var result = streamReader.ReadToEnd();
            Debug.Log(result);
            return result;
        }
    }
}