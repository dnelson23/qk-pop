#pragma warning disable 168     //Variable assigned and not used: Deadzone
#pragma warning disable 414     //Variable assigned and not used: mousePosition, occluded

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Debug = FFP.Debug;

//! 3rd person camera controller.
/*!
 * Child Class of Parent Camera_2 for Patriots of the Past.
 * Inherits initial camera settings from Camera_2.
 * Uses mouse input to orbit camera in a 3D space and mouse wheel for zoom, input works with joysticks.
 * Camera follows character if no mouse input is detected.
 * Includes methods to handle player occlusion and object/environment clipping
 */
[RequireComponent(typeof(CheckTargets))]
public sealed class PoPCamera : Camera_2
{
	private static PoPCamera _instance;

	// Ensures PoPCamera is in the scene when attempting to access it
	public static PoPCamera instance
	{
		get 
		{
			_instance = _instance ?? (_instance = GameObject.FindObjectOfType<PoPCamera>()); 
			if(_instance == null) {
				Debug.Warning ("camera", "PoPCamera is not in scene but a script is attempting to reference it.");
				return null;
			} else {
				return _instance;
			}
		}
	}

    public static CameraState State
    {
        get { return _curState; }
        set { _curState = value; }
    }

	/// Tracks mouse position between frames
	private Vector3 mousePosition;

	///\ingroup Event Members
	///@{ Hidden references for local camera events
	[HideInInspector] public PoPCameraEvent eventTrigger;
	[HideInInspector] public GoSpline eventPath;
	///@}
	
	private float curRotateX = 0f;

	///\ingroup Layers
	///@{ Set our layers for different objs in the scene
	private int noOcclusionLayer = 8;
	private int playerLayer = 9;
	private int stealthObjsLayer = 10;
	private int IgnoreRaycastLayer = 2;
	/// Initialize our actual 32bit layermasks
	public int PlayerLM = 1;
	public int NoOcclusionLM = 1;
    public int IgnoreTargetLM = 1;
	///@}

	private bool occluded = false;						                //!< Used to check if camera is compensating for occlusion
	[ReadOnly] public float targetingRange = 20f;		                //!< Range allowed to target objects
	[ReadOnly] public float screenTargetArea = 20f;		                //!< Area of screen object can be targeted
	[HideInInspector] public int targetindex = 0;	                    //!< Index in list of target objects to look at
    private float targetResetTimer = 0f;		                        //!< Timeframe camera resets after losing track of target
    private float resettingTimer = 0f;                                  //!< Temp fix for camera getting stuck in target reset
	private List<GameObject> targetedObjects = new List<GameObject>();	//!< List of objects player is targeting
	private List<GameObject> allTargetables = new List<GameObject>();	//!< Master List of all available targets in scene
    private Shader targetOutline;
    private Shader standardShader;
    private Dictionary<string, Color> outlineColors = new Dictionary<string, Color>
    {
        { "default", new Color(0, 255, 253) },
        { "enemy", Color.red },
        { "push", Color.green },
        { "soundthrow", Color.magenta },
        { "taze", Color.cyan }
    };

	void Awake()
	{
        _instance = null;
        player = target;
	}

	void Start()
	{
		if (!target) {
            if (GameObject.FindObjectOfType<QK_Character_Movement>())
            {
                target = (Transform)GameObject.FindObjectOfType<QK_Character_Movement>().transform;
				player = target;
                _curState = CameraState.Normal;
			}else{
				Debug.Error ("camera","Cannot find this.target. Please connect the GameObject to the component using the inspector.");
				target = transform;
			}
		}

        standardShader = Shader.Find("Standard");
        targetOutline = Resources.Load<Shader>("TargetOutline");
        if(targetOutline == null)
        {
            Debug.Log("ui", "Could not find targetOutline shader");
            targetOutline = Shader.Find("Standard");
        }

		Go.defaultUpdateType = GoUpdateType.FixedUpdate;
		distance = Mathf.Clamp(distance, distanceMin, distanceMax);
		cameraLatency = Mathf.Clamp (cameraLatency, 0.05f, 1f);
		// Bit Shift our layermasks
		PlayerLM = 1 << playerLayer | 1 << IgnoreRaycastLayer;
		NoOcclusionLM = 1 << noOcclusionLayer | 
            1 << playerLayer |
			1 << stealthObjsLayer |
			1 << IgnoreRaycastLayer;
		// Inverse masks
		PlayerLM = ~PlayerLM;
		NoOcclusionLM = ~NoOcclusionLM;
		Reset();
	}

