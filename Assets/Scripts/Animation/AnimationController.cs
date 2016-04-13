using UnityEngine;
using System.Collections;
/// <summary>
/// Class created to handle Gavyn animations
/// </summary>
public class AnimationController : MonoBehaviour {
	Animator animator;
    bool jumping;
    bool crouching;
    bool sprinting;
    int movement;
    bool running;
	bool ladder;
	bool sidle;
	int selectedAbility;
	bool Pull;
	bool Push;
	bool Skip;
	bool Stun;
	bool SwordSwing;
	bool PhoneOut;
	bool PowerUsed;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();	
	}
	
	void FixedUpdate () 
	{
		//If the player moves vertically, assign the integer value from the input to the movement parameter from the animation controller
		if(InputManager.input.MoveVerticalAxis() != 0) {
			movement = (int)InputManager.input.MoveVerticalAxis();
			//If the player moves horizontally, assign the integer value from the input to the turnRight parameter from the animation controller
		} else if(InputManager.input.MoveHorizontalAxis() != 0) {
			movement = (int)InputManager.input.MoveHorizontalAxis();
		} else {
			//If the player don't move vertically or horizontally, the parameter will be set to zero so no 'movement' animation will occur
			movement = 0;
		}
		//I am using the speed value from QK_Character_Movement script
		if((QK_Character_Movement.Instance.curSpeed >= QK_Character_Movement.Instance.runSpeed)&& (QK_Character_Movement.Instance._moveState!= QK_Character_Movement.CharacterState.Sprint)) {
			running = true;
		} else {
			running = false;
		}
		//Checks if the character is climbing a ladder or not
		if(QK_Character_Movement.Instance._moveState == QK_Character_Movement.CharacterState.Ladder)
			ladder = true;
		else
			ladder = false;
		//Checks if the character is sidling
		if(QK_Character_Movement.Instance._moveState == QK_Character_Movement.CharacterState.Sidle)
			sidle = true;
		else
			sidle = false;
		//Checks if something is targeted or not
		if(PoPCamera.State == PoPCamera.CameraState.TargetLock)
			PhoneOut = true;
		else
			PhoneOut = false;
		//Sets the selected ability bool
		selectedAbility = AbilityDockController.instance.getSelectedAbility ();
		Debug.Log (selectedAbility);
		if (selectedAbility == 0) {
			Pull = true;
			Push = false;
			Stun = false;
			Skip = false;
			SwordSwing = false;
		} else if (selectedAbility == 1) {
			Pull = false;
			Push = true;
			Stun = false;
			Skip = false;
			SwordSwing = false;
		} else if (selectedAbility == 2) {
			Pull = false;
			Push = false;
			Stun = true;
			Skip = false;
			SwordSwing = false;
		} else if (selectedAbility == 3) {
			Pull = false;
			Push = false;
			Stun = false;
			Skip = true;
			SwordSwing = false;
		} else if (selectedAbility == 4) {
			Pull = false;
			Push = false;
			Stun = false;
			Skip = false;
			SwordSwing = true;
		}
		if(InputManager.input.AbilityPressed ()){
			PowerUsed = true;
		}
		else{
			PowerUsed = false;
		}
		//Set the values of the parameters from the animation controller
		jumping = Input.GetButton("Jump");
        crouching = false;
        sprinting = false;
		animator.SetInteger("Movement", movement);
        animator.SetBool("Jump", jumping);
        animator.SetBool("Crouch", crouching);
        animator.SetBool("isSprinting", sprinting);
        animator.SetBool("isRunning", running);
		//animator.SetBool("Ladder", ladder);
		animator.SetBool("Sidle", sidle);
		animator.SetBool("Pull", Pull);
		animator.SetBool("Push", Push);
		animator.SetBool("Skip", Skip);
		animator.SetBool("Stun", Stun);
		animator.SetBool("SwordSwing", SwordSwing);
		animator.SetBool ("Phone Out", PhoneOut);
		animator.SetBool ("PowerUsed", PowerUsed);
	}
}
