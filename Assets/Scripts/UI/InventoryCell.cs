using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public Text counter;
    public Text label;
    
    
    [Header("Data")]
    public PlayerResource storing;

    public void Awake(){
        counter.gameObject.SetActive(false);
        label.gameObject.SetActive(false);
    }

    public void Clear(){
        if(storing != null){
            storing.Change -= OnChangeQuantity;
            storing = null;
        }
        counter.text = "";
        counter.gameObject.SetActive(false);
        label.gameObject.SetActive(false);
    }

    public void SetResource(PlayerResource resource){
        storing = resource;
        counter.gameObject.SetActive(true);
        label.gameObject.SetActive(true);
        label.text = resource.type.name;
        counter.text = resource.quantity.ToString();
        resource.Change += OnChangeQuantity;
    }

    void OnChangeQuantity(){
        counter.text = storing.quantity.ToString();
    }

    public void OnPointerEnter(PointerEventData eventData){

    }

    public void OnPointerExit(PointerEventData eventData){

    }
}
