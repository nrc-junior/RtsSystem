using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Resource", menuName = "Resources/resource", order = 1)]
public class ResourceData : ScriptableObject {
    public new string name;
    public int quantity;
    public string minimapIconMaterialName = "minimapDefaultResource";
    public GameObject[] gatherMeshEvolution;
    
}
