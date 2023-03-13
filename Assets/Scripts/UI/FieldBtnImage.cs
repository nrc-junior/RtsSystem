using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class FieldBtnImage : UiElement {

    public Button btn;
    public GameObject descriptionModule;
    public Text descriptionText;
    public Image icon;

    public ResourcePrice[] totalPrice {get; private set;}
    
    public Action<FieldBtnImage> CLICK;
    

    public delegate void Purchase(object item);
    public Purchase onPurchase;
    public object item;


    public override void Awake(){
        btn.onClick.AddListener(OnClick);   
    }

    void SetEnable(bool status){
        btn.interactable = status;
    }

    void OnClick(){
        CLICK?.Invoke(this);
    }

    public override void OnPointerEnter(PointerEventData eventData){
        descriptionModule.SetActive(true);
    }   
    
    public override void OnPointerExit(PointerEventData eventData){
        descriptionModule.SetActive(false);
    }

    public void SetCost(ResourcePrice[] prices){
        totalPrice = prices;
        string description = "";
        foreach (ResourcePrice price in prices){
            description += $"{price.resource.name}: {price.cost}\n"; 
        }

        descriptionText.text = description;
    }
}
