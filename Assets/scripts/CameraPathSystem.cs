using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PathConfig
{
    [Header("Настройки движения")]
    [Range(0.5f, 60f)] public float travelTime = 10f;
    public AnimationCurve speedProfile = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Поведение камеры")]
    public bool smoothRotation = true;
    [Range(0.1f, 10f)] public float rotationSpeed = 3f;
    public bool lookForward = false;
    
    [Header("Управление камерой")]
    public bool enableCameraControl = true;
    public float mouseSensitivity = 2f;
    public float touchSensitivity = 1.5f;
    public bool invertY = false;
    public bool autoReturnRotation = false; 
    
    [Header("Тип цикла")]
    public LoopType loopType = LoopType.PingPong;
    public bool pauseAtEndpoints = false;
    public float pauseDuration = 1f;
    
    [Header("Тип пути")]
    public PathMode pathMode = PathMode.Linear;
    public float curveStrength = 0.5f;
}

public enum PathMode
{
    Linear, Smooth, Bezier
}

public enum LoopType
{
    None, Repeat, PingPong, PingPongOnce
}

public class CameraPathSystem : MonoBehaviour
{
    [Header("Точки пути")]
    public List<Transform> pathPoints = new List<Transform>();
    
    [Header("Настройки")]
    public PathConfig config = new PathConfig();
    
    [Header("Состояние")]
    [SerializeField] private bool isActive = false;
    [SerializeField] private float timer = 0f;
    [SerializeField] private int currentSegment = 0;
    [SerializeField] private float segmentProgress = 0f;
    [SerializeField] private bool movingForward = true;
    [SerializeField] private bool isPaused = false;
    [SerializeField] private float pauseTimer = 0f;
    
    private Quaternion currentRotation;
    private bool pathReady = false;
    private int currentWaypointIndex = 0;
    
    // Для управления камерой
    private float rotationX = 0f;
    private float rotationY = 0f;
    private bool isCameraControlled = false;
    private Quaternion freeLookRotation;
    
    // Для телефона
    private bool isTouching = false;
    
    void Start()
    {
        SetupPath();
        SetupCameraControl();
    }
    
    void SetupCameraControl()
    {
        rotationX = transform.eulerAngles.y;
        rotationY = transform.eulerAngles.x;
        currentRotation = transform.rotation;
        freeLookRotation = transform.rotation;
        
        if (!Application.isMobilePlatform)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    void Update()
    {
        if (config.enableCameraControl)
        {
            HandleInput();
        }
        
        if (!isActive || !pathReady || pathPoints.Count < 2) return;
        
        if (isPaused)
        {
            HandlePause();
        }
        else
        {
            MoveCamera();
        }
    }
    
    void HandleInput()
    {
        bool inputDetected = false;
        
        //УПРАВЛЕНИЕ ДЛЯ ПК 
        if (!Application.isMobilePlatform)
        {
            if (Input.GetMouseButton(1))
            {
                inputDetected = true;
                isCameraControlled = true;
                
                float mouseX = Input.GetAxis("Mouse X") * config.mouseSensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * config.mouseSensitivity;
                
                rotationX += mouseX;
                rotationY += config.invertY ? mouseY : -mouseY;
                rotationY = Mathf.Clamp(rotationY, -80f, 80f);
                
                freeLookRotation = Quaternion.Euler(rotationY, rotationX, 0);
                transform.rotation = freeLookRotation;
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }
        
        //  УПРАВЛЕНИЕ ДЛЯ ТЕЛЕФОНА 
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    isTouching = true;
                    inputDetected = true;
                    isCameraControlled = true;
                    break;
                    
                case TouchPhase.Moved:
                    if (isTouching)
                    {
                        inputDetected = true;
                        Vector2 delta = touch.deltaPosition;
                        
                        float touchX = delta.x * config.touchSensitivity * 0.5f;
                        float touchY = delta.y * config.touchSensitivity * 0.5f;
                        
                        rotationX += touchX;
                        rotationY += config.invertY ? touchY : -touchY;
                        rotationY = Mathf.Clamp(rotationY, -80f, 80f);
                        
                        freeLookRotation = Quaternion.Euler(rotationY, rotationX, 0);
                        transform.rotation = freeLookRotation;
                    }
                    break;
                    
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isTouching = false;
                    break;
            }
        }
        
        
        if (!inputDetected && isCameraControlled)
        {
            
            currentRotation = transform.rotation;
        }
        
        // Обновляем currentRotation если камера не управляется
        if (!isCameraControlled)
        {
            currentRotation = transform.rotation;
        }
    }
    
