using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[DefaultExecutionOrder(1000)]
public class MainManagerRTS : MonoBehaviour {
    [SerializeField] ScreenSelector selectingManager;
    
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

    public ResourceManager resourceManager { get; set; }
    public UnitsManager unitsManager { get; set; }
    public SpendSystem spendManager {get; set;}

    public Action<GameObject> PLACE_OBJECT;
    public Action CANCEL_PLACE;
    public Action CONFIRM_PLACE;

    public Inventory inventory;

    protected virtual void Awake() {

        resourceManager = gameObject.AddComponent<ResourceManager>();
        resourceManager.player = this;

        selectingManager.buildMenu = menu.buildLayout;

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
        
        inventory.Setup(this);

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

        if(!resRef.ContainsKey(type)){
            PlayerResource newGattered = resources.Find(r => r.type == type);
            
            if(newGattered == null){ // player encontrou novo recurso
                newGattered ??= new(type);
                resources.Add(newGattered);
                
                if(inventory.IsBackpackOpen)
                    inventory.ShowInInventory(newGattered);
            }
            
            resRef.Add(type, newGattered);
        }

        resRef[type] += amount;
        
        if(menu.resourceLayout.UpdateOnScreen(type))
        menu.resourceLayout.SetValue(type, resRef[type].quantity);
    }
}
