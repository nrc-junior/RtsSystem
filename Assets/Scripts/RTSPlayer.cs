using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[DefaultExecutionOrder(1000)]
public class RTSPlayer : MonoBehaviour {
    
    public class Data{
        public Resource hoveredResource;
        public Building hoveredBuilding;
        public Deployable hoveredDeployable;
    }

    public static Data data = new Data();
    public int team = 1;

    public Menu menu;
    public List<PlayerResource> resources = new List<PlayerResource>();

    public Dictionary<ResourceData, PlayerResource> resRef = new Dictionary<ResourceData, PlayerResource>();
    private ScreenSelector selectingManager;

    public ResourceManager resourceManager { get; set; }
    public UnitsManager unitsManager { get; set; }
    public SpendSystem spendManager {get; set;}

    public Action<GameObject> PLACE_OBJECT;
    public Action CANCEL_PLACE;
    public Action CONFIRM_PLACE;

    protected virtual void Awake() {

        resourceManager = gameObject.AddComponent<ResourceManager>();
        resourceManager.player = this;

        selectingManager = GetComponent<ScreenSelector>();
        selectingManager.buildMenu = menu.buildLayout;
        
        selectingManager.player = this;
        PLACE_OBJECT += selectingManager.OnBeginPlaceObject;

        unitsManager = GetComponent<UnitsManager>();
        unitsManager.player = this;
        unitsManager.selector = selectingManager;
        
        spendManager = GetComponent<SpendSystem>();
        spendManager.playerBag = resRef;
        spendManager.player = this;

        menu.buildLayout.player = this;
        menu.buildLayout.team = team;
        // ! menu.buildLayout.playerEconomy = spendManager;
        menu.tabUnits.team = team;
        
        SetupResources();
        LateSetup();
    }

    void SetupResources(){
        resources.ForEach(r => {
                resRef.Add(r.type, r);
                menu.resourceLayout.SetValue(r.type, r.quantity);
            }
        );
    }

    void LateSetup(){
        Building[] buildings = GameObject.FindObjectsOfType<Building>();
        
        foreach (var item in buildings){
            if(item.team == team)
            resourceManager.AddWarehouse(item);
        }
    }

    public void AddPlayerResource(ResourceData type, int amount){
        resRef[type].quantity += amount;
        menu.resourceLayout.SetValue(type, resRef[type].quantity);
    }
}
