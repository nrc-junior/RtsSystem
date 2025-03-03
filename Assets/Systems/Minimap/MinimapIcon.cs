using UnityEngine;
public class MinimapIcon : MonoBehaviour  {
    public Transform iconTransform;
    public void Initialize(string iconMaterialName) {
        
        MeshRenderer mesh =  GameObject.CreatePrimitive(PrimitiveType.Quad).GetComponent<MeshRenderer>();
        mesh.material = Resources.Load<Material>(iconMaterialName);
        iconTransform = mesh.transform;
        iconTransform.SetParent(transform);
        iconTransform.localPosition = Vector3.zero;
        iconTransform.rotation = Quaternion.Euler(new Vector3(90,0,0));
        iconTransform.gameObject.layer = LayerMask.NameToLayer("Minimap");
    }
}
