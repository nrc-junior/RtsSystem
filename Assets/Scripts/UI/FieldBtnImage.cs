using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class FieldBtnImage : UiElement {

    public Button btn;
    public GameObject descriptionModule;
    public Text descriptionText;
    public Image icon;

    public Action CLICK;

    public override void Awake(){
        btn.onClick.AddListener(OnClick);   
    }

    void SetEnable(bool status){
        btn.interactable = status;
    }

    void OnClick(){
        
        CLICK?.Invoke();
    }

    public override void OnPointerEnter(PointerEventData eventData){
        descriptionModule.SetActive(true);
    }   
    
    public override void OnPointerExit(PointerEventData eventData){
        descriptionModule.SetActive(false);
    }

    public void SetCost(ResourcePrice price){
        descriptionText.text = $"r: {price.red}\ng: {price.green}\nb: {price.blue}";
    }
}
