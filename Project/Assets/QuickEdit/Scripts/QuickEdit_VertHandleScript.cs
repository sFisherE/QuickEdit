using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]

public class QuickEdit_VertHandleScript : MonoBehaviour {

	
	public bool isActive = false;
	public bool isSelected = false;
    //public int[] myVertIndices = new int[0];
    public List<int> myVertIndices = new List<int>();
	
	public void Activate()
	{
		isActive = true;
	}
	
	public void AddVertIndex(int vertIndex)
	{
        //var tempIndices = new Array();
        //tempIndices = myVertIndices;
        //tempIndices.Add(vertIndex);
        //myVertIndices = tempIndices;
        myVertIndices.Add(vertIndex);
	}
	
	public void UpdateAttachedVerts(Mesh theMesh) 
	{	
		var meshVerts = theMesh.vertices;
		foreach(var theVertIndex in myVertIndices)
		{
			meshVerts[theVertIndex] = transform.parent.InverseTransformPoint(transform.position);
		}
		theMesh.vertices = meshVerts;
	}
	
	public void OnDrawGizmos()
	{
		if(isActive)
		{
			if(isSelected)
			{
				Gizmos.DrawIcon(transform.position, "QuickEdit/VertOn.tga", false);			
			}
			else
			{
				Gizmos.DrawIcon(transform.position, "QuickEdit/VertOff.tga", false);
			}
		}
	}
}
