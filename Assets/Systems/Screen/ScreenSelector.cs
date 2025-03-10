using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;
using MyBox;

public class ScreenSelector : MonoBehaviour {
    [ReadOnly] public Camera targetCamera;
    [ReadOnly] public Vector2 mPosScreenSpace;
    [ReadOnly] public Vector2 mPosCanvasSpace;
    [ReadOnly] public bool selecting;
    
    [Separator("Configuration")]
    public LayerMask selectionLayer;
    
    public Canvas canvas;
    public RectTransform rect;
    public RectTransform cursor;

    private bool isClickingOnUI;

    private Transform selection3d;
    private Collider selection3dCol;
    
    // * events
    /// <summary> when start selecting </summary>
    public Action BEGIN;

    /// <summary> when released the selection </summary>
    public Action END;

    private Transform camTransform;

    private Vector3 startPoint, start3dPoint;
    private Vector3 endPoint, end3dPoint;

    private Vector2 negativeAnchor;



    void Awake(){
        targetCamera = Camera.main;
        camTransform = targetCamera.transform;

        selection3d = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
        selection3d.gameObject.GetComponent<MeshRenderer>().enabled = false;
        selection3dCol = selection3d.GetComponent<Collider>();
        selection3dCol.isTrigger = true;
    }

    void LateUpdate() {
        mPosScreenSpace = Input.mousePosition;
        mPosCanvasSpace = mPosScreenSpace / canvas.scaleFactor;

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
        Ray ray = targetCamera.ScreenPointToRay(mousePos);

        if(Physics.Raycast(ray, out RaycastHit hit, 100, selectionLayer)){
            start3dPoint = hit.point;
            start3dPoint.y = 0;
        }

        selection3d.gameObject.SetActive(true);
        selection3d.position = start3dPoint;
        
        BEGIN?.Invoke();
    }
    
    void End3DSelection(Vector2 mousePos){
        Ray ray = targetCamera.ScreenPointToRay(mousePos);
        
        if(Physics.Raycast(ray, out RaycastHit hit, 100, 1 << 3 )){
            end3dPoint = hit.point;
        }

        END?.Invoke();

        selection3d.gameObject.SetActive(false);
    }
    
    void Resize3DBox(Vector2 mousePos){
        Ray ray = targetCamera.ScreenPointToRay(mousePos);

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
    

    public bool IntersectBound(Bounds bound){
        return selection3dCol.bounds.Intersects(bound);
    }
}



public class Validator : MonoBehaviour { 
    public Transform myTransform;
    public new Collider collider;
    public Vector3 position {get => myTransform.position;}
    public Vector3 localPosition {set { myTransform.localPosition = value; } }
}

