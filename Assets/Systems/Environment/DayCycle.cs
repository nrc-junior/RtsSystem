using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DayCycle : MonoBehaviour {
    
    public Transform sun;
    [Range(0,1)] public float timeOfDay;
    
    public float changeSpeed;

    public string hourPreview;

    const float anglePerHour = 0.0416666666666667f;
    const float anglePerMinute = 0.00069444444f;

    [Min(1)] public float ratio = 1;

    public float hour;
    public float minute;
    
    public bool updateTimeInEditor;
    bool inGame;

    void Awake(){
        inGame = Application.isPlaying;
    }

    [ExecuteInEditMode]
    void Update() {
        if(inGame || updateTimeInEditor) UpdateTime();

        hour = ((timeOfDay / anglePerHour)+6)%24;
        minute = Mathf.FloorToInt((((hour%1) / anglePerMinute) / 1440) * 60);
        hour = Mathf.FloorToInt(hour);

        Quaternion rot = Quaternion.Euler(Mathf.Lerp(0,360, timeOfDay),0,0);
        sun.transform.rotation = rot;
    }

    void UpdateTime(){
        timeOfDay += ((Time.deltaTime * anglePerMinute)/60) * ratio;
        timeOfDay %= 1;
    }
}
