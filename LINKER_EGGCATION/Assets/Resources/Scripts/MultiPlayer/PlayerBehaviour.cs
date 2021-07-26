using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;

public class PlayerBehaviour : EntityBehaviour<ILinkerPlayerState>
{

    private float _resetColorTime;
    private Renderer _renderer;
    // Unity의 Start 메서드와 같은 역할
    public override void Attached()
    {
        // Your code here...
        _renderer = GetComponent<Renderer>();

        state.SetTransforms(state.PlayerTransform, transform);

        if (entity.IsOwner)
        {
            state.PlayerColor = new Color(Random.value, Random.value, Random.value);
        }

        state.AddCallback("CubeColor", ColorChanged);
    }

    // Unity의 Update 메서드와 같은 역할
    public override void SimulateOwner()
    {
        var speed = 4f;
        var movement = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) { movement.z += 1; }
        if (Input.GetKey(KeyCode.S)) { movement.z -= 1; }
        if (Input.GetKey(KeyCode.A)) { movement.x -= 1; }
        if (Input.GetKey(KeyCode.D)) { movement.x += 1; }

        if (movement != Vector3.zero)
        {
            transform.position = transform.position + (movement.normalized * speed * BoltNetwork.FrameDeltaTime);
        }
        //if (Input.GetKeyDown(KeyCode.F))
        //{
        //    var flash = FlashColorEvent.Create(entity);
        //    flash.FlashColor = Color.red;
        //    flash.Send();
        //}
    }
    //public override void OnEvent(FlashColorEvent evnt)
    //{
    //    _resetColorTime = Time.time + 0.2f;
    //    _renderer.material.color = evnt.FlashColor;
    //}
    //void OnGUI()
    //{
    //    if (entity.IsOwner)
    //    {
    //        GUI.color = state.PlayerColor;
    //        GUILayout.Label("@@@");
    //        GUI.color = Color.white;
    //    }
    //}
    //void Update()
    //{
    //    if (_resetColorTime < Time.time)
    //    {
    //        _renderer.material.color = state.PlayerColor;
    //    }
    //}
    void ColorChanged()
    {
        GetComponent<Renderer>().material.color = state.PlayerColor;
    }
}
