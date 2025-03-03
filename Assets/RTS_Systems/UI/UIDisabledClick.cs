using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDisabledClick : UiElement {
    public bool isHovering;

    void OnDisable(){
        isHovering = false;
    }

    public override void OnPointerEnter(PointerEventData eventData){
        isHovering = true;
    }

    public override void OnPointerExit(PointerEventData eventData){
        isHovering = false;
    }
}
