using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CursorType { Idle, Select, Attack, Collect, Rect }
public class HandleCursor : MonoBehaviour {
    static Animator cursorAnimator;

    protected virtual void Awake() {
        cursorAnimator = GetComponent<Animator>();
        // Cursor.visible = false;
    }

    public static void SetCursor(CursorType type){
        // Cursor.visible = false;

        switch (type){
            case CursorType.Idle: cursorAnimator.Play("Idle"); break;
            case CursorType.Select: cursorAnimator.Play("Select"); break;
            case CursorType.Attack: cursorAnimator.Play("Attack"); break;
            case CursorType.Collect: cursorAnimator.Play("Collect"); break;
            case CursorType.Rect: cursorAnimator.Play("Rect"); break;
        }
    }

    public static void Clear(){
        cursorAnimator.Play("Idle");
    }
}
