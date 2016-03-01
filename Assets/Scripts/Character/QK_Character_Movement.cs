#pragma warning disable 414     //Variable assigned and not used: slideSpeed, groundNormal, slideTheshold, MaxControllableSlideMagnitude, slideDirection, targetAngle

using UnityEngine;
using System.Collections;
using Debug = FFP.Debug;
//using CharacterState = CharacterStates;
public class QK_Character_Movement : MonoBehaviour {

	private static QK_Character_Movement _instance;
	public static QK_Character_Movement Instance 
	{
		get 
		{
			_instance = _instance ?? (_instance = GameObject.FindObjectOfType<QK_Character_Movement>());
			if(_instance == null) {
				Debug.Warning("player", "Character Controller is not in scene but a script is attempting to reference it.");
			}
			return _instance;
		}
	}

	//moved to seperate enum script
	public enum CharacterState {Idle, Move, Pivot, Sprint, Crouch, Hang, Ladder, Sidle, Wait, Normal}
	public CharacterState _moveState { get; private set; }
	public CharacterState _stateModifier { get; set; }

	public static CharacterController charCont;

	public GameObject LedgeDetect;

	[ReadOnly] public float curSpeed = 0f;
	private float acceleration = 0.3f;
	[ReadOnly] public float runSpeed = 8f;
	private float sprintSpeed = 12f;
	private float crouchSpeed = 4f;
	[ReadOnly] public float jumpSpeed = 8f;
	private float slideSpeed = 8f;
	private float gravity = 30f;
	[ReadOnly] public float verticalVelocity = 0f;
	private float terminalVelocity = 30f;
	private float turnRate = 5f;

	private Vector3 moveVector = Vector3.zero;
	private Vector3 desiredMoveVector = Vector3.zero;
    private Vector3 inputDirection { get; set; }
	private Vector3 groundNormal = Vector3.zero;

	private Interactable iObject;
	private GameObject tempObj;
	private GameObject triggeredObj;

    // Ladder Variables
    private bool onLadder = false;
	private bool dismountTop = false;
	private bool dismountBottom = false;
    private Vector3 climbToPosition = Vector3.zero;
	private Vector3 ladderDismountPos = Vector3.zero;

	// Ledge Variables
	private bool onLedge = false;
	RaycastHit ledgeTest;
	public GameObject ledge = null;

	// This is for Slide if implemented
	private float slideTheshold = 0.6f;
	private float MaxControllableSlideMagnitude = 0.4f;
	private Vector3 slideDirection;
	private Quaternion targetAngle = Quaternion.identity;
	private PoPCamera cam;

    void Awake()
    {
        _instance = null;
    }

	// Use this for initialization
	void Start () 
	{
		charCont = this.GetComponent<CharacterController>();
		cam = PoPCamera.instance;
		_moveState = CharacterState.Idle;
		_stateModifier = CharacterState.Normal;

		// Set CharacterController Defaults
		charCont.slopeLimit = 30f;
	}

	void FixedUpdate()
	{
		if (cam == null)
			return;

		CalculateMovementDirection ();


		ApplyGravity ();


		DetermineCharacterState ();

		switch (_stateModifier) 
		{
			case CharacterState.Ladder:
				ClimbLadder();
				break;
			case CharacterState.Hang:
				ClimbLedge();
				break;
			default:
				ProcessStandardMotion();
				break;
			
		}
	}

    void ProcessStandardMotion()
	{
		if (Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0) {
			curSpeed += acceleration;
			curSpeed *= inputDirection.magnitude;
		} else {
			curSpeed -= acceleration;
		}
		
		if (_stateModifier == CharacterState.Sprint) {
			curSpeed = Mathf.Clamp (curSpeed, 0f, sprintSpeed);
		} else if (_stateModifier == CharacterState.Crouch) {
			curSpeed = Mathf.Clamp (curSpeed, 0f, crouchSpeed);
		} else {
			curSpeed = Mathf.Clamp (curSpeed, 0f, runSpeed);
		}

        //curSpeed *= desiredMoveVector.magnitude;
		
		// Apply Slide
		ApplySlide ();
		
		moveVector = new Vector3 (desiredMoveVector.x, verticalVelocity, desiredMoveVector.z);

		Vector3 curMoveVect = new Vector3(moveVector.x * curSpeed, verticalVelocity, moveVector.z * curSpeed);

		if (_moveState != CharacterState.Idle)
			RotateCharacter (inputDirection);

		charCont.Move (curMoveVect * Time.deltaTime);
	}

