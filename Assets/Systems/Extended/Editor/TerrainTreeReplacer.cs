using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[ExecuteInEditMode]
public class TerrainTreeReplacer : EditorWindow {


	[Header("References")]
	public Terrain _terrain;

	static ResourceData treeData;

	[MenuItem("Tools/TreeReplacer")]
	static void Init()
	{
		TerrainTreeReplacer window = (TerrainTreeReplacer)GetWindow(typeof(TerrainTreeReplacer));
		treeData = Resources.Load<ResourceData>("Tree");
	}

	void OnGUI()
	{

		_terrain = (Terrain)EditorGUILayout.ObjectField(_terrain, typeof(Terrain), true);
		_terrain = Terrain.activeTerrain;

		GUILayout.Label("Create");

		GUILayout.BeginHorizontal();

		if (GUILayout.Button("Convert (Keep Previous)", GUILayout.Height(40f)))
		{
			Convert();
		}

		if (GUILayout.Button("Convert (Clear Previous)", GUILayout.Height(40f)))
		{
			Clear();
			Convert();
		}

		GUILayout.EndHorizontal();


		GUILayout.Label("Destroy");

		GUILayout.BeginHorizontal();

		Color oldColor = GUI.backgroundColor;
		GUI.backgroundColor = Color.red;

		if (GUILayout.Button("Clear generated trees", GUILayout.Height(40f)))
		{
			Clear();
		}

		if (GUILayout.Button("ClearTerrainTreeInstances", GUILayout.Height(40f)))
		{
			ClearTerrainTreeInstances();
		}
		GUI.backgroundColor = oldColor;

		GUILayout.EndHorizontal();
	}

	public void Convert()
	{
		if(!treeData){
			treeData = Resources.Load<ResourceData>("Tree");
		}

		TerrainData terrain = _terrain.terrainData;
		Transform terrainTransform = _terrain.transform;


		float width = terrain.size.x;
		float height = terrain.size.z;
		float y = terrain.size.y;
		// Create parent
		GameObject parent = GameObject.Find("TREES_GENERATED");

		if (parent == null)
		{
			parent = new GameObject("TREES_GENERATED");
			//
		}

		Dictionary<GameObject, MeshRenderer> treeMesh = new Dictionary<GameObject, MeshRenderer>();

		foreach (TreePrototype item in terrain.treePrototypes){
			treeMesh.Add(item.prefab, item.prefab.GetComponentInChildren<MeshRenderer>());
		}
		


		for (int i = 0; i < terrain.treeInstances.Length; i++) {

			TreeInstance tree = terrain.treeInstances[i];

			Vector3 localPos = new Vector3(tree.position.x * terrain.size.x, tree.position.y * terrain.size.y, tree.position.z * terrain.size.z);
			Vector3 worldPos = terrainTransform.TransformPoint(localPos);

			GameObject treeObj = PrefabUtility.InstantiatePrefab(terrain.treePrototypes[tree.prototypeIndex].prefab) as GameObject;
			Transform treeTransform = treeObj?.transform;
			
			if (treeTransform != null) {
				treeTransform.name += " (" + i.ToString() + ")";
				treeTransform.parent = parent.transform;
				treeTransform.position = worldPos;
				treeTransform.localScale = new Vector3(tree.widthScale,tree.heightScale, tree.widthScale);
				treeTransform.rotation = Quaternion.Euler(0,tree.rotation * Mathf.Rad2Deg,0);
				
				treeObj.layer = Resource.layer;

				BoxCollider box = treeObj.AddComponent<BoxCollider>();
				Bounds bounds = treeMesh[terrain.treePrototypes[tree.prototypeIndex].prefab].bounds;
				Vector3 hitBox = new Vector3(2,bounds.size.y,2);
				box.size = hitBox; 
				box.center = bounds.center;

				TreeBehaviour collectable = treeObj.AddComponent<TreeBehaviour>();
				collectable.data = treeData;
				collectable.bounds = bounds;
			}
			else
			{
				Debug.LogError("<< Cant find the Tree prototype Prefab >> ", terrain);
			}
		}
	}

	public void Clear()
	{
		DestroyImmediate(GameObject.Find("TREES_GENERATED"));
	}

	public void ClearTerrainTreeInstances()
	{
		_terrain.terrainData.treeInstances = new List<TreeInstance>().ToArray();
	}

}