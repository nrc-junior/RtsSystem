using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Building Data", menuName = "Buildings/data", order = 1)]

public class BuildingData : ScriptableObject {
    public new string name;
    public Sprite icon;
    
    public ResourcePrice cost;
    public List<UnitRoleData> unitsFactory = new List<UnitRoleData>();
    
    public List<ResourceData> acceptResources = new List<ResourceData>();
    // todo skills factory
}
