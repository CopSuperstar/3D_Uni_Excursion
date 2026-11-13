using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneskip : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ExitGame()
    {
        Debug.Log("exit");
        Application.Quit();
    }
    public void LosdMenu()
    {
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }
    
}
