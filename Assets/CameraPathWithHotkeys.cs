using UnityEngine;

public class CameraPathWithHotkey : MonoBehaviour
{
    [Header("Точки маршрута")]
    public Transform startPoint;
    public Transform endPoint;
    
    [Header("Настройки движения")]
    public float duration = 5f;
    public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Горячие клавиши")]
    [Tooltip("Клавиша для старта движения")]
    public KeyCode startKey = KeyCode.P;
    
    [Tooltip("Клавиша для паузы")]
    public KeyCode pauseKey = KeyCode.Space;
    
    [Tooltip("Клавиша для сброса")]
    public KeyCode resetKey = KeyCode.R;
    
    [Tooltip("Показывать подсказки на экране")]
    public bool showHelp = true;
    
    [Tooltip("Начинать заново при нажатии старта во время движения")]
    public bool restartDuringMovement = true;
    
    // Состояние движения
    private enum MovementState { Idle, Moving, Paused, Finished }
    private MovementState currentState = MovementState.Idle;
    private float progress = 0f;
    
    void Update()
    {
        // Обработка ввода
        HandleInput();
        
        // Обновление движения
        UpdateMovement();
    }
    
    void HandleInput()
    {
        // Старт движения
        if (Input.GetKeyDown(startKey))
        {
            if (currentState != MovementState.Moving || restartDuringMovement)
            {
                StartMovement();
            }
        }
        
        // Пауза/продолжение
        if (Input.GetKeyDown(pauseKey))
        {
            if (currentState == MovementState.Moving)
            {
                currentState = MovementState.Paused;
                Debug.Log("Движение приостановлено");
            }
            else if (currentState == MovementState.Paused)
            {
                currentState = MovementState.Moving;
                Debug.Log("Движение продолжено");
            }
        }
        
        // Сброс
        if (Input.GetKeyDown(resetKey))
        {
            ResetCamera();
        }
    }
    
    void UpdateMovement()
    {
        if (currentState != MovementState.Moving) return;
        
        progress += Time.deltaTime / duration;
        progress = Mathf.Clamp01(progress);
        
        float curvedProgress = speedCurve.Evaluate(progress);
        
        // Движение и вращение
        transform.position = Vector3.Lerp(
            startPoint.position, 
            endPoint.position, 
            curvedProgress
        );
        
        transform.rotation = Quaternion.Lerp(
            startPoint.rotation,
            endPoint.rotation,
            curvedProgress
        );
        
        // Проверка завершения
        if (progress >= 1f)
        {
            currentState = MovementState.Finished;
            Debug.Log("Движение завершено!");
        }
    }
    
    void StartMovement()
    {
        if (startPoint == null || endPoint == null)
        {
            Debug.LogError("Назначьте начальную и конечную точки!");
            return;
        }
        
        // Сброс в начальную позицию
        transform.position = startPoint.position;
        transform.rotation = startPoint.rotation;
        
        progress = 0f;
        currentState = MovementState.Moving;
        
        Debug.Log($"Движение начато! Нажмите {pauseKey} для паузы, {resetKey} для сброса");
    }
    
    void ResetCamera()
    {
        if (startPoint != null)
        {
            transform.position = startPoint.position;
            transform.rotation = startPoint.rotation;
        }
        
        progress = 0f;
        currentState = MovementState.Idle;
        
        Debug.Log("Камера сброшена в начальное положение");
    }
    
    // Отображение подсказок на экране
    void OnGUI()
    {
        if (!showHelp) return;
        
        GUI.color = Color.white;
        GUI.backgroundColor = new Color(0, 0, 0, 0.7f);
        
        // Создаем окно с подсказками
        GUILayout.BeginArea(new Rect(20, 20, 400, 400));
        GUILayout.BeginVertical("Box");
        
        GUILayout.Label("<b>Управление камерой:</b>", new GUIStyle(GUI.skin.label) { richText = true });
        GUILayout.Space(10);
        
        GUILayout.Label($"<b>{startKey}</b> - Начать движение", new GUIStyle(GUI.skin.label) { richText = true });
        GUILayout.Label($"<b>{pauseKey}</b> - Пауза/Продолжить", new GUIStyle(GUI.skin.label) { richText = true });
        GUILayout.Label($"<b>{resetKey}</b> - Сброс в начало", new GUIStyle(GUI.skin.label) { richText = true });
        
        GUILayout.Space(10);
        GUILayout.Label($"<b>Состояние:</b> {currentState}", new GUIStyle(GUI.skin.label) { richText = true });
        GUILayout.Label($"<b>Прогресс:</b> {progress:P0}", new GUIStyle(GUI.skin.label) { richText = true });
        
        // Кнопка для скрытия/показа подсказок
        if (GUILayout.Button("Скрыть подсказки"))
        {
            showHelp = false;
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}