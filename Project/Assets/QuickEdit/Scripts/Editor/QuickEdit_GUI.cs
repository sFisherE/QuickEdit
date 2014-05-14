using UnityEngine;
using System.Collections;
using UnityEditor;
public class QuickEdit_GUI : QuickEdit_Base 
{
    [MenuItem("Window/QuickEdit")]
    public static void Initialize()
	{
        var window = GetWindow(typeof(QuickEdit_GUI), true, "QuickEdit");
        window.Show();
    }
	
	//force exit mesh edit on close
	public void OnDisable()
	{
		if(editModeActive)
		{
			CancelMeshEdit();
		}
	}
	//
	
	//GUI
	public void OnGUI()
	{		
		var window = this;
		window.minSize = new Vector2(200,75);
		window.maxSize = new Vector2(200,75);
		
		
		if(!editModeActive)
		{			
			if(newMeshName == "")
			{
				newMeshName = "Type Name";
			}
			newMeshName = EditorGUILayout.TextField(newMeshName);
			if(newMeshName == "")
			{
				newMeshName = "Type Name";
			}

			if(GUILayout.Button("Edit As New new Mesh (Copy)") && Selection.activeGameObject && Selection.gameObjects.Length == 1 && Selection.activeGameObject.GetComponent(typeof(MeshFilter)))
			{
				if(newMeshName == "")
				{
					EditorUtility.DisplayDialog("Name Required", "Please type a name for the new mesh, just above the edit buttons.", "Okay");
				}
				else if(!NameIsUnique(newMeshName))
				{
					if(EditorUtility.DisplayDialog("Overwrite?", "A mesh with this name already exists- overwrite it? (This can NOT be undone!)", "Okay", "Cancel"))
					{
						editingShared = false;
						EnterEditMode();
					}
				}
				else
				{
					editingShared = false;
					EnterEditMode();
				}
			}
			
			if(GUILayout.Button("Edit Source new Mesh (Instance)") && Selection.activeGameObject && Selection.gameObjects.Length == 1 && Selection.activeGameObject.GetComponent(typeof(MeshFilter)))
			{
				if(EditorUtility.DisplayDialog("Edit Shared Mesh?", "This will edit the object's mesh directly, without making a copy. The modifications can not be undone!", "Okay", "Cancel"))
				{
					editingShared = true;
					EnterEditMode();
				}				
			}
		}
		else
		{
			if(GUILayout.Button("Done"))
			{
				if(EditorUtility.DisplayDialog("Exit Edit Mode?", "Once you exit edit mode, changes to the mesh can NOT be undone!", "Okay", "Cancel"))
				{
					ExitEditMode();
				}				
			}
			if(GUILayout.Button("Cancel"))
			{
				if(EditorUtility.DisplayDialog("Cancel Edit Mode?", "All changes will be lost!", "Okay", "Cancel"))
				{
					CancelMeshEdit();
				}				
			}
		}
	}
	//
}
