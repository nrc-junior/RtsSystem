using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour {
    public int team;
    public BuildingData data;
    public Vector3 location {get; set;}

    public void Awake(){
        
        LateSetup();
    }
    
    void LateSetup(){
        location = transform.position;
    }

    protected virtual void OnMouseEnter(){
        HandleCursor.SetCursor(CursorType.Collect);
        RTSPlayer.data.hoveredBuilding = this;
        Debug.Log("hovering" + data.name);
    }

    protected virtual void OnMouseExit(){
        HandleCursor.Clear();
        RTSPlayer.data.hoveredBuilding = null;
    }
}
