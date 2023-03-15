using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBuilding : MonoBehaviour {
    public int team {get; set;}
    public SpendSystem playerEconomy;
    public RTSPlayer player;

    public Transform content;
    public FieldBtnImage buildingPrefab;
    List<FieldBtnImage> currentFields = new List<FieldBtnImage>();
    
    public bool isShowing {get; set;} 

    public void ShowUI(List<BuildingData> unitBuildings){
        if(isShowing){
            Clear();
        }

        foreach (var buildData in unitBuildings){
            FieldBtnImage field = Instantiate(buildingPrefab);
            field.SetCost(buildData.cost.ToArray());
            
            field.onPurchase = PurchaseBuilding;
            field.item = buildData;

            field.icon.sprite = buildData.icon;
            field.transform.SetParent(content);
            field.CLICK += OnClickThumbnail;
            
            currentFields.Add(field);
        }

        isShowing = true;
        gameObject.SetActive(true);
    }


    GameObject placing;
    public void PurchaseBuilding(object buildable){

        BuildingData data = buildable as BuildingData;
        placing = GameObject.Instantiate(data.prefab);
        player.PLACE_OBJECT?.Invoke(placing);
        player.CONFIRM_PLACE += OnPlacedBuilding;
        player.CANCEL_PLACE += RefundPlayer;
        Debug.Log("Comprou " + data.name);
    }

    void RefundPlayer(){

    }

    void OnPlacedBuilding(){
        placing.SetActive(true);
    }


    public void Clear(){
        currentFields.ForEach(o => {
            o.CLICK -= OnClickThumbnail;
            Destroy(o.gameObject);
        });
        currentFields.Clear();
        gameObject.SetActive(false);
        isShowing = false;
    }


    public void OnClickThumbnail(FieldBtnImage field){
        if(playerEconomy.CanAfford(field.totalPrice)){
            playerEconomy.Pay(field.totalPrice);
            field.onPurchase?.Invoke(field.item);
        }
    }

}
