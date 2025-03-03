using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> used to Units change classes </summary>
public class Deployable : MonoBehaviour {
    public DeployablesData data {get; private set;}
    public Vector3 position {get; private set;}
    
    public int remainingClasses = 10;

    public void Setup(DeployablesData data){
        this.data = data;
        position = transform.position;
    }

    protected virtual void OnMouseEnter(){
        MainManagerRTS.data.hoveredDeployable = this;
    }

    protected virtual void OnMouseExit(){
        MainManagerRTS.data.hoveredDeployable = null;
    }

    public void OnReachDeployable(Unit unit){
        unit.REACHED -= OnReachDeployable;
        unit.goingToDeployable = null;
        
        if(unit.lastDeployable){
            // * devolve no remaing classes lá...
        }

        remainingClasses--;
        unit.SetRole(data.deployRole);
        
        // StartCoroutine(BeginTookClass());

        // IEnumerator BeginTookClass(){
        //     yield return new WaitForSeconds(1);
        // }

    }

}
