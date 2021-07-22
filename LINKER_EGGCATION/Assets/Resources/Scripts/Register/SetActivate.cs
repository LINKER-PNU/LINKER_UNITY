using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetActivate : MonoBehaviour
{
    // Start is called before the first frame update
    private bool        state;
    public GameObject   Target;
    void Start()
    {
        state = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            print("마우스 입력 받았음");
            if (state == true) {
                Target.SetActive(false);
                print("사라져");
                state = false;
            } else {
                Target.SetActive(true);
                print("생겨나");
                state = true;
            }
        }
    }
}
