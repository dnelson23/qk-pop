using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Debug = FFP.Debug;

public class CheckTargets : MonoBehaviour {

	void Update()
	{
		List<GameObject> targets = PoPCamera.instance.GetAllTargets ();
		foreach(GameObject go in targets) {
			Targetable target = go.GetComponent<Targetable>();

			if(target.time <= 0f) {
                if (!checkCameraVisibility(go))
                {
                    target.time = Vector3.Distance(go.transform.position, PoPCamera.instance.target.position) / 100f;
                }
			} else
				target.time -= Time.deltaTime;
		}
	}

	public bool checkCameraVisibility(GameObject go)
	{
		Targetable target = go.GetComponent<Targetable> ();
		target.isTargetable = false;
		
		if (Vector3.Distance (go.transform.position, PoPCamera.instance.target.position) <= PoPCamera.instance.targetingRange)
		{
			if(go.GetComponent<Renderer>().IsVisibleFrom(Camera.main))
			{
				RaycastHit hit;
				Physics.Raycast(PoPCamera.instance.transform.position, 
				                (go.transform.position - PoPCamera.instance.transform.position), 
				                out hit, Mathf.Infinity, PoPCamera.instance.NoOcclusionLM);
				
				if(hit.collider.name == go.GetComponent<Collider>().name)
				{
					target.isTargetable = true;
				}
			}
		}
		
		return target.isTargetable;
	}
}
