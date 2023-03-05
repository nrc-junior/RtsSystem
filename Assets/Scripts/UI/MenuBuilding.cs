using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBuilding : MonoBehaviour {
    public Transform content;
    public FieldBtnImage buildingPrefab;
    List<GameObject> currentFields = new List<GameObject>();
    
    public bool isShowing {get; set;} 
    
    public void ShowUI(List<BuildingData> unitBuildings){
        if(isShowing){
            Clear();
        }

        foreach (var buildData in unitBuildings){
            FieldBtnImage field = Instantiate(buildingPrefab);
            field.SetCost(buildData.cost);
            field.icon.sprite = buildData.icon;
            field.transform.SetParent(content);
            
            currentFields.Add(field.gameObject);
        }

        isShowing = true;
        gameObject.SetActive(true);
    }

    public void Clear(){
        currentFields.ForEach(o => Destroy(o));
        currentFields.Clear();
        gameObject.SetActive(false);
        isShowing = false;
    }
}
