using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRig : MonoBehaviour {
    private Animator animator;
    public bool isMoving {private get; set;}
    public bool isCollecting {private get; set;}

    void Awake(){
        animator = GetComponentInChildren<Animator>();
    }

    public void RefreshParameters(){
        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isCollecting", isCollecting);
    }
}
