using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour {
    public int team;
    public static Dictionary<int, List<Building>> teamBuildings;
    protected static Dictionary<int, List<Collider>> teamColliders;

    public BuildingData data;
    public List<DeployablesData> deployeds = new List<DeployablesData>();

    public Vector3 position {get; set;}
    
    bool isBuild = false;
    Transform completeBuilding;
    Transform buildingPhase;

    public Action SELECTED;
    public Action UNSELECT;
    
    public Bounds bounds {get; private set;}

    public void Awake(){
        position = transform.position;
        RegisterBuilding();
        bounds = GetMaxBounds();
    }
    void Start(){
        MinimapIcon minimapIcon = gameObject.AddComponent<MinimapIcon>();
        minimapIcon.Initialize(data.minimapIconMaterialName);

    }
    public void Setup(BuildingData data, MainManagerRTS player){
        this.data = data;
        team = player.team;

        completeBuilding = new GameObject ("Complete Building").transform;
        Transform[] transforms = GetComponentsInChildren<Transform>();

        completeBuilding.SetParent(transform);
        completeBuilding.localPosition = Vector3.zero;

        isBuild = true;
    }

    void RegisterBuilding(){
        if(teamBuildings == null){
            teamBuildings = new Dictionary<int, List<Building>>();
            teamColliders = new Dictionary<int, List<Collider>>();
        }

        if(!teamBuildings.ContainsKey(team)){
            teamBuildings.Add(team, new List<Building>());
            teamColliders.Add(team, new List<Collider>());
        }
        
        teamBuildings[team].Add(this);
        teamColliders[team].Add(GetComponent<Collider>());
    }

    protected virtual void OnMouseEnter(){
        HandleCursor.SetCursor(CursorType.Collect);
        MainManagerRTS.data.hoveredBuilding = this;
    }

    protected virtual void OnMouseExit(){
        HandleCursor.Clear();
        MainManagerRTS.data.hoveredBuilding = null;
    }

    
    Bounds GetMaxBounds() {
        Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        
        foreach (Collider collider in colliders)
        {
            bounds.Encapsulate(collider.bounds);
        }
        
        return bounds;
    }

}
