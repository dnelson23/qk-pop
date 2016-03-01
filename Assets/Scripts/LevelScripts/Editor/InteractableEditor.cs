
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Interactable))]

public class InteractableEditor : Editor 
{
	Interactable thisObject;

	public override void OnInspectorGUI()
	{
		thisObject = (Interactable)target;
		thisObject.Type = (Interactable.ObjectType)EditorGUILayout.EnumPopup ("Type: ", thisObject.Type);

		switch (thisObject.Type) 
		{
			case Interactable.ObjectType.Ladder:

                // Calculate height of ladder from it's collider
                float height = thisObject.transform.localScale.y * ((BoxCollider)thisObject.GetComponent<Collider>()).size.y;

                // Get world position of top and bottom of ladder
                Vector3 top = new Vector3(thisObject.transform.position.x, (thisObject.transform.position.y + (height / 2)), thisObject.transform.position.z);
                Vector3 bottom = new Vector3(thisObject.transform.position.x, thisObject.transform.position.y - (height / 2), thisObject.transform.position.z);

                // Move the positions forward a small amount and set them to the ladders variables
                thisObject.ladderBottom = bottom + (thisObject.transform.forward / 2);
                thisObject.ladderTop = top + (thisObject.transform.forward / 2);

				EditorGUILayout.Vector3Field("Start: ", thisObject.ladderBottom);
				EditorGUILayout.Vector3Field("End: ", thisObject.ladderTop);

				break;
			case Interactable.ObjectType.Ledge:
				
				break;
		}

		if (GUI.changed) {
			EditorUtility.SetDirty(thisObject);
		}

	}

	public void OnSceneGUI()
	{
		thisObject = (Interactable)target;
		Debug.Log ("player", thisObject);
		switch (thisObject.Type) 
		{

		case Interactable.ObjectType.Ladder:
				// Show start and end labels
				Handles.Label(thisObject.ladderBottom, "Start");
				Handles.Label (thisObject.ladderTop, "End");
				break;

			case Interactable.ObjectType.Ledge:
				// Show start and end labels
				break;
		}

		Repaint ();
	}
}