	void CalculateMovementDirection ()
	{
		float inputHor = Input.GetAxisRaw("Horizontal");
		float inputVert = Input.GetAxisRaw("Vertical");
		Vector3 forward = transform.position + cam.transform.forward;
		forward = new Vector3(forward.x, transform.position.y, forward.z);
		forward = Vector3.Normalize(forward - transform.position);
		Vector3 right = new Vector3(forward.z, 0f, -forward.x);

		inputDirection = Vector3.Normalize((inputHor * right) + (inputVert * forward));
		desiredMoveVector = Vector3.Lerp (moveVector, inputDirection, 0.2f);
		desiredMoveVector = new Vector3 (desiredMoveVector.x, 0f, desiredMoveVector.z);

		if (inputHor != 0f || inputVert != 0) 
        {
            if (_moveState == CharacterState.Idle)
            {
                _moveState = CharacterState.Pivot;
            }
            else if (_moveState == CharacterState.Pivot)
            {
                if (Vector3.Dot(Vector3.Normalize(transform.forward), Vector3.Normalize(inputDirection)) >= 0.9)
                    _moveState = CharacterState.Move;
                else
                    _moveState = CharacterState.Pivot;
            }
            else
            {
                if (Vector3.Angle(Vector3.Normalize(transform.forward), Vector3.Normalize(inputDirection)) >= 90f)
                {
                    _moveState = CharacterState.Pivot;
                } else {
                    _moveState = CharacterState.Move;
                }
            }
		} 
        else 
        {
			_moveState = CharacterState.Idle;
		}
	}

	void ApplyGravity () 
	{
        if (!charCont.isGrounded && moveVector.y > -terminalVelocity) {
			verticalVelocity -= gravity * Time.deltaTime;
		} else if(charCont.isGrounded) {
			verticalVelocity = -1;
		}
	}

	void ApplySlide ()
	{
		if (!QK_Character_Movement.charCont.isGrounded)
			return;

		slideDirection = Vector3.zero;

		RaycastHit hitInfo;

		if (Physics.Raycast(transform.position, Vector3.down, out hitInfo, 5f, cam.PlayerLM)) {
			groundNormal = hitInfo.normal;
			if (Vector3.Angle (hitInfo.normal, Vector3.up) < charCont.slopeLimit) {
				slideDirection = new Vector3 (hitInfo.normal.x, -hitInfo.normal.y, hitInfo.normal.z);
			}
		}

		/*if (slideDirection.magnitude < MaxControllableSlideMagnitude) {
			moveVector += slideDirection;
		}
		else {
			moveVector = slideDirection;
		}*/
	}
	
	void DetermineCharacterState () 
	{
		if(!IsInActionState())
		{
			if (Input.GetKey(KeyCode.E)) 
			{
				iObject = GetActionObject();

				if(iObject != null)
				{
					if(iObject.Type == Interactable.ObjectType.Ladder)
					{
						_stateModifier = CharacterState.Ladder;
					}
					else if(iObject.Type == Interactable.ObjectType.Sidle)
					{
						_stateModifier = CharacterState.Sidle;
					}
					else if(iObject.Type == Interactable.ObjectType.Door)
					{
						_stateModifier = CharacterState.Wait;
					}
					else 
					{
						Debug.Warning("player", "No player action for type "+iObject.Type);
						_stateModifier = CharacterState.Normal;
					}

					// We have an action to do, break out
					return;
				}
			}

			if (Input.GetKeyDown(KeyCode.Space)) 
			{
				tempObj = GetLedge();//todo
			}

			if (Input.GetButton("Jump")) {
				Jump ();
				return;
			}

			if (false) {
				_stateModifier = CharacterState.Sprint;
			} else if (false) {
				_stateModifier = CharacterState.Crouch;
			} else {
				_stateModifier = CharacterState.Normal;
			}
		}
	}

	//todo capsulecast up
	GameObject GetLedge()
	{

		//instantiate game object to "cast"
		//on collision inside helper script the game determines if player should jump to it
		Vector3 tempLoc = this.gameObject.transform.position;
		tempLoc.y += 3f;
		GameObject detector = Instantiate(LedgeDetect, tempLoc, this.transform.rotation) as GameObject;
		Physics.IgnoreCollision(this.transform.GetComponent<Collider>(), detector.transform.GetComponent<Collider>(), true);
		//Destroy (detector.gameObject);

		if (ledge != null) {
			triggeredObj = ledge;
			onLedge = true;
			//return ledge.GetComponent<Interactable>();
			return ledge;
		} else {
			return null;
		}

	}

