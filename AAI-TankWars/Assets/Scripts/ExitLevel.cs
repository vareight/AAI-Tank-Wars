using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitLevel : MonoBehaviour
{
    GameManager gameManager;
    //public LayerMask playerMask;

    //public int tanksInLevel = 5;
    //public int tanksKilled;
    //public bool playerKilled;
    public GameObject[] enemyTanks;
    public GameObject player;

    private void Awake()
    {
        //DontDestroyOnLoad(this);
        gameManager = FindObjectOfType<GameManager>();
    }

    /*private void OnTriggerEnter2D(Collider2D collision)
    {
        if(((1 << collision.gameObject.layer) & playerMask) != 0)
        {
            gameManager.SaveData();
            gameManager.LoadNextLevel();
        }
    }*/

    private void Update()
    {
        enemyTanks = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemyTanks.Length == 0)
        {
            gameManager.SaveData();
            gameManager.LoadNextLevel();
        }

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            gameManager.Restart();
            SceneManager.LoadScene("Menu");
        }
    }

}
