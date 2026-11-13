// MainMenu.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI элементы")]
    public Button excursionButton;  // кнопка ЭКСКУРСИЯ
    public Button exitButton;       // кнопка ВЫХОД
    public Button navigatorButton;  // кнопка НАВИГАТОР
    
    void Start()
    {
        // Кнопка ЭКСКУРСИЯ -> ведет на testline
        if (excursionButton != null)
            excursionButton.onClick.AddListener(OnExcursionClick);
        
        // Кнопка ВЫХОД
        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitButtonClick);
        
        // Кнопка НАВИГАТОР -> ведет на SelectionScene
        if (navigatorButton != null)
            navigatorButton.onClick.AddListener(OnNavigatorClick);
    }
    
    // ЭКСКУРСИЯ -> загружаем сцену testline
    void OnExcursionClick()
    {
        Debug.Log("Нажата кнопка ЭКСКУРСИЯ - загрузка testline");
        SceneManager.LoadScene("testline");
    }
    
    // НАВИГАТОР -> загружаем сцену выбора локации
    public void OnNavigatorClick()
    {
        Debug.Log("Нажата кнопка НАВИГАТОР - загрузка SelectionScene");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadSelectionScene();
        }
        else
        {
            SceneManager.LoadScene("SelectionScene");
        }
    }
    
    // ВЫХОД
    void OnExitButtonClick()
    {
        Debug.Log("Выход из игры");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}