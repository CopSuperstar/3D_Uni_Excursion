using UnityEngine;

public class DrawBrokenLine : MonoBehaviour
{
    public Transform[] points; // Массив точек, через которые будет проходить линия
    private LineRenderer lineRenderer;

    void Start()
    {
        // Получаем компонент LineRenderer
        lineRenderer = GetComponent<LineRenderer>();

        // Настройки LineRenderer
        lineRenderer.startWidth = 0.05f; // Ширина начала линии
        lineRenderer.endWidth = 0.05f; // Ширина конца линии
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Устанавливаем материал для видимости линии
        lineRenderer.startColor = Color.red; // Цвет начала линии
        lineRenderer.endColor = Color.red; // Цвет конца линии

        

        // Добавляем коллайдер к каждой точке
        foreach (Transform point in points)
        {
            if (point.gameObject.GetComponent<Collider>() == null)
            {
                point.gameObject.AddComponent<SphereCollider>().isTrigger = true; // Добавляем триггерный коллайдер
            }
        }
    }

    void Update()
    {
        if (points.Length > 0)
        {
            // Устанавливаем количество точек в LineRenderer
            lineRenderer.positionCount = points.Length;

            // Устанавливаем позиции для LineRenderer
            for (int i = 0; i < points.Length; i++)
            {
                lineRenderer.SetPosition(i, points[i].position);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
        // Проверяем, является ли объект персонажем (например, по тегу)
        if (other.CompareTag("Player"))
        {
            
            // Пробегаемся по всем точкам и удаляем те, которые были затронуты
            for (int i = 0; i < points.Length; i++)
            {
                if (other.bounds.Intersects(points[i].GetComponent<Collider>().bounds))
                {
                    Destroy(points[i].gameObject); // Удаляем точку
                    UpdateLine(); // Обновляем линию после удаления точки
                    break; // Выходим из цикла после удаления одной точки
                }
            }
        }
    }

    private void UpdateLine()
{
    // Проверяем количество оставшихся точек
    if (points.Length <= 1)
    {
        lineRenderer.positionCount = 0; // Убираем линию, если точек больше нет
        return;
    }

    // Обновляем массив точек, убирая удаленные
    Transform[] updatedPoints = new Transform[points.Length - 1];
    int index = 0;

    for (int i = 0; i < points.Length; i++)
    {
        if (points[i] != null)
        {
            updatedPoints[index++] = points[i];
        }
    }

    points = updatedPoints;
}
}
