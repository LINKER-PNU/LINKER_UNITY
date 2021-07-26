using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;

public class ControllServerInMain : GlobalEventListener
{
    public GameObject MainObject;
    public void CreateNewRoomBttn()
    {
        // START SERVER
        BoltLauncher.StartServer(UdpKit.UdpEndPoint.Parse("127.0.0.1:27000"));
    }
    public override void BoltStartDone()
    {
        MainObject.SetActive(false);
        if (BoltNetwork.IsServer) { }
        else BoltNetwork.Connect(27000);

    }
}
