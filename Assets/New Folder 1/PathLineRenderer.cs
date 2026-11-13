using UnityEngine;
using System.Collections.Generic;

public class PathLineRenderer : MonoBehaviour
{
    [Header("Настройки линии")]
    public GameObject targetObject; // Целевой объект
    public Material lineMaterial;   // Материал для линии
    public Color lineColor = Color.red; // Цвет линии
    public float lineWidth = 0.1f;     // Толщина линии
    public float touchDistance = 0.5f; // Дистанция соприкосновения с точкой
    
    [Header("Точки излома линии")]
    public List<Vector3> bendPoints = new List<Vector3>(); // Точки излома линии
    
    private LineRenderer lineRenderer;
    private bool isLineVisible = true;
    private List<bool> activePoints; // Отслеживание активных точек
    
    void Start()
    {
        // Создаем и настраиваем LineRenderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        
        // Настройка внешнего вида линии
        lineRenderer.material = lineMaterial;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        
        // ВАЖНО: Устанавливаем использование мировых координат
        lineRenderer.useWorldSpace = true;
        
        // Инициализируем список активных точек
        InitializeActivePoints();
        
        // Устанавливаем количество точек линии
        UpdateLineRendererPoints();
    }
    
    void InitializeActivePoints()
    {
        activePoints = new List<bool>();
        for (int i = 0; i < bendPoints.Count; i++)
        {
            activePoints.Add(true); // Все точки активны изначально
        }
    }
    
    void Update()
    {
        // Проверяем достиг ли герой цели
        if (targetObject != null && isLineVisible)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetObject.transform.position);
            
            // Если герой близко к цели - скрываем линию
            if (distanceToTarget < 1.5f)
            {
                HideLine();
                return;
            }
            
            // Проверяем соприкосновение с точками излома
            CheckPointCollisions();
            
            // Показываем линию если она была скрыта и игрок отошел от цели
            if (!lineRenderer.enabled)
            {
                ShowLine();
            }
            
            // Обновляем позиции точек линии
            UpdateLinePositions();
        }
        else if (targetObject == null)
        {
            // Если целевой объект не назначен, скрываем линию
            HideLine();
        }
    }
    
    void CheckPointCollisions()
    {
        if (bendPoints.Count == 0) return;
        
        for (int i = 0; i < bendPoints.Count; i++)
        {
            // Если точка уже не активна, пропускаем
            if (!activePoints[i]) continue;
            
            // Проверяем расстояние до точки
            float distanceToPoint = Vector3.Distance(transform.position, bendPoints[i]);
            
            if (distanceToPoint < touchDistance)
            {
                // Деактивируем точку
                activePoints[i] = false;
                Debug.Log($"Точка {i} деактивирована!");
                
                // Обновляем LineRenderer
                UpdateLineRendererPoints();
            }
        }
    }
    
    void UpdateLineRendererPoints()
    {
        if (lineRenderer == null) return;
        
        // Считаем количество активных точек
        int activeCount = GetActivePointsCount();
        
        // +2 потому что: начальная точка (герой) и конечная точка (цель)
        lineRenderer.positionCount = activeCount + 2;
    }
    
    void UpdateLinePositions()
    {
        if (lineRenderer == null || !isLineVisible) return;
        
        int pointIndex = 0;
        
        // Первая точка - позиция главного героя
        lineRenderer.SetPosition(pointIndex++, transform.position);
        
        // Промежуточные активные точки излома
        for (int i = 0; i < bendPoints.Count; i++)
        {
            if (activePoints[i]) // Только активные точки
            {
                lineRenderer.SetPosition(pointIndex++, bendPoints[i]);
            }
        }
        
        // Последняя точка - позиция целевого объекта
        if (targetObject != null)
        {
            lineRenderer.SetPosition(pointIndex, targetObject.transform.position);
        }
    }
    
    int GetActivePointsCount()
    {
        int count = 0;
        for (int i = 0; i < activePoints.Count; i++)
        {
            if (activePoints[i]) count++;
        }
        return count;
    }
    
    void HideLine()
    {
        isLineVisible = false;
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }
    
    // Метод для показа линии (если нужно будет снова показать)
    public void ShowLine()
    {
        isLineVisible = true;
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
        }
    }
    
    // Метод для сброса всех точек (если нужно восстановить линию)
    public void ResetAllPoints()
    {
        for (int i = 0; i < activePoints.Count; i++)
        {
            activePoints[i] = true;
        }
        UpdateLineRendererPoints();
        Debug.Log("Все точки восстановлены!");
    }
    
    // Визуализация точек излома в редакторе
    void OnDrawGizmosSelected()
    {
        if (bendPoints.Count > 0)
        {
            // Рисуем сферы в точках излома
            for (int i = 0; i < bendPoints.Count; i++)
            {
                // Разный цвет для активных и неактивных точек
                if (Application.isPlaying && activePoints != null && activePoints.Count > i && !activePoints[i])
                {
                    Gizmos.color = Color.gray; // Неактивная точка
                }
                else
                {
                    Gizmos.color = Color.yellow; // Активная точка
                }
                
                Gizmos.DrawSphere(bendPoints[i], 0.2f);
            }
            
            // Рисуем линии между точками
            Gizmos.color = Color.green;
            if (targetObject != null)
            {
                Vector3 lastPoint = transform.position;
                bool hasActivePoints = false;
                
                // Рисуем линии только к активным точкам
                for (int i = 0; i < bendPoints.Count; i++)
                {
                    if (!Application.isPlaying || activePoints == null || activePoints.Count <= i || activePoints[i])
                    {
                        Gizmos.DrawLine(lastPoint, bendPoints[i]);
                        lastPoint = bendPoints[i];
                        hasActivePoints = true;
                    }
                }
                
                // Линия к цели
                if (hasActivePoints || bendPoints.Count == 0)
                {
                    Gizmos.DrawLine(lastPoint, targetObject.transform.position);
                }
            }
            
            // Рисуем зону соприкосновения вокруг героя
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, touchDistance);
        }
    }
    
    void OnDestroy()
    {
        if (lineRenderer != null)
        {
            Destroy(lineRenderer);
        }
    }
}