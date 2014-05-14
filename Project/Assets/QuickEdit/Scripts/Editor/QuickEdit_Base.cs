using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class QuickEdit_Base : EditorWindow 
{	
	//external variables
	public Object vertHandlePrefab = (Resources.LoadAssetAtPath("Assets/QuickEdit/Shared/Prefabs/VertHandlePrefab.prefab", typeof(Object)));
	
	//states
	public bool editModeActive = false;
	public bool editingShared = false;
	
	//internally shared variables
	public GameObject sourceObject;
    //public Transform[] vertHandles;
    public List<Transform> vertHandles;//=new List<Transform>();
	public string newMeshName = "";
	public int vertIndex = 0;
	public Vector3[] vertPositions;
	public Mesh sourceMesh;
	public Mesh editMesh;
	public Vector3[] backupMeshVertData;
	public Vector2[] backupMeshUVData;
	
	
	//selection variables
	public bool sel_Dirty = false;
	public GameObject storedSelection;
	public int storedLength;
    //public QuickEdit_VertHandleScript[] vertHandleSelection;
    public List<QuickEdit_VertHandleScript> vertHandleSelection;//=new List<QuickEdit_VertHandleScript>();
	public Vector3 activeObjectPrevPos;
	
	//Update Loop
	public void Update()
	{
		if(editModeActive)
		{
			//custom "OnSelectionChange"
			if(Selection.activeGameObject != storedSelection || Selection.gameObjects.Length != storedLength)
			{
				storedSelection = Selection.activeGameObject;
				storedLength = Selection.gameObjects.Length;
				FilterSelection();				
			}		
			//move verts
			if(Selection.activeTransform && !sel_Dirty)
			{
			activeObjectCurrentPos = Selection.activeTransform.position;
				if(activeObjectCurrentPos != activeObjectPrevPos)
				{
					UpdateVertHandles();
					activeObjectPrevPos = activeObjectCurrentPos; 
				}			
			}
		}
	}
	//
    Vector3 activeObjectCurrentPos;
	public void EnterEditMode()
	{
		editModeActive = true;

		sourceObject = Selection.activeGameObject;
        sourceMesh = sourceObject.GetComponent<MeshFilter>().sharedMesh;

        

		//Undo.RegisterUndo(sourceObject.GetComponent(typeof(MeshFilter)), "Edit Mesh");
		
		//save the starting mesh data, so it can be cancelled
		backupMeshVertData = sourceMesh.vertices;
		backupMeshUVData = sourceMesh.uv;
		
		
		if(!editingShared) //copy the source mesh, save it, and make it the new used mesh
		{
			editMesh = new Mesh();
			
			editMesh.vertices = sourceMesh.vertices;
			editMesh.uv = sourceMesh.uv;			
			editMesh.uv2 = sourceMesh.uv2;			
			editMesh.triangles = sourceMesh.triangles;
			editMesh.normals = sourceMesh.normals;
			editMesh.tangents = sourceMesh.tangents;
			
			//get/set triangles for submeshes (yayy!)
			editMesh.subMeshCount = sourceMesh.subMeshCount;
			for(int t = 0;t<sourceMesh.subMeshCount;t++)
			{
				editMesh.SetTriangles(sourceMesh.GetTriangles(t), t);
			}
			//

			AssetDatabase.CreateAsset(editMesh, "Assets/QuickEdit/Meshes/"+newMeshName+".asset");
			AssetDatabase.SaveAssets();

            sourceObject.GetComponent<MeshFilter>().sharedMesh = editMesh;
		}		
		else //edit the source mesh directly
		{
			editMesh = sourceMesh;
		}

		vertPositions = editMesh.vertices;
        //vertHandles = new Transform[0];
        vertHandles = new List<Transform>();
		vertIndex = 0;
        //Selection.objects = new Object[0];// null;// new Array();
        //vertHandleSelection = new QuickEdit_VertHandleScript[0];
        vertHandleSelection = new List<QuickEdit_VertHandleScript>();
		AssignVertHandles();
	}
	
	public void CancelMeshEdit()
	{
		if(!editingShared)
		{
			sourceObject.GetComponent<MeshFilter>().sharedMesh = sourceMesh;
		}
		else
		{
			sourceMesh.vertices = backupMeshVertData;
			sourceMesh.uv = backupMeshUVData;
		}
		
		foreach(var theVertHandle in vertHandles)
		{
			if(theVertHandle)
			{
				DestroyImmediate(theVertHandle.gameObject);
			}
		}
        //Selection.objects = new Object[0];// new Array();
		Selection.activeObject = sourceObject;
		editModeActive = false;	
	}
	
	public void ExitEditMode()
	{
		foreach(var theVertHandle in vertHandles)
		{
			if(theVertHandle)
			{
				DestroyImmediate(theVertHandle.gameObject);
			}
		}
        //Selection.objects = new Object[0];// new Array();
		Selection.activeObject = sourceObject;
		editModeActive = false;
	}
	
	//check the pos of each vert, and assign to vert handle or make new vert handle as needed
	public void AssignVertHandles()
	{		
		foreach(var theVertPos in vertPositions)
		{			
			if(!VertHandleHere(theVertPos, vertIndex))
			{
				//create a new vert handle at this vert's pos and set it up
				GameObject newVertHandle = Instantiate(vertHandlePrefab, Vector3.zero, Quaternion.identity) as GameObject;
				newVertHandle.transform.parent = sourceObject.transform;
				newVertHandle.transform.localPosition = theVertPos;
                newVertHandle.GetComponent<QuickEdit_VertHandleScript>().Activate();
                newVertHandle.GetComponent<QuickEdit_VertHandleScript>().AddVertIndex(vertIndex);

				newVertHandle.name = "VertHandle_"+vertIndex;
				
				//add the new vert handle to the vert handle array
                vertHandles.Add(newVertHandle.transform);
			}
			vertIndex++;
		}
	}
	
	public bool VertHandleHere(Vector3 theVertPos, int vertIndex)
	{
		foreach(var theVertHandle in vertHandles)
		{
			if(theVertHandle.transform.localPosition == theVertPos)
			{
                theVertHandle.gameObject.GetComponent<QuickEdit_VertHandleScript>().AddVertIndex(vertIndex);
                return true;
			}
		}
        return false;
	}
	
	public bool NameIsUnique(string newMeshName)
	{
		if(AssetDatabase.LoadAssetAtPath("Assets/QuickEdit/Meshes/"+newMeshName+".asset", typeof(Object)))
		{
			return false;
		}
		else
			return true;
	}
	
	public void FilterSelection()
	{
		sel_Dirty = true;
		if(editModeActive)
		{
			for(int i = 0;i<vertHandleSelection.Count;i++)
			{
				vertHandleSelection[i].isSelected = false;
			}
            vertHandleSelection.Clear();
            //var tempHandleSelection = new Array();
			for(int v=0;v<Selection.gameObjects.Length;v++)
			{
				if(Selection.gameObjects[v].GetComponent<QuickEdit_VertHandleScript>())
				{
                    //vertHandleSelection.Add(Selection.gameObjects[v].GetComponent(typeof(QuickEdit_VertHandleScript)));
                    vertHandleSelection.Add(Selection.gameObjects[v].GetComponent<QuickEdit_VertHandleScript>());
                }
			}
            //vertHandleSelection = tempHandleSelection;

            if (Selection.activeTransform)
            {
                if (vertHandleSelection.Count > 0)
                {
                    var tempSel = new List<GameObject>();
                    foreach (var tvh in vertHandleSelection)
                    {
                        tempSel.Add(tvh.gameObject);
                    }
                    //Selection.objects = tempSel.ToArray();
                    activeObjectPrevPos = Selection.activeTransform.position;
                    foreach (var theVertHandle in vertHandleSelection)
                    {
                        theVertHandle.isSelected = true;
                    }
                    activeObjectCurrentPos = Selection.activeTransform.position;
                }
            }
            //else
            //    Selection.objects = new Object[0];// new Array();
		}
		sel_Dirty = false;
	}
	
	public void UpdateVertHandles()
	{
		foreach(var vertHandle in vertHandleSelection)
		{
			vertHandle.UpdateAttachedVerts(editMesh);
		}
	}
}
