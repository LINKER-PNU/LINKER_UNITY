using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;


[BoltGlobalBehaviour]
public class NetworkCallbacks : GlobalEventListener
{
    public override void Connected(BoltConnection connection)
    {
        // randomize a position
        var pos = new Vector3(Random.Range(-4, 4), 0, Random.Range(-4, 4));

        // instantiate cube
        BoltNetwork.Instantiate(BoltPrefabs.mainPlayer, pos, Quaternion.identity);
    }
}