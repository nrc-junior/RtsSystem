using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeBehaviour : Resource {
    public Bounds bounds {private get; set;}
    
    protected override void Awake() {
        base.Awake();

        EMPTY += OnEmpty;
    }

    public void OnEmpty(Resource tree){
        Debug.Log(tree.name + " is empty");

        Vector3 curEuler = transform.localEulerAngles;
        curEuler.z = -90f;
        LeanTween.rotateLocal(gameObject, curEuler, Random.Range(1f,2f)).setOnComplete(tree.DisableResource);
    }
}
