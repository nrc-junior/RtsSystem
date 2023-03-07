using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class RTSPlayer : MonoBehaviour {
    public int team = 1;
    public Menu menu;
    public List<PlayerResource> resources = new List<PlayerResource>();

    private Dictionary<ResourceData, PlayerResource> resRef = new Dictionary<ResourceData, PlayerResource>();
   
    private ScreenSelector selectingManager;

    public ResourceManager resourceManager { get; set; }
    public UnitsManager unitsManager { get; set; }
    public SpendSystem spendManager {get; set;}

    public class Data{
        public Resource hoveredResource;
        public Building hoveredBuilding;
    }

    public static Data data = new Data();

    protected virtual void Awake() {

        resourceManager = gameObject.AddComponent<ResourceManager>();
        resourceManager.player = this;

        selectingManager = GetComponent<ScreenSelector>();
        selectingManager.buildMenu = menu.buildLayout;
        
        unitsManager = GetComponent<UnitsManager>();
        unitsManager.player = this;
        unitsManager.selector = selectingManager;
        spendManager = GetComponent<SpendSystem>();


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
