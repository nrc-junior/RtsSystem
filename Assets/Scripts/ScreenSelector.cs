using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;

public class ScreenSelector : MonoBehaviour {
    
    public RTSPlayer player {get; set;}
    public LayerMask terrainLayer;

    public RectTransform cursor;
    public RectTransform rect;
    public Canvas canvas;
    bool selecting;
    Camera cam;
    Transform camTransform;

    Vector2 startPoint;
    Vector3 start3dPoint;

    Vector2 endPoint;
    Vector3 end3dPoint;
    Vector2 negativeAnchor;

    public Transform selection3d;
    Collider selectCollider;
    
    public int team;

    HashSet<Building> selectedBuildings = new HashSet<Building>();
    HashSet<Unit> selectedUnits = new HashSet<Unit>();

    public Action<List<Unit>> SELECTED;
    public Action UNSELECTED;
    
    public MenuBuilding buildMenu {protected get; set;}
    bool isClickingOnUI;

    bool isPlacing;
    Vector3 prefabGround;
    
    MeshRenderer[] placeholderMeshes;

    bool isValid;
    Material validMaterial;
    Material invalidMaterial;

    Transform prefab;
    Transform placeholder;
    // Validator[] validators = new Validator[4];
    Validator[][] validatorPairs;
    List<Material> placeholderMaterials = new List<Material>();

    [SerializeField] LayerMask ignoreRaycastLayer;
    void Awake(){
        cam = Camera.main;
        camTransform = cam.transform;
        selectCollider = selection3d.GetComponent<Collider>();

        validMaterial = Resources.Load<Material>("Materials/placeholderOk");
        invalidMaterial = Resources.Load<Material>("Materials/placeholderFail");
    }


    void LateUpdate() {
        Vector2 mPosScreenSpace = Input.mousePosition;
        Vector2 mPosCanvasSpace = mPosScreenSpace / canvas.scaleFactor;

        cursor.anchoredPosition = mPosCanvasSpace;
        if(isPlacing){
            PlacingPrefab(mPosCanvasSpace);
            return;
        }
        

        if(Input.GetMouseButtonDown(0)){
            if(Menu.instance.IsClickingUI()){
                isClickingOnUI = true;
                return;
            }

            if(RTSPlayer.data.hoveredBuilding){
                // player.menu.buildLayout.gameObject.SetActive(true);
                player.menu.buildLayout.ShowBuildingHud(RTSPlayer.data.hoveredBuilding);
            }else{
                player.menu.buildLayout.gameObject.SetActive(false);
            }

            if(RTSPlayer.data.hoveredDeployable){
                // player.menu.buildLayout.gameObject.SetActive(true);
                //* menuzim
                // player.menu.buildLayout.ShowBuildingHud(RTSPlayer.data.hoveredBuilding);
            }else{
                // player.menu.buildLayout.gameObject.SetActive(false);
            }

            selecting = true;
            Begin2DSelection(mPosCanvasSpace);
            Begin3DSelection(mPosCanvasSpace);
            HandleCursor.SetCursor(CursorType.Rect);

        } else if(Input.GetMouseButtonUp(0)){
            if(isClickingOnUI){
                isClickingOnUI = false;
                return;
            }
            
            HandleCursor.Clear();
            
            selecting = false;
            End2DSelection(mPosCanvasSpace);
            End3DSelection(mPosScreenSpace);
        }

        if(selecting){
            Resize2DRect(mPosCanvasSpace);
            Resize3DBox(mPosScreenSpace);
            FindUnitsInSelection();
        }
    }

    void Begin2DSelection(Vector2 mP){
        rect.gameObject.SetActive(true);
        startPoint = mP;
        rect.anchoredPosition = mP;
    }

    void End2DSelection(Vector2 mP){
        rect.gameObject.SetActive(false);
        endPoint = mP;
    }

    void Resize2DRect(Vector2 mp){
        Vector2 dst = new Vector2(mp.x - startPoint.x, mp.y - startPoint.y); 
        negativeAnchor = startPoint;

        if(dst.x < 0){
            dst.x = startPoint.x - mp.x;
            negativeAnchor.x = mp.x;
            rect.anchoredPosition = negativeAnchor;

        }
        
        if(dst.y < 0){
            dst.y = startPoint.y - mp.y;
            negativeAnchor.y = mp.y; 
            rect.anchoredPosition = negativeAnchor;

        }

        rect.sizeDelta = dst;
    }

    void Begin3DSelection(Vector2 mousePos){
        Ray ray = cam.ScreenPointToRay(mousePos);

        if(Physics.Raycast(ray, out RaycastHit hit, 100, terrainLayer)){
            start3dPoint = hit.point;
            start3dPoint.y = 0;
        }

        selection3d.gameObject.SetActive(true);
        selection3d.position = start3dPoint;
        
        ClearSelection();
    }
    
