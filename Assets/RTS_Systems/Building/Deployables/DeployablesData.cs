using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Deployable Data", menuName = "Deployable")]
public class DeployablesData : ScriptableObject {
    public new string name;
    public Sprite icon;
    public Color btnColor;
    public string resourceDeployable = "defaultDeployable";

    [Space]
    public UnitRoleData deployRole;
    
    // todo validar status da unidade que pode usar pickdata
}