    void SetupPath()
    {
        if (pathPoints.Count < 2)
        {
            CreateDefaultPath();
        }
        
        currentRotation = transform.rotation;
        pathReady = true;
        
        if (pathPoints.Count > 0)
        {
            transform.position = pathPoints[0].position;
            transform.rotation = pathPoints[0].rotation;
            
            Vector3 euler = transform.eulerAngles;
            rotationX = euler.y;
            rotationY = euler.x;
            freeLookRotation = transform.rotation;
        }
    }
    
    void CreateDefaultPath()
    {
        pathPoints.Clear();
        
        GameObject start = new GameObject("Path_Start");
        start.transform.position = transform.position;
        start.transform.rotation = transform.rotation;
        pathPoints.Add(start.transform);
        
        for (int i = 1; i <= 3; i++)
        {
            GameObject point = new GameObject($"Path_Point_{i}");
            point.transform.position = transform.position + 
                transform.forward * (i * 5f) + 
                Vector3.up * 2f + 
                (i % 2 == 0 ? Vector3.right * 3f : Vector3.left * 3f);
            point.transform.rotation = Quaternion.LookRotation(
                (i < 3 ? pathPoints[i-1].position : transform.position) - point.transform.position
            );
            pathPoints.Add(point.transform);
        }
    }
    
    void MoveCamera()
    {
        if (config.loopType == LoopType.None && timer >= config.travelTime)
        {
            FinishMovement();
            return;
        }
        
        timer += Time.deltaTime;
        float totalProgress = CalculateTotalProgress();
        float smoothProgress = config.speedProfile.Evaluate(totalProgress);
        
        UpdateCurrentSegment(smoothProgress);
        
        Vector3 targetPosition = CalculatePosition(smoothProgress);
        transform.position = targetPosition;
        
        
        if (!isCameraControlled)
        {
            // Только если игрок не крутит камеру, применяем автоматическое вращение
            if (config.smoothRotation && !config.autoReturnRotation)
            {
                // Легкое следование по пути, но не принудительное
                FollowPathDirection(targetPosition);
            }
        }
        else
        {
            // Если игрок управляет - сохраняем его вращение
            transform.rotation = freeLookRotation;
        }
    }
    