    void End3DSelection(Vector2 mousePos){
        Ray ray = cam.ScreenPointToRay(mousePos);
        
        if(Physics.Raycast(ray, out RaycastHit hit, 100, 1 << 3 )){
            end3dPoint = hit.point;
        }

        FindUnitsInSelection();

        if(selectedUnits.Count > 0 ){
            SELECTED?.Invoke(selectedUnits.ToList());
        }
        selection3d.gameObject.SetActive(false);
    }
    
    void Resize3DBox(Vector2 mousePos){
        Ray ray = cam.ScreenPointToRay(mousePos);

        if(Physics.Raycast(ray, out RaycastHit hit, 100, 1 << 3  )){

            Vector3 cursorPoint = hit.point;

            Vector3 dst = cursorPoint - start3dPoint;
            dst.y = 0;

            Vector3 selectionCenter;
            Vector3 selectionScale = Vector3.one;
             
            
            selectionCenter = start3dPoint + (dst / 2);
            selectionScale = dst;
            selectionCenter.y = 1;
            selectionScale.y = 5;

            selection3d.position = selectionCenter;
            selection3d.localScale = selectionScale;
        }
    }
    

    Dictionary<UnitRoleData, int> currentBuilders = new Dictionary<UnitRoleData, int>();

    void FindUnitsInSelection(){
        if(!Unit.teamUnits.ContainsKey(team)) return;

        List<Unit> units = Unit.teamUnits[team];
        List<Building> buildings = Building.teamBuildings[team];

        foreach (var unit in units) {
            CheckUnitInSelection(unit);
        }

        foreach (var building in buildings) {
            CheckBuildingInSelection(building);
        }   
    }

    void CheckUnitInSelection(Unit unit){
        if(selectCollider.bounds.Intersects(unit.mainCollider.bounds)){
            if(selectedUnits.Contains(unit)) return;

            selectedUnits.Add(unit);
            unit.SELECTED?.Invoke(unit);

        } else if (selectedUnits.Contains(unit)) {

            unit.UNSELECT?.Invoke(unit);
            selectedUnits.Remove(unit);
        }
    }

    void CheckBuildingInSelection(Building build){
        if(selectCollider.bounds.Intersects(build.bounds)){
            if(selectedBuildings.Contains(build)) return;

            selectedBuildings.Add(build);
            build.SELECTED?.Invoke();

        } else if (selectedBuildings.Contains(build)) {

            build.UNSELECT?.Invoke();
            selectedBuildings.Remove(build);
        }
    }

    void ClearSelection(){
        if(selectedUnits.Count > 0 ){
            // buildMenu.Clear();
            selectedUnits.ToList().ForEach(unit => unit.UNSELECT.Invoke(unit));
            selectedUnits.Clear();
            UNSELECTED?.Invoke();
        }
    }

    void PlacingPrefab(Vector2 mousePos){
        Ray ray = cam.ScreenPointToRay(mousePos);
        if(Physics.Raycast(ray, out RaycastHit hit, 100, 1 << 3  )){
            Vector3 cursorPoint = hit.point;
            
            placeholder.SetPositionAndRotation(cursorPoint + prefabGround , Quaternion.identity);
            bool curValidPosition = IsValidPosition();
            
            if(!isValid && curValidPosition){
                isValid = true;
                SetPlaceholderMaterial(validMaterial);

            }else if(isValid && !curValidPosition){
                isValid = false;
                SetPlaceholderMaterial(invalidMaterial);
            }


            if(Input.GetMouseButtonDown(0) && isValid){
                prefab.SetPositionAndRotation(cursorPoint + prefabGround , Quaternion.identity);
                Destroy(placeholder.gameObject);
                prefab.gameObject.SetActive(true);

                isPlacing = false;
                prefab = null;
                placeholder = null;
                player.CONFIRM_PLACE?.Invoke();
            }
        }

    }

