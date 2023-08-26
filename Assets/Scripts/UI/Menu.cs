using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour {
    public static Menu instance;    
    public MenuBuilding buildLayout;
    public MenuResource resourceLayout;
    public TabUnits tabUnits;
    public TabBuildings tabBuildings;

    UIDisabledClick[] uiElements;

    void Awake(){
        instance = this;
        uiElements = GetComponentsInChildren<UIDisabledClick>(true);
    }

    public bool IsClickingUI(){

        foreach(UIDisabledClick ui in uiElements){
            if(ui.isHovering){
                Debug.Log(ui.name);
                return true;
            }
        }
        return false;
    }
    
}
