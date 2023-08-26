using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class MenuBuilding : MonoBehaviour {
    public RTSPlayer player;
    public int team {get; set;}
    
    [SerializeField] Text txBuildingName;
    [SerializeField] RectTransform deployableList;
    [SerializeField] PrefDeployableBtn prefDeployable;

    List<PrefDeployableBtn> currentDeployables = new List<PrefDeployableBtn>();
    public delegate void DeployClassBox(PrefDeployableBtn deployed);

    void Awake(){
        prefDeployable.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    void OnDisable(){ 
        Clear();
    }

    public void ShowBuildingHud(Building building){
        if(isActiveAndEnabled){
            Clear();
        }else{
            gameObject.SetActive(true);
        }

        txBuildingName.text = building.data.name;
        DeployClassBox callback = Deploy;

        foreach (DeployablesData deploy in building.data.deployables){
            
            PrefDeployableBtn btnDeployable = GameObject.Instantiate(prefDeployable, deployableList);
            bool isActive = !building.deployeds.Contains(deploy);

            btnDeployable.SetupButton(isActive, callback, deploy);
            currentDeployables.Add(btnDeployable);
        }

        RefreshUI();

        // local methods

        void Deploy(PrefDeployableBtn deploy){
            GameObject deployable = GameObject.Instantiate(Resources.Load(deploy.data.resourceDeployable) as GameObject);
            player.PLACE_OBJECT?.Invoke(deployable);

            player.CONFIRM_PLACE += OnDeployPlace;
            player.CANCEL_PLACE += OnDeployCanceled;

            deploy.Disable();
            gameObject.SetActive(false);

            //local methods

            void OnDeployPlace(){
                player.CONFIRM_PLACE -= OnDeployPlace;
                building.deployeds.Add(deploy.data);
                deployable.AddComponent<Deployable>().Setup(deploy.data);
            }

            void OnDeployCanceled(){
                player.CANCEL_PLACE -= OnDeployCanceled;
                deploy.Enable();
            }
        }
    }
    
    void RefreshUI(){
        var layouts = GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>();

        for(int i = 0; i < layouts.Length; i++){
            if(layouts[i].TryGetComponent<RectTransform>(out var panel)){
                LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
            }
        }
    }
    
    void Clear(){
        foreach (var deployable in currentDeployables){
            DestroyImmediate(deployable.gameObject);
        }
        currentDeployables.Clear();
    }

    // todo refatorar:

    // public SpendSystem playerEconomy;

    // public Transform content;
    // public FieldBtnImage buildingPrefab;
    // List<FieldBtnImage> currentFields = new List<FieldBtnImage>();
    
    // public bool isShowing {get; set;} 
    
    public void ShowUI(List<BuildingData> unitBuildings){
        // if(isShowing){
        //     Clear();
        // }

        // foreach (var buildData in unitBuildings){
        //     FieldBtnImage field = Instantiate(buildingPrefab);
        //     field.SetCost(buildData.cost.ToArray());
            
        //     field.onPurchase = PurchaseBuilding;
        //     field.item = buildData;

        //     field.icon.sprite = buildData.icon;
        //     field.transform.SetParent(content);
        //     field.CLICK += OnClickThumbnail;
            
        //     currentFields.Add(field);
        // }

        // isShowing = true;
        // gameObject.SetActive(true);
    }


    GameObject placing;
    BuildingData placingData;
    public void PurchaseBuilding(object buildable){

        // placingData = buildable as BuildingData;
        // placing = GameObject.Instantiate(placingData.prefab);
        // player.PLACE_OBJECT?.Invoke(placing);
        // player.CONFIRM_PLACE += OnPlacedBuilding;
        // player.CANCEL_PLACE += RefundPlayer;
        // Debug.Log("Comprou " + placingData.name);
    }

    void RefundPlayer(){

    }

    void OnPlacedBuilding(){
        // placing.SetActive(true);
        // placing.AddComponent<Building>().Setup(placingData, player);

    }


    // public void Clear(){
    //     // currentFields.ForEach(o => {
    //     //     o.CLICK -= OnClickThumbnail;
    //     //     Destroy(o.gameObject);
    //     // });
    //     // currentFields.Clear();
    //     // gameObject.SetActive(false);
    //     // isShowing = false;
    // }


    public void OnClickThumbnail(FieldBtnImage field){
        // if(placingData != null) return;

        // if(playerEconomy.CanAfford(field.totalPrice)){
        //     playerEconomy.Pay(field.totalPrice);
        //     field.onPurchase?.Invoke(field.item);
        // }
    }

}
