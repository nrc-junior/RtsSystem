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
        if(justDelivery){
            collector.data.isCollecting = true;
            collector.data.tempDelivery = true;
            collector.currentState = Unit.State.Deliverying;
            newCollectors.Add(collector);
            updatedList = true;
            return;
        }

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

    public void RemoveCollector(Unit collector) {
        collector.data.isCollecting = false;
        collector.currentState = Unit.State.Idle;
        removedCollectors.Add(collector);
        collector.RollbackBehaviour();
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

                // ----------------------- Staying in Resource to collect
                case Unit.State.Collecting: 
                    if(Time.time < unit.timer.collectNext ) continue;

                    CollectData collectorInfo = unit.role.getterData[unit.collectingIdx];
                    Resource resource = unit.currentResource;
                    
                    if(unit.DistanceTo(resource.position) > 2f){
                        unit.currentState = Unit.State.Returning;
                        continue;
                    }

                    bool belowZero = (resource.remaining - collectorInfo.getterAmount) < 0;
                    
                    int gattered = belowZero ? resource.remaining : collectorInfo.getterAmount;
                    int newUnitTotal = gattered + unit.totalCollected;
                    int overflowed = newUnitTotal > collectorInfo.maxPocket ? (newUnitTotal - collectorInfo.maxPocket) : 0;
                    
                    gattered -= overflowed;
    
                    resource.remaining -= gattered;
                    
                    if(gattered > 0){
                        unit.lastCollected = resource.data;
                    }
                    
                    if(resource.remaining <= 0){
                        unit.lastCollected = resource.data;
                        unit.currentWarehouse ??= GetNearestWarehouse(unit.curWorldPosition, unit.lastCollected);

                        if(unit.currentWarehouse){
                            unit.currentResource = GetNearestResource(unit.currentWarehouse.position, resource.data); //! change resource
                            unit.currentState = Unit.State.Returning;
                            unit.Stop();
                        }else if(unit.lastCollected == null){
                            unit.currentState = Unit.State.Idle;
                            Debug.Log("what warehouse?!", unit.gameObject);
                        }else{
                            unit.currentState = Unit.State.Idle;
                            Debug.Log("state set to null", unit.gameObject);
                        }
                        
                        if(resource != null) {
                            resource.Empty();
                        }
                    }

                    unit.totalCollected += gattered;
                    unit.timer.collectNext = Time.time + collectorInfo.getterSpeed;
                    
                    if(unit.totalCollected >= collectorInfo.maxPocket){
                        unit.currentState = Unit.State.Deliverying;
                        unit.data.movingToWarehouse = false;
                    }
                    break;

                // ----------------------- Deliverying to Warehouse
                case Unit.State.Deliverying: 

                    if (unit.currentWarehouse == null || !unit.data.movingToWarehouse) {
                        Building warehouse = GetNearestWarehouse(unit.curWorldPosition, unit.lastCollected);
                        
                        if (warehouse == null){
                            Debug.Log("state set to null", unit.gameObject);
                            unit.currentState = Unit.State.Idle;
                            unit.Stop();
                            
                            removedCollectors.Remove(unit);
                            continue;
                        }

                        unit.currentWarehouse = warehouse;
                        unit.MoveTo(warehouse.position);
                        unit.data.movingToWarehouse = true;
                        continue;
                    }
                    
                    if(unit.data.reachedJobPoint){
                        player.AddPlayerResource(unit.lastCollected, unit.totalCollected);
                        unit.data.ai.nav.isStopped = true;
                        unit.totalCollected = 0;

                        if(unit.data.tempDelivery){
                            unit.currentState = Unit.State.Idle;
                            RemoveCollector(unit);
                        }else{
                            unit.currentState = Unit.State.Returning;
                        }
                    }
                    break;

                // ----------------------- Returning to Resource
                case Unit.State.Returning: 

                    if(unit.totalCollected >= unit.role.getterData[unit.collectingIdx].maxPocket){
                        unit.currentState = Unit.State.Deliverying;
                        unit.data.movingToWarehouse = false;
                        Debug.Log("is deliverying");

                        continue;
                    }

                    if(unit.data.ai.nav.isStopped){
                        unit.currentWarehouse = null;

                        if(unit.currentResource){
                            unit.MoveTo(unit.currentResource.position);
                            unit.currentResource.collectors++;
                            unit.data.ai.nav.isStopped = false;

                        }else{
                            unit.currentWarehouse ??= GetNearestWarehouse(unit.curWorldPosition, unit.lastCollected);

                            if(unit.currentWarehouse)
                                unit.currentResource = GetNearestResource(unit.currentWarehouse.position, unit.lastCollected); //! change resource
                                
                            if(unit.currentResource == null){
                                if(unit.totalCollected == 0){
                                    unit.currentState = Unit.State.Idle;
                                    continue;
                                }
                                unit.currentState = Unit.State.Deliverying;
                                unit.data.movingToWarehouse = false;
                            }
                            continue;
                        }

                    }
                    
                    if(unit.data.reachedJobPoint){
                        if(unit.DistanceTo(unit.currentResource.position) < 2f){
                            unit.timer.collectNext = Time.time + unit.role.getterData[unit.collectingIdx].getterSpeed;
                            unit.currentState = Unit.State.Collecting;
                        }
                    }
                    break;

                {}
            }
        }
    }

    private Resource GetNearestResource(Vector3 pos, ResourceData curData){
        Collider[] cols = new Collider[100];
        Physics.OverlapSphereNonAlloc(pos, 32, cols, Resource.layer);
        
        Resource resource = null;
        List<Resource> avaiableResources = new List<Resource>();

        for (int i = 0; i < 100; i++){
            cols[i]?.TryGetComponent(out resource);
            
            if(resource && resource.data == curData && resource.remaining > 0){
                avaiableResources.Add(resource);
            }
        }

        if(avaiableResources.Count == 0) return null;

        avaiableResources.Sort((e1,e2) => {
            float distance1 = Vector3.Distance(e1.position, pos);
            float distance2 = Vector3.Distance(e2.position, pos);
            
            distance1 = Mathf.RoundToInt(distance1 / 4.0f) * 4;
            distance2 = Mathf.RoundToInt(distance2 / 4.0f) * 4;

            if (distance1 != distance2)
            {
                // Sort by distance first
                return distance1.CompareTo(distance2);
            }
            else
            {
                // If distances are equal, sort by count
                return e1.collectors.CompareTo(e2.collectors);
            }
        });  

        return avaiableResources[0];
    }
    
    private Building GetNearestWarehouse(Vector3 pos, ResourceData resourceType){
        List<Building> availableWarehouses = buildings.FindAll(b => b.data.acceptResources.Contains(resourceType));
        
        if(availableWarehouses.Count == 0) return null;

        availableWarehouses.Sort((B1,B2) => Vector3.Distance(B1.position, pos).CompareTo(Vector3.Distance(B2.position, pos)));  
        return availableWarehouses[0];
    }
}


[System.Serializable]
public class PlayerResource{
    public ResourceData type;
    public int quantity = 0;
}
