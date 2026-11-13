using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class DestinationInfo
{
    public string id;
    public string displayName;
    public string description;
    public Sprite icon;
    public Vector3 positionInLocation;
    public string targetObjectName;
    public string targetSceneName;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public string mainMenuSceneName = "testmenu";
    public string selectionSceneName = "SelectionScene";
    
    public List<DestinationInfo> destinations = new List<DestinationInfo>();
    
    private DestinationInfo selectedDestination;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (destinations.Count == 0)
        {
            CreateDestinations();
        }
    }
    
    void CreateDestinations()
    {
        destinations.Clear();
        
        DestinationInfo sport = new DestinationInfo();
        sport.displayName = "Кафедра физической культуры";
        sport.targetSceneName = "fizkaf"; // ← ПРОВЕРЬ ИМЯ!
        destinations.Add(sport);
        
        DestinationInfo military = new DestinationInfo();
        military.displayName = "Военный кабинет";
        military.targetSceneName = "korpusv2";
        destinations.Add(military);
        
        DestinationInfo medical = new DestinationInfo();
        medical.displayName = "Медпункт";
        medical.targetSceneName = "exc";
        destinations.Add(medical);
        
        DestinationInfo accounting = new DestinationInfo();
        accounting.displayName = "Кабинет бухгалтера";
        accounting.targetSceneName = "korpusv1";
        destinations.Add(accounting);
        
        Debug.Log("=== ЛОКАЦИИ СОЗДАНЫ ===");
        foreach (var d in destinations)
        {
            Debug.Log($"{d.displayName} -> {d.targetSceneName}");
        }
    }
    
    public void SelectDestination(DestinationInfo destination)
    {
        selectedDestination = destination;
        Debug.Log($"Выбрано: {destination.displayName} -> {destination.targetSceneName}");
    }
    
    public void LoadMainMenu()
    {
        Debug.Log("Загрузка: testmenu");
        SceneManager.LoadScene("testmenu");
    }
    
    public void LoadSelectionScene()
    {
        Debug.Log("Загрузка: SelectionScene");
        SceneManager.LoadScene("SelectionScene");
    }
    
    public void LoadTargetScene()
    {
        if (selectedDestination != null)
        {
            Debug.Log($"Загрузка: {selectedDestination.targetSceneName}");
            SceneManager.LoadScene(selectedDestination.targetSceneName);
        }
        else
        {
            Debug.LogError("Ничего не выбрано!");
        }
    }
}