	/*
	 * This Update functions serves to decide whether we should be targeting or
	 * not and which specific target in the group (if there is a group) to be
	 * locked on. Since these functions query input we get a better result by
	 * having it in vanilla Update instead of Fixed Update.
	 */
	void Update() 
	{
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reset();
        }
        switch (_curState)
        {
            case CameraState.Normal:
                if (InputManager.input.isTargetPressed() && AcquireTarget().Count != 0f)
                {
                    _curState = CameraState.TargetLock;
                }
                break;

            case CameraState.TargetLock:
                if (InputManager.input.isTargetPressed())
                {
					targetedObjects[targetindex].GetComponent<Item>().is_targeted = false;
                    _curState = CameraState.TargetReset;
                }
                else
                {
					if(!GameHUD.Instance.skillsOpen && targetedObjects.Count != 0) 
					{
						targetindex += Input.GetMouseButtonDown(1) ? 1 : 0;
						targetindex = targetindex < 0 ? targetedObjects.Count - 1 :
							Mathf.Abs(targetindex % targetedObjects.Count);
						targetedObjects[targetindex].GetComponent<Item>().is_targeted = true;
					}
                }
                break;
        }
	}

	/*
	 * The bulk of PoP's 3rd person camera controllers implementation.
	 * Determines whether camera should be in an event, behind player, or in targeting mode.
	 * Tracks mouse and player position at the end of the frame for use during frame.
	 */
	void FixedUpdate()
	{
        #if UNITY_EDITOR
        /** Swap material color of targeted objects to debug targeting **/
        if (Debug.IsKeyActive("camera"))
        {
            foreach (GameObject go in allTargetables)
            {
                go.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            }
        }
        #endif

        switch (_curState)
        {
            /*** Normal Camera Behavior ***/
            case CameraState.Normal:
                targetLookAt = player.position;
				
				#if UNITY_EDITOR
				if(!Input.GetKey(KeyCode.LeftShift))
                	HandleMouseInput();
				#else
				HandleMouseInput();
				#endif

                var count = 0;

                do
                {
                    CalculateDesiredPosition();
                    count++;
                } while (CheckifOccluded(count));

                UpdatePosition();
                break;
            /****************************/

            /*** Target Lock Behavior ***/
            case CameraState.TargetLock:
                resettingTimer = 2f;
                if (targetedObjects.Count == 0f)
                {
					// Attempt to get targets, if no targets found break out
                    targetedObjects = AcquireTarget();
					if(targetedObjects.Count == 0) {
						_curState = CameraState.TargetReset;
						break;
					}
                }

                RaycastHit hit;
                Physics.Raycast(transform.position, (targetedObjects[targetindex].transform.position - transform.position), out hit, Mathf.Infinity, NoOcclusionLM);
                if (hit.collider.name != targetedObjects[targetindex].GetComponent<Collider>().name)
                {
                    targetResetTimer -= Time.deltaTime;
                }
                else
                {
                    targetResetTimer = 3f;
                }

                TargetLockCamera(targetindex);

                if (targetResetTimer <= 0f)
                {
                    _curState = CameraState.TargetReset;
                }
                else
                {
                    UpdatePosition();
                }

                #if UNITY_EDITOR
                /** Color target green for debug purposes **/
                if (Debug.IsKeyActive("camera"))
                    targetedObjects[targetindex].GetComponent<Renderer>().material.color = Color.green;
                #endif

                break;
            /***********************/

            /*** Reset Targeting ***/
            case CameraState.TargetReset:
                TargetReset();
				CalculateDesiredPosition();
				UpdatePosition();
				Quaternion rotation = Quaternion.LookRotation(targetLookAt - transform.position);
				if (transform.rotation == rotation || resettingTimer <= 0f)
				{
					_curState = CameraState.Normal;
				}
                resettingTimer -= Time.deltaTime;
				break;
			/***********************/

            /*** Camera Event ***/
            case CameraState.CamEvent:
                eventTrigger.Event();
                break;
            /********************/

            /*** Paused State ***/
            case CameraState.Pause:
                UpdatePosition();
                break;
            /********************/
        }
        
		mousePosition = Input.mousePosition;
        HighlightTarget();
	}

	void CalculateDesiredPosition(float rotateY, float rotateX) {
		ResetOccludedDistance();

		// interpolate from current distance to desired
		distance = Mathf.Lerp(distance, desiredDistance, distanceSmooth);

		// Pass mouse input into function to calculate new position
		desiredPosition = CalculatePosition(rotateY, rotateX, distance);
	}

	// Handle all player mouse input and prepare for camera position calculation
	void HandleMouseInput()
	{
		var deadzone = 0.01f;

		// Takes mouse input if mouse is moving and increments camera rotation dependent variables
		// If mouse has been inactive for specified time period camera will become a followcam once player moves
		if(_curState == CameraState.Normal) {
			mouseX += InputManager.input.CameraHorizontalAxis() * xMouseSensitivity;
			mouseY -= InputManager.input.CameraVerticalAxis() * yMouseSensitivity;
		} else {
			curRotateX += InputManager.input.CameraHorizontalAxis() * xMouseSensitivity;
			mouseY -= InputManager.input.CameraVerticalAxis() * yMouseSensitivity;
			ClampAngle(curRotateX, curRotateX - 10f, curRotateX + 10f);
			Mathf.Clamp(mouseY, mouseY + 5f, mouseY - 5f);
		}

		//Limit Y-axis input
		mouseY = ClampAngle(mouseY, yMinLimit, yMaxLimit);
	}

	#region Targeting
    // Offsets camera position slightly to the right of the player during targeting, focuses target
	void TargetLockCamera(int i)
	{
		Vector3 player2DPos = new Vector3 (player.transform.position.x, 0, player.transform.position.z); 

		Vector3 toTarget = new Vector3(targetedObjects[i].transform.position.x, 0, targetedObjects[i].transform.position.z)
								- player2DPos;
		
		Vector3 toCamera = -Vector3.Normalize(toTarget);
		
		float angleFrom0 = Vector3.Angle ((player2DPos + Vector3.back) - player2DPos, toCamera);
		float angleFrom90 = Vector3.Angle ((player2DPos + Vector3.left) - player2DPos, toCamera);
		float angleFrom180 = Vector3.Angle ((player2DPos + Vector3.forward) - player2DPos, toCamera);
		float angleFrom270 = Vector3.Angle ((player2DPos + Vector3.right) - player2DPos, toCamera);

		if(angleFrom90 <= angleFrom270) {
			curRotateX = angleFrom0 - 20f;
		} else {
			curRotateX = angleFrom180 + 180f - 20f;
		}

		rotateSmooth = 0.4f;
		targetLookAt = targetedObjects[targetindex].transform.position;

		int count = 0;
		do {
			CalculateDesiredPosition(mouseY, curRotateX);
			count++;
		} while(CheckifOccluded(count));

        _curState = CameraState.TargetLock;
	}

	//! Finds objects available to be targeted. Returns a list with the GameObjects
	//! that are available to be targeted. **NOTE** this function only returns a list
	//! of objects in range, it doesn't actually make the camera target objects.
	/*! 
	 * \return the list of objects. Use List.Any() to check if any objects were returned 
	 */
	public static List<GameObject> AcquireTarget() 
	{
		List<GameObject> targets = new List<GameObject>();

		foreach(GameObject go in PoPCamera.instance.allTargetables) {
			if(go.GetComponent<Targetable>().isTargetable)
            {
				if(Vector3.Angle(_instance.transform.forward, go.transform.position - _instance.transform.position) <= _instance.screenTargetArea)
                {
					if(targets.Count == 0)
                    {
						targets.Add(go);
					}
                    else
                    {
						int i = 0;

						while(i < targets.Count && (Vector3.Distance(go.transform.position, _instance.transform.position) > Vector3.Distance(targets[i].transform.position, _instance.transform.position)))
                        {
							i++;
						}

						targets.Insert(i, go);
					}
				}
                //go.GetComponent<Renderer>().material.shader = Shader.Find("Standard");
			}
            //go.GetComponent<Renderer>().material.shader = Shader.Find("Standard");
		}

        /*if (targets.Count > 0)
        {
            targets[0].GetComponent<Renderer>().material.shader = PoPCamera.instance.targetOutline;
            targets[0].GetComponent<Renderer>().material.SetColor("_OutlineColor", PoPCamera.instance.outlineColors["default"]);
        }*/

        return targets;
	}

	// Returns current gameobject or null if none is targeted
	public GameObject CurrentTarget() {
		if(targetedObjects != null && targetedObjects.Count > 0)
			return targetedObjects[targetindex];
		else {
			Debug.Warning("camera", "No current object targeted");
			return null;
		}
	}

	public List<GameObject> GetAllTargets()
	{
		return allTargetables;
	}

	public void AddTargetable(GameObject obj)
	{
		allTargetables.Add (obj);
	}

    void HighlightTarget()
    {
        Renderer rend;
        if(_curState == CameraState.TargetLock && CurrentTarget() != null)
        {
            rend = CurrentTarget().GetComponent<Renderer>();
            rend.material.shader = targetOutline;
            rend.material.SetColor("_OutlineColor", outlineColors["default"]);
        }
        else if(AcquireTarget().Count != 0f)
        {
            rend = AcquireTarget()[0].GetComponent<Renderer>();
            rend.material.shader = targetOutline;
            rend.material.SetColor("_OutlineColor", outlineColors["default"]);
        }
        else
        {
            foreach(GameObject go in allTargetables)
            {
                rend = go.GetComponent<Renderer>();
                if(rend.material.shader != standardShader)
                {
                    rend.material.shader = standardShader;
                }
            }
        }
    }
	#endregion

	#region Occlusion Checking
	/// Checks if the target can see each point in the cameras near clipping plane
	/// If it can target is not occluded, if not target is occluded
	bool CheckifOccluded(int count)
	{
		var isOccluded = false;
		var nearestDistance = 0f;

		if(_curState == CameraState.TargetLock) {
			nearestDistance = CheckCameraPoints(target.position, desiredPosition);
		} else {
			nearestDistance = CheckCameraPoints(target.position, desiredPosition);
		}

			if(nearestDistance != -1) {
				if(count < 10) {
					isOccluded = occluded = true;
					distance -= occlusionDistanceMove;
					if(_curState == CameraState.TargetLock)
						curRotateX += 0.01f;

					if(distance < 0.25f)
						distance = 0.25f;
				} else
					distance = nearestDistance - Camera.main.nearClipPlane;

				desiredDistance = distance - 0.5f;
				distanceSmooth = distanceResumeSmooth;
			}

		return isOccluded;
	}

	/// Creates near clip plane points, checks if something is between target and points
	/// Returns -1 if target is not occluded, returns nearest point to player if occluded
	float CheckCameraPoints(Vector3 from, Vector3 to)
	{
		var NearDistance = -1f;
		RaycastHit HitInfo;

		ClipPlanePoints PlanePoints = NearClipPlane (to);

		if (Physics.Linecast (from, PlanePoints.UpperLeft, out HitInfo, NoOcclusionLM) && HitInfo.collider.gameObject.name != "_Player")
			NearDistance = HitInfo.distance;
		if (Physics.Linecast (from, PlanePoints.LowerLeft, out HitInfo, NoOcclusionLM) && HitInfo.collider.gameObject.name != "_Player")
			if(HitInfo.distance < NearDistance || NearDistance == -1)
				NearDistance = HitInfo.distance;
		if (Physics.Linecast (from, PlanePoints.UpperRight, out HitInfo, NoOcclusionLM) && HitInfo.collider.gameObject.name != "_Player")
			if(HitInfo.distance < NearDistance || NearDistance == -1)
				NearDistance = HitInfo.distance;
		if (Physics.Linecast (from, PlanePoints.LowerRight, out HitInfo, NoOcclusionLM) && HitInfo.collider.gameObject.name != "_Player")
			if(HitInfo.distance < NearDistance || NearDistance == -1)
				NearDistance = HitInfo.distance;
		if (Physics.Linecast (from, to + transform.forward * -GetComponent<Camera>().nearClipPlane, out HitInfo, NoOcclusionLM) && HitInfo.collider.gameObject.name != "_Player")
			if(HitInfo.distance < NearDistance || NearDistance == -1)
				NearDistance = HitInfo.distance;

		return NearDistance;
	}

	 // Resets camera position to preoccluded position
	 protected override void ResetOccludedDistance()
	 {
		 Vector3 pos;
		 if(desiredDistance < preOccludedDistance)
		 {
			 if(_curState == CameraState.TargetLock)
				pos = CalculatePosition(mouseY, curRotateX, preOccludedDistance);
			 else
				pos = CalculatePosition(mouseY, mouseX, preOccludedDistance);

			var nearestDistance = CheckCameraPoints(target.position, pos);

			if(nearestDistance == -1 || nearestDistance > preOccludedDistance)
			{
				desiredDistance = preOccludedDistance;
				occluded = false;
			}
			else
				desiredDistance = nearestDistance - Camera.main.nearClipPlane;
		 }
	 }

	 public struct ClipPlanePoints {
		 public Vector3 UpperLeft;
		 public Vector3 UpperRight;
		 public Vector3 LowerLeft;
		 public Vector3 LowerRight;
	 }

	 /// Creates 4 Vector3 points to represent cameras near clipping plane
	 /// Used to determine player occlusion
	 public static ClipPlanePoints NearClipPlane(Vector3 position) {
		 var PlanePoints = new ClipPlanePoints();

		 if(Camera.main == null)
			 return PlanePoints;

		 var transform = Camera.main.transform;
		 var halfFOV = (Camera.main.fieldOfView / 2) * Mathf.Deg2Rad;
		 var aspect = Camera.main.aspect;
		 var distance = Camera.main.nearClipPlane;
		 var height = distance * Mathf.Tan(halfFOV);
		 var width = height * aspect;

		 PlanePoints.LowerRight = position + transform.right * width;
		 PlanePoints.LowerRight -= transform.up * height;
		 PlanePoints.LowerRight += transform.forward * distance;

		 PlanePoints.LowerLeft = position - transform.right * width;
		 PlanePoints.LowerLeft -= transform.up * height;
		 PlanePoints.LowerLeft += transform.forward * distance;

		 PlanePoints.UpperRight = position + transform.right * width;
		 PlanePoints.UpperRight += transform.up * height;
		 PlanePoints.UpperRight += transform.forward * distance;

		 PlanePoints.UpperLeft = position - transform.right * width;
		 PlanePoints.UpperLeft += transform.up * height;
		 PlanePoints.UpperLeft += transform.forward * distance;

		 return PlanePoints;
	 }
	#endregion

    public void StartDialog(GameObject targetObj)
    {
        target = targetObj.transform;
        _curState = CameraState.Pause;
    }

	// Resets camera variables to default values
	public void Reset()
	{
		target = player;
		mouseX = 0;
		mouseY = 10;
		cameraLatency = 0.2f;
		distance = 5f;
		desiredDistance = distance;
		preOccludedDistance = desiredDistance;
		if (targetedObjects.Count > 0) {
			targetedObjects.Clear ();
			targetedObjects.TrimExcess ();
		}
        targetResetTimer = 3f;
		_curState = CameraState.Normal;
	}

    // Resets camera back to looking at player and normal state
    public void SoftReset()
    {
        target = player;
        _curState = CameraState.Normal;
    }

    // Used to smoothly transition camera back to player after targeting
    private void TargetReset()
    {
        targetedObjects.Clear();
        targetedObjects.TrimExcess();
        targetindex = 0;
        targetLookAt = player.position;
    }

	// Clamps a given angle to 360, then clamps that to min/max 
	public static float ClampAngle(float angle, float min, float max)
	{
		do
		{
			if(angle < -360)
				angle %= 360;
			if(angle > 360)
				angle %= 360;
		} while(angle < -360 || angle > 360);

		return Mathf.Clamp(angle, min, max);
	}

	// Draws Gizmos when Camera is selected in scene editor to assist in targetable object placing
	void OnDrawGizmosSelected()
	{
#if UNITY_EDITOR

        if (Debug.IsKeyActive ("camera")) {
			foreach (GameObject target in allTargetables) {
				if (Vector3.Distance (this.target.position, target.transform.position) <= target.GetComponent<Targetable> ().range) {
					if (target.GetComponent<Targetable> ().isTargetable) {
						if (Vector3.Angle (this.transform.forward, target.transform.position - this.transform.position) <= screenTargetArea) {
							Gizmos.color = Color.green;
							Gizmos.DrawRay (target.transform.position, (this.transform.position - target.transform.position));
						} else {
							Gizmos.color = Color.red;
							Gizmos.DrawRay (target.transform.position, (this.transform.position - target.transform.position));
						}
					} else {
						Gizmos.color = Color.red;
						Gizmos.DrawRay (target.transform.position, (this.transform.position - target.transform.position));
					}
				}
			}

			Gizmos.color = new Color (1, 1, 1, 0.5f);
			Gizmos.DrawSphere (this.target.position, targetingRange);
		}
#endif

    }
}