    public void OnBeginPlaceObject(GameObject prefab){
        isPlacing = true;
        placeholder = GameObject.Instantiate(prefab).transform;
        
        if(placeholder.TryGetComponent<UnityEngine.AI.NavMeshObstacle>(out var collider)){
            Destroy(collider);
        }

        Collider[] cols = placeholder.GetComponentsInChildren<Collider>();
        foreach(Collider col in cols){
            Destroy(col);
        }

        prefab.SetActive(false);
        
        placeholderMeshes = placeholder.GetComponentsInChildren<MeshRenderer>();
        float globalLow = float.MaxValue;
        
        SetPlaceholderMaterial(invalidMaterial);

        // bordas da mesh https://f.feridinha.com/EYZFd.png
        Vector3 v0 = new Vector3(float.MaxValue,0,float.MaxValue); // bottom left
        Vector3 v1; // upper left
        Vector3 v2 = new Vector3(float.MinValue,0,float.MinValue); // upper right
        Vector3 v3; // bottom right

        for (int i = 0; i < placeholderMeshes.Length; i++){
            MeshRenderer mesh = placeholderMeshes[i];
            Vector3 localMin = mesh.bounds.min;
            Vector3 localMax = mesh.bounds.max;
            float localLow = mesh.bounds.min.y;

            if(localMin.x <= v0.x) v0.x = localMin.x;
            if(localMin.z <= v0.z) v0.z = localMin.z;
            
            if(localMax.x >= v2.x) v2.x = localMax.x;
            if(localMax.z >= v2.z) v2.z = localMax.z;
            
            if(localLow < globalLow) globalLow = localLow;
        }

        v1 = new Vector3(v0.x,0,v2.z);
        v3 = new Vector3(v2.x,0,v0.z);
        
        prefabGround = placeholder.InverseTransformPoint(Vector3.up * Math.Abs(globalLow));
        CreatePrefabCornersValidators(new Vector3[4]{v0,v1,v2,v3});
        
        this.prefab = prefab.transform;
    }

    void CreatePrefabCornersValidators(Vector3[] meshBorders){
        Validator[] validators = new Validator[4];

        for (int i = 0; i < 4; i++){
            Validator validator = new GameObject("validator p"+i).AddComponent<Validator>();
            validator.myTransform = validator.transform;
            validator.myTransform.SetParent(placeholder, true);
            
            BoxCollider box = validator.gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = Vector3.one * 0.02f;

            validator.collider = box;

            Vector3 prefabCorner = placeholder.InverseTransformPoint(meshBorders[i]);
            validator.localPosition =  new Vector3(prefabCorner.x, -prefabGround.y + 0.2f, prefabCorner.z);
            validators[i] = validator;
        }
        
        validatorPairs = new Validator[][]{
            new Validator[] {validators[0], validators[2]}, // Left bottom to right upper
            new Validator[] {validators[1], validators[3]}, // Left upper to right bottom
            new Validator[] {validators[0], validators[1]}, // Left bottom to left upper
            new Validator[] {validators[1], validators[2]}, // Left upper to right upper
            new Validator[] {validators[2], validators[3]}, // Right upper to right bottom
            new Validator[] {validators[3], validators[0]}  // Right bottom to left bottom
        };
    }

    bool IsValidPosition(){
        bool isValid = true;

        for (int i = 0; i < validatorPairs.Length; i++) {
            Validator v1 = validatorPairs[i][0];
            Validator v2 = validatorPairs[i][1];

            RaycastHit hit;
            Vector3 dir = (v2.position - v1.position).normalized;
            Ray ray = new Ray(v1.position, dir);

            if (Physics.Raycast(ray, out hit, Vector3.Distance(v1.position, v2.position), ~ignoreRaycastLayer, QueryTriggerInteraction.Collide)) {
                if(hit.collider == v1.collider) continue;

                if (!(hit.collider == v2.collider)) {
                    isValid = false;
                    break;
                }
            }
        }

        return isValid;
    }

    void OnDrawGizmos(){
        if(!placeholder) return;

        for (int i = 0; i < validatorPairs.Length; i++){
            Validator v1 = validatorPairs[i][0];
            Validator v2 = validatorPairs[i][1];

            RaycastHit hit;
            Vector3 dir = (v2.position - v1.position).normalized;
            Ray ray = new Ray(v1.position, dir);

            bool isValidPair = false;
            if (Physics.Raycast(ray, out hit, Vector3.Distance(v1.position, v2.position) + 0.2f, ~ignoreRaycastLayer, QueryTriggerInteraction.Collide))
            {
                if(hit.collider == v1.collider) continue;
                
                isValidPair = (hit.collider == v2.GetComponent<Collider>());
                if (!isValidPair)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(v1.position, hit.point);
                    Gizmos.DrawSphere(hit.point, 0.05f);
                    
                    if(!Physics.BoxCast(v1.position, Vector3.one * 0.2f, Vector3.down, Quaternion.identity, 1, terrainLayer)){
                    isValid = false;
                    break;
                    }
                }
            }

            if (isValidPair)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(v1.position, v2.position);
            }
        }
    }
    
    void SetPlaceholderMaterial(Material mat){
        for (int i = 0; i < placeholderMeshes.Length; i++){
            MeshRenderer mesh = placeholderMeshes[i];
            mesh.materials = Enumerable.Repeat(mat, mesh.materials.Length).ToArray();
        }
    }
}



public class Validator : MonoBehaviour { 
    public Transform myTransform;
    public new Collider collider;
    public Vector3 position {get => myTransform.position;}
    public Vector3 localPosition {set { myTransform.localPosition = value; } }
}

