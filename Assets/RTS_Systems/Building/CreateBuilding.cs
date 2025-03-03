using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreateBuilding : MonoBehaviour
{
    [SerializeField] ScreenSelector selector;
    [SerializeField] MainManagerRTS player;

    bool isPlacing;
    Vector3 prefabGround;
    MeshRenderer[] placeholderMeshes;
    bool isValid;
    Material validMaterial;
    Material invalidMaterial;
    Transform prefab;
    Transform placeholder;


    Validator[][] validatorPairs;
    List<Material> placeholderMaterials = new List<Material>();
    [SerializeField] LayerMask ignoreRaycastLayer;

    void Awake(){
        player.PLACE_OBJECT += OnBeginPlaceObject;
        validMaterial = Resources.Load<Material>("Materials/placeholderOk");
        invalidMaterial = Resources.Load<Material>("Materials/placeholderFail");
    }

    void LateUpdate(){
        if(isPlacing){
            PlacingPrefab(selector.mPosCanvasSpace);
            return;
        }
    }

    void PlacingPrefab(Vector2 mousePos){
        Ray ray = selector.targetCamera.ScreenPointToRay(mousePos);
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
        
        prefabGround = placeholder.InverseTransformPoint(Vector3.up * Mathf.Abs(globalLow));
        CreatePrefabCornersValidators(new Vector3[4]{v0,v1,v2,v3});
        
        this.prefab = prefab.transform;
    }

    void SetPlaceholderMaterial(Material mat){
        for (int i = 0; i < placeholderMeshes.Length; i++){
            MeshRenderer mesh = placeholderMeshes[i];
            mesh.materials = Enumerable.Repeat(mat, mesh.materials.Length).ToArray();
        }
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
                    
                    if(!Physics.BoxCast(v1.position, Vector3.one * 0.2f, Vector3.down, Quaternion.identity, 1, selector.selectionLayer)){
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
    
}