	Interactable GetActionObject()
	{
		GameObject actionableObj = null;
		RaycastHit[] hit = Physics.SphereCastAll(transform.position + charCont.center, 1.5f, Vector3.forward, 10);
		if(hit.Length > 0)
		{
			foreach(RaycastHit obj in hit)
			{
				if(GetComponentInHeirarchy<Interactable>(obj.collider.gameObject))
				{
					if(actionableObj == null)
						actionableObj = obj.collider.gameObject;
					else 
					{
						Vector3 toActionObj = actionableObj.transform.position - transform.position;
						toActionObj = new Vector3(toActionObj.x, 0f, toActionObj.z);
						Vector3 toNewObj = obj.collider.transform.position - transform.position;
						toNewObj = new Vector3(toNewObj.x, 0f, toNewObj.z);
						
						if(Vector3.Angle(toActionObj, transform.forward) < 45f && 
						   Vector3.Angle(toNewObj, transform.forward) < 45f)
						{
							if(Vector3.Distance(actionableObj.transform.position, transform.position) >
							   Vector3.Distance(obj.transform.position, transform.position))
							{
								actionableObj = obj.collider.gameObject;
							}
						}
						else if(Vector3.Angle(toNewObj, transform.forward) <
						        Vector3.Angle(toActionObj, transform.forward))
						{
							actionableObj = obj.collider.gameObject;
						}
					}
				}
			}
		}
		if(actionableObj != null) {
			triggeredObj = actionableObj;
			return GetComponentInHeirarchy<Interactable>(actionableObj);
		} else
			return null;
	}
	void Jump() 
	{
		if (charCont.isGrounded)
			verticalVelocity = jumpSpeed;
	}


	void ClimbLedge()
	{
		if (tempObj == null) {
			// For some reason we're trying to climb something thats not a ledge
			_stateModifier = CharacterState.Normal;
			return;
		}
		if (onLedge) {
			//this.gameObject.transform.position = iObject.gameObject.transform.position;
			//find the position on the ledge that the player is supposed to be at
			//move left and right as needed
			if (Input.GetKeyDown(KeyCode.S)) 
			{

				if(_stateModifier == CharacterState.Hang){
					_stateModifier = CharacterState.Normal;
					ledge = null;
					onLedge = false;
				}
			}
			if (Input.GetKeyDown(KeyCode.Space)) 
			{
				if (Input.GetKeyUp(KeyCode.Space)) 
				{
					if(_stateModifier == CharacterState.Hang){
						_stateModifier = CharacterState.Normal;
						ledge = null;
						onLedge = false;
					}
				}

			}
			if (Input.GetKey(KeyCode.A)){
				//move left
				transform.position = Vector3.MoveTowards(transform.position, ledge.GetComponent<QK_Ledge>().getLeftPoint().transform.position, 0.04f);

			}
			if (Input.GetKey (KeyCode.D)){
				//move right
				transform.position = Vector3.MoveTowards(transform.position, ledge.GetComponent<QK_Ledge>().getRightPoint().transform.position, 0.04f);
			}
			if (Input.GetKeyDown (KeyCode.W)){
				//climb ledge
				if (Input.GetKeyDown (KeyCode.W)){
						Vector3 tempPos = transform.position;
						tempPos.y += 3f;
						transform.position = tempPos;
						if(_stateModifier == CharacterState.Hang){
							_stateModifier = CharacterState.Normal;
							ledge = null;
							onLedge = false;
						}
				}
				
			}
		}
	}


