using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialUI : MonoBehaviour {
    public Transform centerAnchor;
    public Transform highlight;
    public Transform minAnchor, maxAnchor;

    Vector3 center { get => centerAnchor.position; }
    Vector3 min { get => minAnchor.position; }
    Vector3 max { get => maxAnchor.position; }

    bool isOpen;

    void Awake(){
        Close();
    }
    
    public void Open(){
        if(isOpen) return;
        isOpen = true;
        gameObject.SetActive(true);
    }
    
    public void Close(){
        isOpen = false;
        gameObject.SetActive(false);
    }

    public void Update(){
        if(Input.GetKeyDown(KeyCode.Escape) && isOpen){
            Close();
        }
        Vector3 mp = Input.mousePosition;
        Vector2 delta = center - mp;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        angle += 180;
        
        bool underMax = Vector3.Distance(mp, center) < Vector3.Distance(max, center);
        bool aboveMin = Vector3.Distance(mp, center) > Vector3.Distance(min, center);
        
        if(underMax && aboveMin){
            if(!highlight.gameObject.activeSelf){
                highlight.gameObject.SetActive(true);
            }

            for (var i = 0; i < 360; i+= 45){
                if(angle >= i && angle < i + 45){
                    highlight.eulerAngles = new Vector3(0,0,i-45);
                }
            }

        }else if(highlight.gameObject.activeSelf){
            highlight.gameObject.SetActive(false);
        }
    }

}
