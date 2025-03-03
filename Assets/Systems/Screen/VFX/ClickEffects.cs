using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickEffect : MonoBehaviour
{
    ParticleSystem particles;
    Transform thisTransform;
    public static ClickEffect instance;
    
    void Awake(){
        instance = this;
        particles = GetComponent<ParticleSystem>();
        thisTransform = transform;
    }
    
    public void Play(Vector3 pos){
        thisTransform.position = pos;
        particles.Play();
    }
}
