using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro; // Добавь если используешь TextMeshPro

public class DestinationSelectionUI : MonoBehaviour
{
    [Header("UI элементы")]
    public Transform destinationsContainer;
    public GameObject destinationButtonPrefab;
    public Button nextButton;
    public Button backButton;
    public Text infoText;
    public Text selectedDestinationText;
    
    [Header("Настройки")]
    public List<DestinationInfo> availableDestinations = new List<DestinationInfo>();
    
    private DestinationInfo currentSelectedDestination;
    
    void Start()
    {
        Debug.Log("DestinationSelectionUI Start");
        
        // ПРИНУДИТЕЛЬНО ОЧИЩАЕМ ВСЕ СТАРЫЕ КНОПКИ
        if (destinationsContainer != null)
        {
            Debug.Log("Очищаем старые кнопки...");
            for (int i = destinationsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(destinationsContainer.GetChild(i).gameObject);
            }
        }
        
        availableDestinations.Clear();
        
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is NULL!");
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }
        
        LoadDestinations();
        SetupButtons();
        CreateDestinationButtons();
        UpdateSelectedText();
        
        if (nextButton != null)
            nextButton.interactable = false;
    }
    
    void LoadDestinations()
    {
        if (GameManager.Instance != null && GameManager.Instance.destinations.Count > 0)
        {
            foreach (var dest in GameManager.Instance.destinations)
            {
                availableDestinations.Add(dest);
                Debug.Log($"Загружена цель: {dest.displayName} -> сцена: {dest.targetSceneName}");
            }
        }
        else
        {
            CreateTestDestinations();
        }
        Debug.Log($"Всего загружено целей: {availableDestinations.Count}");
    }
    
    void CreateTestDestinations()
    {
        availableDestinations.Clear();
        
        DestinationInfo sport = new DestinationInfo();
        sport.displayName = "Кафедра физической культуры";
        sport.targetSceneName = "fizkaf";
        availableDestinations.Add(sport);
        
        DestinationInfo military = new DestinationInfo();
        military.displayName = "Военный кабинет";
        military.targetSceneName = "korpusv2";
        availableDestinations.Add(military);
        
        DestinationInfo medical = new DestinationInfo();
        medical.displayName = "Медпункт";
        medical.targetSceneName = "exc";
        availableDestinations.Add(medical);
        
        DestinationInfo accounting = new DestinationInfo();
        accounting.displayName = "Кабинет бухгалтера";
        accounting.targetSceneName = "korpusv1";
        availableDestinations.Add(accounting);
    }
    
    void SetupButtons()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextClick);
        }
        
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackClick);
        }
    }
    
    void CreateDestinationButtons()
    {
        if (destinationsContainer == null)
        {
            Debug.LogError("Destinations Container не назначен!");
            return;
        }
        
        Debug.Log($"Создаем {availableDestinations.Count} кнопок...");
        
        // Если нет префаба, создаем кнопку программно
        if (destinationButtonPrefab == null)
        {
            Debug.LogWarning("Destination Button Prefab не назначен! Создаю кнопку программно...");
            CreateButtonsProgrammatically();
            return;
        }
        
        // Создаем кнопки из префаба
        foreach (var destination in availableDestinations)
        {
            GameObject buttonObj = Instantiate(destinationButtonPrefab, destinationsContainer);
            buttonObj.name = destination.displayName;
            
            // Меняем текст на кнопке (ОБЯЗАТЕЛЬНО!)
            Text btnText = buttonObj.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                btnText.text = destination.displayName;
                Debug.Log($"Установлен текст на кнопке: {destination.displayName}");
            }
            
            // Для TextMeshPro
            TextMeshProUGUI tmpText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = destination.displayName;
                Debug.Log($"Установлен TMP текст: {destination.displayName}");
            }
            
            Button btn = buttonObj.GetComponent<Button>();
            DestinationInfo capturedDest = destination;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnDestinationSelected(capturedDest));
        }
        
        Debug.Log($"Создано {availableDestinations.Count} кнопок");
    }
    
    void CreateButtonsProgrammatically()
    {
        // Создаем кнопки без префаба
        foreach (var destination in availableDestinations)
        {
            GameObject buttonObj = new GameObject(destination.displayName);
            buttonObj.transform.SetParent(destinationsContainer);
            
            // Добавляем RectTransform
            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280, 80);
            
            // Добавляем Image для фона
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.27f, 0.55f, 0.85f);
            
            // Добавляем Button
            Button btn = buttonObj.AddComponent<Button>();
            
            // Добавляем текст
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            Text text = textObj.AddComponent<Text>();
            text.text = destination.displayName;
            text.fontSize = 20;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            DestinationInfo capturedDest = destination;
            btn.onClick.AddListener(() => OnDestinationSelected(capturedDest));
        }
    }
    
    public void OnDestinationSelected(DestinationInfo destination)
    {
        if (destination == null) return;
        
        currentSelectedDestination = destination;
        
        if (GameManager.Instance != null)
            GameManager.Instance.SelectDestination(destination);
        
        if (infoText != null)
            infoText.text = $"<b>{destination.displayName}</b>\n\n{destination.description}";
        
        UpdateSelectedText();
        
        if (nextButton != null)
            nextButton.interactable = true;
        
        Debug.Log($"Выбрано: {destination.displayName}");
    }
    
    void UpdateSelectedText()
    {
        if (selectedDestinationText != null)
        {
            if (currentSelectedDestination != null)
                selectedDestinationText.text = $"✓ Выбрано: {currentSelectedDestination.displayName}";
            else
                selectedDestinationText.text = "⚡ Выберите место для получения справки";
        }
    }
    
    void OnNextClick()
    {
        if (currentSelectedDestination == null)
        {
            if (infoText != null)
                infoText.text = "⚠️ Сначала выберите место!";
            return;
        }
        
        if (GameManager.Instance != null)
            GameManager.Instance.LoadTargetScene();
    }
    
    void OnBackClick()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LoadMainMenu();
        else
            SceneManager.LoadScene("testmenu");
    }
}