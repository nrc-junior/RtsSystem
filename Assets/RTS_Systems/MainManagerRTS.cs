using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

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

    HashSet<Unit> selectedUnits = new HashSet<Unit>();
    HashSet<Building> selectedBuildings = new HashSet<Building>();
    public Action<List<Unit>> SELECTED;
    
    public Inventory inventory;

    protected virtual void Awake() {

        resourceManager = gameObject.AddComponent<ResourceManager>();
        resourceManager.player = this;

        selectingManager.BEGIN += ClearSelection;
        
        selectingManager.END += FindUnitsInSelection;

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
        if(selectingManager.selecting){
            FindUnitsInSelection();
        }

        Building[] buildings = GameObject.FindObjectsOfType<Building>();
        
        foreach (var item in buildings){
            if(item.team == team)
            resourceManager.AddWarehouse(item);
        }
    }


    void FindUnitsInSelection(){
        if(!Unit.teamUnits.ContainsKey(team)) return;

        List<Unit> units = Unit.teamUnits[team];
        List<Building> buildings = Building.teamBuildings[team];

        foreach (var unit in units) {
            CheckUnitInSelection(unit);
        }

        foreach (var building in buildings) {
            CheckBuildingInSelection(building);
        }   
        
        if(selectedUnits.Count > 0 ){
            unitsManager.Select(selectedUnits.ToList());
        }
    }

    void CheckUnitInSelection(Unit unit){
        if(selectingManager.IntersectBound(unit.mainCollider.bounds)){
            if(selectedUnits.Contains(unit)) return;
            selectedUnits.Add(unit);
            unit.SELECTED?.Invoke(unit);

        } else if (selectedUnits.Contains(unit)) {
            unit.UNSELECT?.Invoke(unit);
            selectedUnits.Remove(unit);
        }
    }

    void CheckBuildingInSelection(Building build){
        if(selectingManager.IntersectBound(build.bounds)){
            if(selectedBuildings.Contains(build)) return;
            selectedBuildings.Add(build);
            build.SELECTED?.Invoke();

        } else if (selectedBuildings.Contains(build)) {
            build.UNSELECT?.Invoke();
            selectedBuildings.Remove(build);
        }
    }

    void ClearSelection(){
        if(selectedUnits.Count > 0 ){
            selectedUnits.ToList().ForEach(unit => unit.UNSELECT.Invoke(unit));
            selectedUnits.Clear();
            unitsManager.Unselect();
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
