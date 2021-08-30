using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoMoveManager : MonoBehaviour
{

    [SerializeField]
    AutoMoveController player1, player3, player4;

    // private bool isP1Grounded = true,isP3Grounded = true,isP4Grounded = true;
    

    // Start is called before the first frame update
    void Start()
    {
      StartCoroutine(Player1Move());
      StartCoroutine(Player3Move());
      StartCoroutine(Player4Move());

    }

    void Update(){

    }



    private IEnumerator Player1Move()
    {
      while(true){
        player1.GetComponent<AutoMoveController>().JumpTo();
        yield return new WaitForSeconds(1f);
      }
      
    }
    private IEnumerator Player3Move()
    {
      while(true){
        player3.GetComponent<AutoMoveController>().MoveForward();
        yield return new WaitForSeconds(3f);
        player3.GetComponent<AutoMoveController>().JumpTo();
        yield return new WaitForSeconds(2.3f);
        player3.GetComponent<AutoMoveController>().Rotation();
        yield return new WaitForSeconds(0.05f);
        player3.GetComponent<AutoMoveController>().MoveForward();
        yield return new WaitForSeconds(1.5f);
        player3.GetComponent<AutoMoveController>().JumpTo();
        // yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(1f);
        player3.GetComponent<AutoMoveController>().Rotation();
        yield return new WaitForSeconds(0.05f);
        
        
      }
    }
    private IEnumerator Player4Move()
    {
      while(true){
      yield return new WaitForSeconds(0.3f);
      player4.GetComponent<AutoMoveController>().JumpTo();
      }
    }
}
