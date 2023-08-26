using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Building Data", menuName = "Buildings/data", order = 1)]

public class BuildingData : ScriptableObject {
    public new string name;
    public Sprite icon;
    public string minimapIconMaterialName = "minimapDefaultTeam";
    

    public List<ResourcePrice> cost = new List<ResourcePrice>();
    public List<DeployablesData> deployables = new List<DeployablesData>();
    
    public List<int> upgradesFactory = new List<int>();
    public List<ResourceData> acceptResources = new List<ResourceData>();
    
    public GameObject prefab;
    public GameObject constructionPrefab;
    //public static Action purchase;

    public delegate void Purchase(BuildingData building);
    public Purchase purchase;
}
