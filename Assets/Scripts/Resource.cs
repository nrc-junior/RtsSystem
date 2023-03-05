using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Resource : MonoBehaviour{
    public ResourceData data;

    public Vector3 location {get; set;}
    public int remaining {get; set;}
    void Awake(){

        if(data == null){
            DisableResource();
            return;
        } 

        remaining = remaining != 0 ? remaining : data.quantity;
        
        if(remaining == 0){
            DisableResource();
            return;
        }

        if(data.gatherMeshEvolution.Length > 0){
            // data.quantity / quantityLeft
        }
        
        LateSetup();
    }

    void LateSetup(){
        location = transform.position;
    }

    public void DisableResource(){
        Destroy(gameObject);
    }

    protected virtual void OnMouseEnter(){
        HandleCursor.SetCursor(CursorType.Collect);
        RTSPlayer.data.hoveredResource = this;
    }

    protected virtual void OnMouseExit(){
        HandleCursor.Clear();
        RTSPlayer.data.hoveredResource = null;
    }

}
