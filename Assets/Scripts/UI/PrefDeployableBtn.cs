using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrefDeployableBtn : MonoBehaviour {
    [HideInInspector] public DeployablesData data;

    public Button button;
    public new Text name;
    public Image icon;
    public Image background;

    const float disabledAlphaVal = .2f;
    const float enabledAlphaVal = 1;
    MenuBuilding.DeployClassBox onClicked;


    public void SetupButton(bool active, MenuBuilding.DeployClassBox callback, DeployablesData data){
        this.data = data;
        this.name.text = data.name;
        this.icon.sprite = data.icon;
        this.background.color = data.btnColor;
        
        onClicked = callback;
        button.onClick.AddListener(SendCallback);

        gameObject.SetActive(true);

        if(!active) 
            Disable();

    }

    void SendCallback(){
        onClicked(this);
    }

    public void Disable(){
        button.interactable = false;
        
        Color disabledAlpha = name.color;
        disabledAlpha.a = disabledAlphaVal;
        name.color = disabledAlpha;

        disabledAlpha = icon.color;
        disabledAlpha.a = disabledAlphaVal;
        icon.color = disabledAlpha;

        disabledAlpha = background.color;
        disabledAlpha.a = disabledAlphaVal;
        background.color  = disabledAlpha;
    }
    public void Enable(){
        button.interactable = true;
        
        Color enabledAlpha = name.color;
        enabledAlpha.a = enabledAlphaVal;
        name.color = enabledAlpha;

        enabledAlpha = icon.color;
        enabledAlpha.a = enabledAlphaVal;
        icon.color = enabledAlpha;

        enabledAlpha = background.color;
        enabledAlpha.a = enabledAlphaVal;
        background.color  = enabledAlpha;
    }
}