	void ClimbLadder()
	{
		if (iObject == null || iObject.Type != Interactable.ObjectType.Ladder)
		{
			// For some reason we're trying to climb something thats not a ladder
			_stateModifier = CharacterState.Normal;
			return;
		}

		// Snap to ladder
		if(!onLadder)
		{
            // Just use an estimated distance check because absolute position checking is inaccurate
            if (Vector3.Distance(transform.position, triggeredObj.transform.position) < 0.01f) {
				// Ready to begin climbing
                onLadder = true;
				climbToPosition = triggeredObj.transform.position;
            } else {
                transform.position = Vector3.Lerp(transform.position, triggeredObj.transform.position, 2 * Time.deltaTime);
				transform.LookAt(transform.position - iObject.transform.forward);
            }

			return;
		}

		Vector3 bottom = iObject.ladderBottom;
		Vector3 top = iObject.ladderTop;
		Vector3 climbDir = Vector3.zero;

		// Check if we're at top or bottom
		if(dismountTop)
		{
			// Do dismount top
			transform.position = Vector3.Lerp(transform.position, top + transform.forward / 1.5f, Time.deltaTime * 2);

			if(Vector3.Distance(transform.position, top + transform.forward / 1.5f) <= 0.01f)
				Reset();
			return;
		}
		else if(dismountBottom) 
		{
			// Do dismount bottom
			transform.position = Vector3.Lerp(transform.position, bottom - transform.forward / 1.5f, Time.deltaTime * 2);

			if(Vector3.Distance(transform.position, bottom - transform.forward / 1.5f) <= 0.01f)
				Reset();
			return;
		}

		if(Vector3.Distance(climbToPosition, transform.position) > 0.1f)
		{
			transform.position = Vector3.Lerp(transform.position, climbToPosition, 0.1f);
			return;
		}

		if(Input.GetAxisRaw("Vertical") > 0)
		{
			if(Vector3.Distance(transform.position, top) <= 0.5f)
			{
				if(Vector3.Distance(transform.position, top) <= 0.01f)
					dismountTop = true;
				else
					transform.position = Vector3.Lerp(transform.position, top, Time.deltaTime * 2);
				return;
			}
			// Set position to move up
			climbDir = Vector3.Normalize(top - transform.position)/1.5f;
		}
		else if(Input.GetAxisRaw("Vertical") < 0)
		{
			if(Vector3.Distance(transform.position, bottom) <= 0.5f)
			{
				if(Vector3.Distance(transform.position, bottom) <= 0.01f)
					dismountBottom = true;
				else
					transform.position = Vector3.Lerp(transform.position, bottom, Time.deltaTime * 2);
				return;
			}
			// Set position to move down
			climbDir = Vector3.Normalize(bottom - transform.position)/1.5f;
		}

		climbToPosition = transform.position + climbDir;
	}

	bool IsInActionState()
	{
		if (_stateModifier == CharacterState.Ladder ||
			_stateModifier == CharacterState.Sidle ||
			_stateModifier == CharacterState.Hang ||
			!charCont.isGrounded)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	void RotateCharacter(Vector3 toRotate)
	{
		Quaternion newRotation = Quaternion.LookRotation (toRotate, Vector3.up);
		transform.rotation = Quaternion.Slerp (transform.rotation, newRotation, turnRate * Time.deltaTime);
	}

	void Reset()
	{
		_moveState = CharacterState.Idle;
		_stateModifier = CharacterState.Idle;
		moveVector = Vector3.zero;

		iObject = null;
		triggeredObj = null;

		dismountBottom = false;
		dismountTop = false;
		onLadder = false;
		climbToPosition = Vector3.zero;
	}

	public T GetComponentInHeirarchy<T>(GameObject obj) 
	{
		if(obj.GetComponent<T>() != null) {
			return obj.GetComponent<T>();
		} else if(obj.transform.parent == null) {
			return default(T);
		} else {
			return GetComponentInHeirarchy<T>(obj.transform.parent.gameObject);
		}
	}

	void OnDrawGizmosSelected()
	{
#if UNITY_EDITOR
        if (cam && Debug.IsKeyActive("player")) {
			/*float inputHor = InputManager.input.MoveHorizontalAxis ();
			float inputVert = InputManager.input.MoveVerticalAxis ();
			Vector3 forward = transform.position + cam.transform.forward;
			forward = new Vector3 (forward.x, transform.position.y, forward.z);
			forward = Vector3.Normalize (forward - transform.position);
			Vector3 right = new Vector3 (forward.z, 0f, -forward.x);

			Vector3 moveDir = Vector3.Normalize((inputHor * right) + (inputVert * forward));

			Vector3 testMoveVect = Vector3.Lerp(moveVector, moveDir, 0.2f);
			testMoveVect = new Vector3(testMoveVect.x, 0f, testMoveVect.z);*/
			if(Input.GetKey(KeyCode.E)) 
			{
				Gizmos.DrawSphere(transform.position + charCont.center, 1.5f);
			}
		}
#endif
    }
}
