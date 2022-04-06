using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public int difficulty = 1;

    public Slider slider;

    public Canvas levelMenu;

    public Canvas mainMenu;

    void Start()
    {
        mainMenu = GetComponent<Canvas>();
        slider.onValueChanged.AddListener((value) =>
        {
            difficulty = (int)value;
        });
    }

    public void PlayRandom()
    {
        Grid.levelType = 0;
        RayCastShooter.difficulty = difficulty;
        SceneManager.LoadScene("BubbleScene");
    }

    public void PlayLevel(int level)
    {
        Grid.levelType = level;
        RayCastShooter.difficulty = difficulty;
        SceneManager.LoadScene("BubbleScene");
    }

    public void GoToLevelMenu()
    {
        levelMenu.enabled = true;
        mainMenu.enabled = false;
    }

    public void GoToMainMenu()
    {
        mainMenu.enabled = true;
        levelMenu.enabled = false;
    }
}
