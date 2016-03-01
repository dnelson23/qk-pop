using UnityEngine;
using System.Collections;

public class Interactable : MonoBehaviour
{
	public enum ObjectType { Ladder, Door, Sidle, Ledge }
	public ObjectType Type { get; set; }

	// Ladder Public Variables
	public Vector3 ladderBottom = Vector3.zero;
	public Vector3 ladderTop = Vector3.zero;

	void OnDrawGizmosSelected()
	{
		switch(Type)
		{
			case ObjectType.Ladder:
				Gizmos.color = Color.red;
				Gizmos.DrawLine(ladderBottom, ladderTop);
				Gizmos.color = Color.cyan;
				Gizmos.DrawWireSphere(ladderBottom, 0.3f);
				Gizmos.DrawWireSphere(ladderTop, 0.3f);
				Gizmos.color = new Color(0,1,1,0.5f);
				Gizmos.DrawSphere(ladderBottom, 0.3f);
				Gizmos.DrawSphere(ladderTop, 0.3f);
			break;
			case ObjectType.Sidle:
				
			break;
			case ObjectType.Door:
				
			break;
			default:

			break;
		}
	}

	void Start()
	{
		switch(Type) 
		{
			case ObjectType.Ladder:
				InitLadderComponents();
			break;
			default:

			break;
		}
	}

	void Update() {
		Debug.Log (this.gameObject.name + this.Type);
	}

	void InitLadderComponents() 
	{
		GameObject bottomObj = new GameObject();
		GameObject topObj = new GameObject();

		bottomObj.name = "Ladder Bottom";
		bottomObj.transform.position = ladderBottom;
		topObj.name = "Ladder Top";
		topObj.transform.position = ladderTop;

		bottomObj.transform.parent = transform;
		topObj.transform.parent = transform;

		SphereCollider startCol = bottomObj.AddComponent<SphereCollider>();
		SphereCollider endCol = topObj.AddComponent<SphereCollider>();

		bottomObj.layer = LayerMask.NameToLayer("No Occlusion");
		topObj.layer = LayerMask.NameToLayer("No Occlusion");

		startCol.radius = 0.3f;
		endCol.radius = 0.3f;
	}
}
