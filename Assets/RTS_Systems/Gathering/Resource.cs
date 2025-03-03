using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Resource : MonoBehaviour {
    public static int layer = 1 << 8;

    public ResourceData data;

    public Vector3 position {get; set;}
    public int remaining {get; set;}
    public int collectors {get; set;}
    public Action<Resource> EMPTY;

    protected virtual void Awake(){

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

    void Start(){
        MinimapIcon minimapIcon = gameObject.AddComponent<MinimapIcon>();
        minimapIcon.Initialize(data.minimapIconMaterialName);

    }

    void LateSetup(){
        position = transform.position;
    }

    public void Empty(){
        if(EMPTY == null){
            DisableResource();
        }else{
           EMPTY.Invoke(this);
        } 
    }

    public void DisableResource(){
        Destroy(gameObject);
    }

    protected virtual void OnMouseEnter(){
        HandleCursor.SetCursor(CursorType.Collect);
        MainManagerRTS.data.hoveredResource = this;
    }

    protected virtual void OnMouseExit(){
        HandleCursor.Clear();
        MainManagerRTS.data.hoveredResource = null;
    }

}
