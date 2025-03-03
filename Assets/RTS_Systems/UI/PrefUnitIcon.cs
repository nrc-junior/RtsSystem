using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrefUnitIcon : MonoBehaviour {
        public Image icon;
        public new Text name;
        public Text count;
        public int countNmbr {get; private set;} = 0;
        public void AddUnit(){
            countNmbr++;
            count.text = countNmbr.ToString();
        }

        public void RemUnit(){
            countNmbr--;
            count.text = countNmbr.ToString();
            if(countNmbr == 0){
                Destroy(gameObject);
            }
        }

}
