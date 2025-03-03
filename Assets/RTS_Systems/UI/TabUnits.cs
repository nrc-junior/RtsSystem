using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabUnits : MonoBehaviour {

    public int team {get; set;}
    [SerializeField] PrefUnitIcon pref;

    Dictionary<UnitRoleData, PrefUnitIcon> selecteds = new Dictionary<UnitRoleData, PrefUnitIcon>();

    void Awake(){
        Unit.SETUP += OnSetupUnit;
        pref.gameObject.SetActive(false);
    }

    void OnSetupUnit(Unit unit){
        unit.SELECTED += OnUnitSelected;
        unit.UNSELECT += OnUnitDeselected;
    }

    public void OnUnitSelected (Unit unit){
        if(unit.data.team != team) return;
        
        UnitRoleData role = unit.role;
        
        if(selecteds.ContainsKey(role)){
            selecteds[role].AddUnit(); 
            selecteds[role].name.text = role.name;
        }else{
            PrefUnitIcon newIcon = GameObject.Instantiate(pref, pref.transform.parent);
            selecteds.Add(unit.role, newIcon);
            selecteds[role].AddUnit(); 
            selecteds[role].name.text = role.name;
            newIcon.gameObject.SetActive(true);
        }

    }

    public void OnUnitDeselected (Unit unit){
        if(unit.data.team != team) return;

        UnitRoleData role = unit.role;
        
        if(selecteds.ContainsKey(role)){
            
            selecteds[role].RemUnit(); 
            
            if(selecteds[role].countNmbr == 0){
                Destroy(selecteds[role].gameObject);
                selecteds.Remove(role);
            }

        }
    }


}
