using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static bool paused = false;

    public Canvas pauseMenu;
    
    public Canvas gameCanvas;

    public Text score;
    public Text shots;

    private static int scoreValue = 0;

    private static int shotsValue = -1;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PauseGame()
    {
        paused = true;
        pauseMenu.enabled = true;
        gameCanvas.enabled = false;
        Time.timeScale = 0;
    }


    public void ContinueGame()
    {
        pauseMenu.enabled = false;
        gameCanvas.enabled = true;
        StartCoroutine(Unpause());
        Time.timeScale = 1;
    }
    
    public void RestartGame()
    {
        paused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        scoreValue = 0;
        shotsValue = -1;
        ContinueGame();     
    }

    public void GoToMainMenu()
    {
        paused = false;
        StartCoroutine(Unpause());
        scoreValue = 0;
        shotsValue = -1;
        SceneManager.LoadScene("Menu");
        ContinueGame();
    }

    public void AddScore(int balls)
    {
        scoreValue += balls * 100;
        score.text = "Score: " + scoreValue;
    }

    public void AddShot()
    {
        shotsValue++;
        shots.text = "Shots: " + shotsValue;
    }

    IEnumerator Unpause()
    {
        yield return new WaitForSeconds(0.2f);
        paused = false;
    }
}
