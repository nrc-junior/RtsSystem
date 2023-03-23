using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour {
    public int team;
    public BuildingData data;
    public Vector3 position {get; set;}
    
    bool isBuild = false;
    Transform completeBuilding;
    Transform buildingPhase;

    public void Awake(){
        
        LateSetup();
    }
    
    public void Setup(BuildingData data, RTSPlayer player){
        this.data = data;
        team = player.team;

        completeBuilding = new GameObject ("Complete Building").transform;
        Transform[] transforms = GetComponentsInChildren<Transform>();

        completeBuilding.SetParent(transform);
        completeBuilding.localPosition = Vector3.zero;

        // if(data.constructionPrefab){
        //     for (int i = 1; i < transforms.Length; i++){
        //         transforms[i].SetParent(completeBuilding, true);
        //     }

        //     completeBuilding.gameObject.SetActive(false);

        //     buildingPhase = GameObject.Instantiate(data.constructionPrefab).transform;
        //     buildingPhase.SetParent(transform);
        //     buildingPhase.localPosition = Vector3.zero;
        //     buildingPhase.gameObject.SetActive(true);
        // }else{
            isBuild = true;
        // }
    }

    void LateSetup(){
        position = transform.position;
    }

    protected virtual void OnMouseEnter(){
        HandleCursor.SetCursor(CursorType.Collect);
        RTSPlayer.data.hoveredBuilding = this;
        Debug.Log("hovering" + data.name);
    }

    protected virtual void OnMouseExit(){
        HandleCursor.Clear();
        RTSPlayer.data.hoveredBuilding = null;
    }
}
