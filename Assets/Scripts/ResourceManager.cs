using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour {
    private readonly List<Unit> collectors = new List<Unit>();
    private readonly List<Building> buildings = new List<Building>();
    
    public RTSPlayer player {protected get; set;}
    
    List<Unit> newCollectors = new List<Unit>();
    List<Unit> removedCollectors = new List<Unit>();
    public bool updatedList {private get; set;}

    public void AddCollector(Unit collector, bool justDelivery = false) {
        collector.data.isCollecting = true;
        collector.currentState = Unit.State.Returning;
        collector.behaviour = Unit.Behaviour.TrackResources;

        newCollectors.Add(collector);
        
        if(collector.totalCollected > 0 && collector.collectedType != collector.currentResource.data){
            collector.totalCollected = 0;
        }

        collector.collectedType = collector.currentResource.data;
        
        updatedList = true;
    }

    public void RemoveCollector(Unit miner) {
        miner.currentState = Unit.State.Idle;
        removedCollectors.Add(miner);
        miner.RollbackBehaviour();
        updatedList = true;
    }

    public void AddWarehouse(Building warehouse) {
        buildings.Add(warehouse);
    }

    public void Update() {
       
        if(updatedList){
            removedCollectors.ForEach(u => collectors.Remove(u));
            newCollectors.ForEach(u => collectors.Add(u));
            
            removedCollectors.Clear();
            newCollectors.Clear();
            updatedList = false;
        }

        foreach (var unit in collectors) {
            switch (unit.currentState) {

                case Unit.State.Collecting: // * BEGIN COLLECT * //
                    if(Time.time < unit.timer.collectNext ) continue;

                    CollectData collectorInfo = unit.role.getterData[unit.collectingIdx];
                    Resource resource = unit.currentResource;
                    
                    bool belowZero = (resource.remaining - collectorInfo.getterAmount) < 0;
                    
                    int gattered = belowZero ? resource.remaining : collectorInfo.getterAmount;
                    int newUnitTotal = gattered + unit.totalCollected;
                    int overflowed = newUnitTotal > collectorInfo.maxPocket ? (newUnitTotal - collectorInfo.maxPocket) : 0;
                    
                    gattered -= overflowed;
    
                    resource.remaining -= gattered;
                    
                    if(resource.remaining <= 0){
                        resource.Empty();
                    }

                    unit.totalCollected += gattered;
                    unit.timer.collectNext = Time.time + collectorInfo.getterSpeed;
                    
                    if(unit.totalCollected >= collectorInfo.maxPocket){
                        unit.currentState = Unit.State.Deliverying;
                    }
                    break;

                case Unit.State.Deliverying: // * BEGIN DELIVERY * //

                    if (unit.currentWarehouse == null || !unit.data.movingToWarehouse) {
                        Building warehouse = GetNearestWarehouse(unit.curWorldPosition, unit.currentResource);
                        
                        if (warehouse == null){
                            unit.currentState = Unit.State.Idle;
                            unit.Stop();
                            
                            removedCollectors.Remove(unit);
                            continue;
                        }
                       
                        Debug.Log(warehouse.name);

                        unit.currentWarehouse = warehouse;
                        unit.MoveTo(warehouse.location);
                        unit.data.movingToWarehouse = true;
                        continue;
                    }
                    
                    if(unit.data.reachedJobPoint){
                        Debug.Log(unit.name + "chegou no delivery point");
                        player.AddPlayerResource(unit.currentResource.data, unit.totalCollected);
                        unit.data.ai.nav.isStopped = true;
                        unit.currentState = Unit.State.Returning;
                        unit.totalCollected = 0;
                    }
                    break;

                case Unit.State.Returning: // * RETURN TO RESOURCE * //
                    if(unit.currentResource){

                    }

                    if(unit.totalCollected >= unit.role.getterData[unit.collectingIdx].maxPocket){
                        unit.currentState = Unit.State.Deliverying;
                        unit.data.movingToWarehouse = false;
                        Debug.Log("is deliverying");

                        continue;
                    }

                    if(unit.data.ai.nav.isStopped){
                        Debug.Log("is stopped");
                        unit.currentWarehouse = null;
                        unit.MoveTo(unit.currentResource.location);
                        unit.data.ai.nav.isStopped = false;
                    }
                    
                    if(unit.data.reachedJobPoint){
                        Debug.Log(unit.name + "voltou a minerar");
                        unit.timer.collectNext = Time.time + unit.role.getterData[unit.collectingIdx].getterSpeed;
                        unit.currentState = Unit.State.Collecting;
                    }
                    break;

                {}
            }
        }
    }

    private Resource GetNearestOre(Vector3 pos){
        return null;
    }
    
    private Building GetNearestWarehouse(Vector3 pos, Resource resource){
        List<Building> availableWarehouses = buildings.FindAll(b => b.data.acceptResources.Contains(resource.data));
        
        if(availableWarehouses.Count == 0) return null;

        availableWarehouses.Sort((B1,B2) => Vector3.Distance(B1.location, pos).CompareTo(Vector3.Distance(B2.location, pos)));  
        return availableWarehouses[0];
    }
}


[System.Serializable]
public class PlayerResource{
    public ResourceData type;
    public int quantity = 0;
}
