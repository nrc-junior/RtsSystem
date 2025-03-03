using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ResourceType {
 R,G,B
}

public class Inventory : MonoBehaviour {
    MainManagerRTS player;
    // --- *** --- //
    public static Inventory instance;
    static List<GameObject> showing = new();

    [SerializeField] TextDisplay pickedTextPrefab;
    public float gatteredTextHideTime;
    
    Transform itemPickLayout;
    
    [Header("Backpack")]
    [SerializeField] GameObject backpack;
    public RadialUI radialMenu;
    
    public InventoryCell[] cells;
    int lastIdxFound = -1;
    public bool IsBackpackOpen{ get => backpack.activeInHierarchy; }
    
    void Awake(){
        instance = this;
        itemPickLayout = pickedTextPrefab.transform.parent;
        pickedTextPrefab.gameObject.SetActive(false);
        backpack.SetActive(false);
    }

    public void Setup(MainManagerRTS player){
        this.player = player;
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.Tab)){
            backpack.SetActive(!backpack.activeInHierarchy);
            if(backpack.activeInHierarchy){
                CleanCells();
                OpenBackpack();
            }else{
                CloseBackpack();
            }
        }
    }

    void CleanCells(){
        lastIdxFound = -1;
        foreach (var cell in cells){
            cell.Clear();
        }
    }

    void OpenBackpack(){
        player.resources.ForEach(resource => ShowInInventory(resource));
    }

    void CloseBackpack(){

    }
    
    public void ShowInInventory(PlayerResource resource){
        if(resource.quantity == 0){
            resource.Change += OnRefilledAlreadyContainedResource;
            return;
        }

        int fav = resource.favoriteInventoryIndex;
        if(fav != -1){
            if(cells[fav].storing == null){
                cells[fav].SetResource(resource);
            }else{
                fav = -1;
            }
        }

        if(fav == -1){
            int idx = FindNextIndexAvaiable();
            resource.favoriteInventoryIndex = idx;
            cells[idx].SetResource(resource);
        }
    
        void OnRefilledAlreadyContainedResource(){
            if(resource.quantity > 0) // ! AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
            resource.Change -= OnRefilledAlreadyContainedResource;
            ShowInInventory(resource);
        }
    }


    
    int FindNextIndexAvaiable(){
        InventoryCell cell;
        int i = ++lastIdxFound;
        
        while (true){
            cell = cells[i];
            
            if(cell.storing == null){
                lastIdxFound = i;
                return i;
            }

            if(++i >= cells.Length)
            break;
        }

        lastIdxFound++;
        
        return -1;
    }

    // *** ------ Inventory Static Calls ------ *** //

    public static void Show(ResourceData type, int add, int total){
        if(instance == null) return;
        
        TextDisplay obj = GameObject.Instantiate(instance.pickedTextPrefab, instance.itemPickLayout);
        obj.label.text = $"+{add} {type.name}({total})";
        obj.gameObject.SetActive(true);
        obj.StartCoroutine(ShowAndKill());
        
        IEnumerator ShowAndKill(){
            yield return new WaitForSeconds(1);
            LeanTween.value(obj.gameObject,1,0,instance.gatteredTextHideTime).setOnUpdate(x => obj.SetAlpha(x)).setOnComplete(() => Destroy(obj.gameObject));
        }
    }

    
}
