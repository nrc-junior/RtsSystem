using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class ScreenSelector : MonoBehaviour {
    
    public LayerMask selectables;

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

    List<Unit> selecteds = new List<Unit>();

    public Action<List<Unit>> SELECTED;
    public Action UNSELECTED;
    
    public MenuBuilding buildMenu {protected get; set;}
    bool isClickingOnUI;

    void Awake(){
        cam = Camera.main;
        camTransform = cam.transform;
        selectCollider = selection3d.GetComponent<Collider>();
    }


    void LateUpdate() {
        Vector2 mPosScreenSpace = Input.mousePosition;
        Vector2 mPosCanvasSpace = mPosScreenSpace / canvas.scaleFactor;

        cursor.anchoredPosition = mPosCanvasSpace;

        if(Input.GetMouseButtonDown(0)){
            if(Menu.instance.IsClickingUI()){
                isClickingOnUI = true;
                return;
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

        if(Physics.Raycast(ray, out RaycastHit hit, 100, selectables)){
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
    

    void FindUnitsInSelection(){
        if(!Unit.teamUnits.ContainsKey(team)) return;
        
        List<Unit> units = Unit.teamUnits[team];
        UnitRoleData builder = null;

        foreach (var unit in units) {
            if(selectCollider.bounds.Intersects(unit.mainCollider.bounds)){
                selecteds.Add(unit);
                
                if(unit.role.buildables.Count > 0){
                    builder = unit.role;
                }

                unit.SELECTED.Invoke();
                
            }
        }
        
        if(builder != null){
            buildMenu.ShowUI(builder.buildables);
        }

        if(selecteds.Count > 0 ){
            SELECTED?.Invoke(selecteds);
        }
    }

    void ClearSelection(){
        if(selecteds.Count > 0 ){
            buildMenu.Clear();
            selecteds.ForEach(unit => unit.UNSELECT.Invoke());
            selecteds.Clear();
            UNSELECTED?.Invoke();
        }
    }
}

