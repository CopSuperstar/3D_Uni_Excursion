using UnityEngine;

public class InitDestinations : MonoBehaviour
{
    void Start()
    {
        DestinationSelectionUI selectionUI = GetComponent<DestinationSelectionUI>();
        if (selectionUI != null)
        {
            selectionUI.availableDestinations.Clear();
            
            // Добавляем цели программно
            AddDestination(selectionUI, "director", "Кабинет директора", 
                "Справка о документах и отчетах", new Vector3(10, 0, 15), "DirectorOffice");
            
            AddDestination(selectionUI, "library", "Библиотека", 
                "Учебные материалы и справочники", new Vector3(-5, 0, 20), "LibraryDesk");
            
            AddDestination(selectionUI, "it", "IT отдел", 
                "Техническая поддержка", new Vector3(15, 0, -10), "ITDepartment");
            
            AddDestination(selectionUI, "accounting", "Бухгалтерия", 
                "Финансовые справки", new Vector3(-8, 0, -12), "AccountingDesk");
            
            Debug.Log($"Добавлено {selectionUI.availableDestinations.Count} целей");
        }
    }
    
    void AddDestination(DestinationSelectionUI ui, string id, string name, string desc, Vector3 pos, string objName)
    {
        DestinationInfo dest = new DestinationInfo();
        dest.id = id;
        dest.displayName = name;
        dest.description = desc;
        dest.positionInLocation = pos;
        dest.targetObjectName = objName;
        ui.availableDestinations.Add(dest);
    }
}