
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitsManager : MonoBehaviour {
    [System.Serializable]
    public class Formation{
        public int  width;
        public int  height;
        public float spacing;

        public Formation(){
            width = 0;
            height = 0;
        }
    }
    public MainManagerRTS player {private get; set;}
    public ScreenSelector selector {private get; set;}

    public List<Unit> unitGroup;

    private Camera cam;
    bool controlling;

    public Formation formation;
    public bool showUnitPopUp;
    
    public class Inputs{
        public bool clickRMB;
        public bool holdingRMB;
        
        const bool clickAfterReleaseHold = true;
        const short holdFrameCount = 15;

        float holdRMBTime;
        float timeHeldingRMB;

        public void Update(){
            clickRMB = false;
            holdingRMB = false;

            if(Input.GetMouseButton(1)){
                holdRMBTime += Time.deltaTime;
            }else{
                timeHeldingRMB = holdRMBTime;
                holdRMBTime = 0;
            } 

            if(holdRMBTime >= Time.deltaTime * holdFrameCount){
                holdingRMB = true;

            }else if (timeHeldingRMB > 0){
                clickRMB = clickAfterReleaseHold ? true : timeHeldingRMB < Time.deltaTime * holdFrameCount; 
                holdingRMB = false;
            }
        }
    }

    UnitsManager.Inputs input = new(); 
     

    void Start(){
        cam = Camera.main;
        selector.SELECTED += onSelect;
        selector.UNSELECTED += onUnselect;
    }

    void Update(){
        if(!controlling) return;
        input.Update();

        if(input.clickRMB){
            MoveUnits(Input.mousePosition);
        }

        // if(input.holdingRMB){
        //     player.inventory.radialMenu.Open();
        // }
    }

    void MoveUnits(Vector2 mousePos){
        Ray ray = cam.ScreenPointToRay(mousePos);
        
        if(Physics.Raycast(ray, out RaycastHit hit )){
            Vector3 center = hit.point;
            
            Formation formationData = new Formation(); 

            Resource selectedResource = MainManagerRTS.data.hoveredResource;
            Building selectedBuilding = MainManagerRTS.data.hoveredBuilding;
            Deployable selectedDeployable = MainManagerRTS.data.hoveredDeployable;

            foreach (var unit in unitGroup){
                if(unit == null ) continue;
                
                CollectData colletableData = null;

                if(unit.goingToDeployable && !selectedDeployable){
                    unit.REACHED -= unit.goingToDeployable.OnReachDeployable;
                    unit.goingToDeployable = null;
                }

                if(selectedDeployable){
                    if(unit.goingToDeployable){
                        unit.REACHED -= unit.goingToDeployable.OnReachDeployable;
                        unit.goingToDeployable = null;
                    }

                    unit.MoveTo(selectedDeployable.position);
                    unit.REACHED += selectedDeployable.OnReachDeployable;
                    unit.goingToDeployable = selectedDeployable;
                }




                if(selectedResource){
                    colletableData = unit.role.getterData.Find(x => x.resource == selectedResource.data);
                }

                if(unit.data.isCollecting){
                    player.resourceManager.RemoveCollector(unit);
                }

                unit.RecivePlayerOrder();

                if(colletableData != null){
                    unit.collectingIdx = unit.role.getterData.IndexOf(colletableData);
                    unit.currentResource = selectedResource;
                    player.resourceManager.AddCollector(unit);
                    continue;
                }
                
                if(unit.totalCollected > 0 && selectedBuilding){
                    if(selectedBuilding.data.acceptResources.Contains(unit.collectedType)){
                        Debug.Log("tenta fazer delivery de " + unit.collectedType);
                        unit.currentWarehouse = selectedBuilding;
                        unit.MoveTo(selectedBuilding.position);
                        unit.data.movingToWarehouse = true;
                        player.resourceManager.AddCollector(unit, true);

                        continue;
                    }
                }


                if(formationData.width < formation.width){
                    formationData.width++;
                }else{
                    formationData.width = 1;
                    formationData.height++;
                }

                Vector3 destination = center + PositionInGroup(formationData);
                unit.MoveTo(destination);
            }
        }
    }

    // diagonal formation example:
    // 1. UnitPos (0,0) width: 0, height: 0
    // 2. UnitPos (1,1) width: 1, height: 1
    Vector3 PositionInGroup(Formation unitFormation){
        Vector3 localPosition = Vector3.zero;
        localPosition.x = formation.spacing * unitFormation.width;
        localPosition.z = formation.spacing * unitFormation.height;
        return localPosition;
    }

    void onSelect(List<Unit> selecteds){
        unitGroup = selecteds;
        selecteds.ForEach(s => s.DIED += OnUnitDie);
        controlling = true;
    }

    void OnUnitDie(Unit deadUnit){
        deadUnit.UNSELECT?.Invoke(deadUnit);
        unitGroup.Remove(deadUnit);
    }

    void onUnselect(){
        unitGroup.Clear();
        controlling = false;
    }
}
