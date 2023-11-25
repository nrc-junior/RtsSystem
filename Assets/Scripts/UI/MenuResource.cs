using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuResource : MonoBehaviour {
    Dictionary<ResourceData, FieldResource> resources;

    public void Awake(){
        FieldResource[] fields = GetComponentsInChildren<FieldResource>();
        resources = new Dictionary<ResourceData, FieldResource>(fields.Length);

        foreach (FieldResource resource in fields){
            resources.Add(resource.type, resource);     
        }
    }

    public bool UpdateOnScreen(ResourceData type){
        return resources.ContainsKey(type);
    }
    
    public void SetValue(ResourceData type, int value){
        resources[type].value.text = value.ToString();
    }
}