    void FollowPathDirection(Vector3 currentPos)
    {
        // Очень плавное следование, только если разница большая
        if (config.lookForward && currentWaypointIndex < pathPoints.Count - 1)
        {
            Vector3 lookTarget = pathPoints[currentWaypointIndex + 1].position;
            Vector3 direction = lookTarget - currentPos;
            
            if (direction != Vector3.zero && Vector3.Angle(transform.forward, direction) > 30f)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                currentRotation = Quaternion.Slerp(currentRotation, targetRot, 
                    Time.deltaTime * 0.5f);
                transform.rotation = currentRotation;
                
                // Обновляем свободный взгляд
                Vector3 euler = transform.eulerAngles;
                rotationX = euler.y;
                rotationY = euler.x;
                freeLookRotation = transform.rotation;
            }
        }
    }
    
    float CalculateTotalProgress()
    {
        if (config.loopType == LoopType.None)
        {
            return Mathf.Clamp01(timer / config.travelTime);
        }
        else if (config.loopType == LoopType.Repeat)
        {
            return timer / config.travelTime;
        }
        else
        {
            float pingPongTime = timer / (config.travelTime * 2f);
            
            if (config.loopType == LoopType.PingPongOnce && pingPongTime >= 1f)
            {
                return 1f;
            }
            
            return pingPongTime - Mathf.Floor(pingPongTime);
        }
    }
    
    void UpdateCurrentSegment(float totalProgress)
    {
        if (pathPoints.Count < 2) return;
        
        if (config.loopType == LoopType.PingPong || config.loopType == LoopType.PingPongOnce)
        {
            float pingPongValue = Mathf.PingPong(totalProgress * (pathPoints.Count - 1), pathPoints.Count - 1);
            currentSegment = Mathf.FloorToInt(pingPongValue);
            segmentProgress = pingPongValue - currentSegment;
            movingForward = (Mathf.FloorToInt(totalProgress * (pathPoints.Count - 1) * 2) % 2 == 0);
        }
        else
        {
            int segments = pathPoints.Count - 1;
            float segmentSize = 1f / segments;
            
            int rawSegment = Mathf.FloorToInt(totalProgress * segments);
            currentSegment = rawSegment % segments;
            segmentProgress = (totalProgress - (currentSegment * segmentSize)) / segmentSize;
            segmentProgress = Mathf.Clamp01(segmentProgress);
            movingForward = true;
        }
        
        if (segmentProgress < 0.5f)
        {
            currentWaypointIndex = currentSegment;
        }
        else
        {
            currentWaypointIndex = currentSegment + 1;
        }
        
        CheckForPause();
    }
    
    void CheckForPause()
    {
        if (!config.pauseAtEndpoints || isPaused) return;
        
        if (currentWaypointIndex == 0 && segmentProgress < 0.01f && movingForward == false)
        {
            StartPause();
        }
        else if (currentWaypointIndex == pathPoints.Count - 1 && segmentProgress > 0.99f && movingForward == true)
        {
            StartPause();
        }
    }
    
    void StartPause()
    {
        isPaused = true;
        pauseTimer = 0f;
    }
    
    void HandlePause()
    {
        pauseTimer += Time.deltaTime;
        
        if (pauseTimer >= config.pauseDuration)
        {
            isPaused = false;
            pauseTimer = 0f;
        }
    }
    
    Vector3 CalculatePosition(float totalProgress)
    {
        if (pathPoints.Count < 2) return transform.position;
        
        switch (config.pathMode)
        {
            case PathMode.Smooth:
                return GetSmoothPosition(totalProgress);
            case PathMode.Bezier:
                return GetBezierPosition(currentSegment, segmentProgress);
            default:
                return GetLinearPosition(currentSegment, segmentProgress);
        }
    }
    
    Vector3 GetLinearPosition(int segment, float t)
    {
        if (segment < 0 || segment >= pathPoints.Count - 1) 
            return transform.position;
            
        int pointA = segment;
        int pointB = segment + 1;
        
        if (!movingForward && config.loopType == LoopType.PingPong)
        {
            pointA = segment + 1;
            pointB = segment;
        }
        
        return Vector3.Lerp(
            pathPoints[pointA].position,
            pathPoints[pointB].position,
            movingForward ? t : 1f - t
        );
    }
    
    Vector3 GetSmoothPosition(float t)
    {
        int pointCount = pathPoints.Count;
        if (pointCount < 4) return GetLinearPosition(currentSegment, segmentProgress);
        
        float adjustedT = movingForward ? t : 1f - t;
        float scaledT = adjustedT * (pointCount - 1);
        int index = Mathf.FloorToInt(scaledT);
        float localT = scaledT - index;
        
        index = Mathf.Clamp(index, 1, pointCount - 3);
        
        Vector3 p0 = pathPoints[index - 1].position;
        Vector3 p1 = pathPoints[index].position;
        Vector3 p2 = pathPoints[index + 1].position;
        Vector3 p3 = pathPoints[index + 2].position;
        
        return CalculateSpline(p0, p1, p2, p3, localT, config.curveStrength);
    }
    
    Vector3 GetBezierPosition(int segment, float t)
    {
        if (segment < 0 || segment >= pathPoints.Count - 1) 
            return transform.position;
            
        Vector3 p0, p1, p2, p3;
        
        if (movingForward)
        {
            p0 = pathPoints[segment].position;
            p1 = p0 + pathPoints[segment].forward * 2f;
            p2 = pathPoints[segment + 1].position - pathPoints[segment + 1].forward * 2f;
            p3 = pathPoints[segment + 1].position;
        }
        else
        {
            p0 = pathPoints[segment + 1].position;
            p1 = p0 - pathPoints[segment + 1].forward * 2f;
            p2 = pathPoints[segment].position + pathPoints[segment].forward * 2f;
            p3 = pathPoints[segment].position;
        }
        
        return CalculateBezier(p0, p1, p2, p3, movingForward ? t : 1f - t);
    }
    
    Vector3 CalculateSpline(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, float tension)
    {
        float tt = t * t;
        float ttt = tt * t;
        
        float q1 = -tension * ttt + 2f * tension * tt - tension * t;
        float q2 = (2f - tension) * ttt + (tension - 3f) * tt + 1f;
        float q3 = (tension - 2f) * ttt + (3f - 2f * tension) * tt + tension * t;
        float q4 = tension * ttt - tension * tt;
        
        return q1 * p0 + q2 * p1 + q3 * p2 + q4 * p3;
    }
    
    Vector3 CalculateBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1 - t;
        float uu = u * u;
        float uuu = uu * u;
        float tt = t * t;
        float ttt = tt * t;
        
        return uuu * p0 + 3f * uu * t * p1 + 3f * u * tt * p2 + ttt * p3;
    }
    
    void FinishMovement()
    {
        isActive = false;
        
        if (config.loopType == LoopType.PingPong || config.loopType == LoopType.PingPongOnce)
        {
            transform.position = pathPoints[0].position;
        }
        else
        {
            transform.position = pathPoints[pathPoints.Count - 1].position;
        }
    }
    
    // ПУБЛИЧНЫЕ МЕТОДЫ
    
    [ContextMenu("Старт движения")]
    public void BeginMovement()
    {
        if (pathPoints.Count < 2)
        {
            SetupPath();
        }
        
        timer = 0f;
        currentSegment = 0;
        segmentProgress = 0f;
        currentWaypointIndex = 0;
        movingForward = true;
        isPaused = false;
        pauseTimer = 0f;
        isActive = true;
        currentRotation = transform.rotation;
        isCameraControlled = false;
        
        Debug.Log($"Движение начато: {pathPoints.Count} точек");
    }
    
    [ContextMenu("Стоп")]
    public void StopMoving() => isActive = false;
    
    [ContextMenu("Продолжить")]
    public void ContinueMoving()
    {
        if (!isActive) isActive = true;
    }
    
    [ContextMenu("Вернуться в начало")]
    public void GoToStart()
    {
        isActive = false;
        timer = 0f;
        currentSegment = 0;
        segmentProgress = 0f;
        currentWaypointIndex = 0;
        movingForward = true;
        isPaused = false;
        isCameraControlled = false;
        
        if (pathPoints.Count > 0)
        {
            transform.position = pathPoints[0].position;
            transform.rotation = pathPoints[0].rotation;
        }
    }
    
    [ContextMenu("Добавить точку")]
    public void AddPointFromCurrent()
    {
        GameObject point = new GameObject($"Point_{pathPoints.Count}");
        point.transform.position = transform.position;
        point.transform.rotation = transform.rotation;
        pathPoints.Add(point.transform);
        Debug.Log($"Добавлена точка #{pathPoints.Count}");
    }
    
    void ClearAllPoints()
    {
        foreach (Transform point in pathPoints)
        {
            if (point != null)
            {
                if (Application.isPlaying) Destroy(point.gameObject);
                else DestroyImmediate(point.gameObject);
            }
        }
        pathPoints.Clear();
    }
    
    public bool IsMoving => isActive;
    public bool IsPaused => isPaused;
    public float Progress => timer / config.travelTime;
    public int CurrentPoint => currentWaypointIndex;
    public int TotalPoints => pathPoints.Count;
    public bool IsMovingForward => movingForward;
    
    void OnDrawGizmos()
    {
        if (pathPoints == null || pathPoints.Count < 2) return;
        
        for (int i = 0; i < pathPoints.Count; i++)
        {
            if (pathPoints[i] == null) continue;
            
            Gizmos.color = i == 0 ? Color.green : i == pathPoints.Count - 1 ? Color.red : Color.yellow;
            Gizmos.DrawSphere(pathPoints[i].position, 0.25f);
        }
        
        Gizmos.color = Color.cyan;
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            if (pathPoints[i] != null && pathPoints[i + 1] != null)
            {
                Gizmos.DrawLine(pathPoints[i].position, pathPoints[i + 1].position);
            }
        }
        
        if (Application.isPlaying && isActive)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
}