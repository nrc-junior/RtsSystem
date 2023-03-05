using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UiElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public virtual void Awake(){}
    public virtual void OnPointerEnter(PointerEventData eventData){}
    public virtual void OnPointerExit(PointerEventData eventData){}
}
