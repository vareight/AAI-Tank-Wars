using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 5f;
        Time.fixedDeltaTime = Time.fixedDeltaTime * Time.timeScale;
    }
}
