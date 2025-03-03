using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuResource : MonoBehaviour {
    bool setup = false;
    Dictionary<ResourceData, FieldResource> resources;

    public void Awake(){
        FieldResource[] fields = GetComponentsInChildren<FieldResource>();
        resources = new Dictionary<ResourceData, FieldResource>(fields.Length);

        foreach (FieldResource resource in fields){
            resources.Add(resource.type, resource);     
        }
        setup = true;
    }

    public bool UpdateOnScreen(ResourceData type){
        return resources.ContainsKey(type);
    }
    
    public void SetValue(ResourceData type, int value){
        if(!setup){
            Awake();
        }
        
        resources[type].value.text = value.ToString();
    }
}
