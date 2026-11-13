using UnityEngine;

public class InputTester : MonoBehaviour
{
    void Update()
    {
        // Тест всех возможных способов
        TestInputP();
        TestAlternativeKeys();
        TestAnyKey();
    }
    
    void TestInputP()
    {
        // Все способы проверки клавиши P
        if (Input.GetKeyDown(KeyCode.P))
            Debug.Log("<color=green>✓ Input.GetKeyDown(KeyCode.P) - РАБОТАЕТ!</color>");
        
        if (Input.GetKey(KeyCode.P))
            Debug.Log("P удерживается");
            
        if (Input.GetKeyUp(KeyCode.P))
            Debug.Log("P отпущена");
    }
    
    void TestAlternativeKeys()
    {
        // Проверка других клавиш
        if (Input.GetKeyDown(KeyCode.Space))
            Debug.Log("<color=yellow>Space работает</color>");
            
        if (Input.GetKeyDown(KeyCode.Return))
            Debug.Log("<color=yellow>Enter работает</color>");
            
        if (Input.GetKeyDown(KeyCode.Alpha1))
            Debug.Log("<color=yellow>Цифра 1 работает</color>");
    }
    
    void TestAnyKey()
    {
        if (Input.anyKeyDown)
        {
            string keys = "";
            foreach(KeyCode k in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(k))
                    keys += k.ToString() + ", ";
            }
            
            if (!string.IsNullOrEmpty(keys))
                Debug.Log($"<color=cyan>Нажаты: {keys}</color>");
        }
    }
    
    void OnGUI()
    {
        // Отображение состояния ввода
        GUI.color = Color.white;
        GUI.Box(new Rect(10, 10, 300, 200), "ТЕСТ ВВОДА");
        
        GUI.Label(new Rect(20, 40, 280, 30), 
            $"P нажата: {(Input.GetKey(KeyCode.P) ? "ДА" : "нет")}");
        GUI.Label(new Rect(20, 70, 280, 30), 
            $"Любая клавиша: {(Input.anyKey ? "ДА" : "нет")}");
            
        // Кнопка для теста
        if (GUI.Button(new Rect(20, 110, 260, 40), "ТЕСТ КНОПКА (кликни)"))
        {
            Debug.Log("GUI кнопка работает!");
        }
    }
}