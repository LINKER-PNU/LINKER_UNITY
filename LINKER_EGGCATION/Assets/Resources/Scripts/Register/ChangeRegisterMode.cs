using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeRegisterMode : MonoBehaviour
{
    public GameObject LoginObject;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void changeWindow()
    {
        LoginObject.SetActive(false);
    }
}
