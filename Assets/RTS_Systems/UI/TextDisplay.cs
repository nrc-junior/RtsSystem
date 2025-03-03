using UnityEngine;
using UnityEngine.UI;

public class TextDisplay : MonoBehaviour {
    public Text label;
    public CanvasGroup background;
    Color cached = Color.clear;
    public void SetAlpha(float a){
        background.alpha = a;
    }

}