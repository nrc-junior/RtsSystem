using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRC.World;
public class SettleInitialization : MonoBehaviour {
    [SerializeField] WorldGeneration world;
    

    void Start(){
        Chunk spawn = world.GetLandmark(Landmark.Hot | Landmark.Ignore_Water);
        int center = Mathf.FloorToInt(spawn.points.GetLength(0)/2);
        Vector3 pos = world.ToWorldPosition(spawn.points[center,center]);
        var settle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        settle.transform.position = pos;
        settle.name = "Settle";
        

    }
}
