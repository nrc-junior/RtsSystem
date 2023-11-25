using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class FlightControl : MonoBehaviour {
    public float netVelocity;
    Rigidbody rb;
    
    Vector3 gravity { get => Physics.gravity; }

    void Awake(){
        TryGetComponent(out rb);
        // rb.useGravity = false;
    }

    [ExecuteAlways]
    void OnGUI(){
        GUILayout.Box($"<color=pink>{netVelocity}</color> ", new GUIStyle(){fontSize= 42, richText = true});
    }

    void FixedUpdate(){
        netVelocity = rb.velocity.y;
        
        float thrust = Mathf.Abs(rb.velocity.y + (rb.mass * 9.81f));
        Vector3 thrustDir = transform.up;
        rb.AddForce(thrust * thrustDir);
    }
}
