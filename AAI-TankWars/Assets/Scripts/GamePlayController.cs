using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayController : MonoBehaviour
{
  /*  private float fixedDeltaTime;
    private void Awake()
    {
     this.fixedDeltaTime = Time.fixedDeltaTime * Time.timeScale;
    }*/
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1f;
        //Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;
    }
}
