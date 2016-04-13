using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Debug = FFP.Debug;

public enum quincy_ability{
    Pull,
    Push,
    Stun,
    SoundThrow,
    Cut,
    Heat,
	Cool,
	Blast
};
/*! \brief Quincy:
 *         This class is used to store the Quincy abilities stats
 *         and verify that the ability is ready to fire.
 * 
 *  The stats includes the cooldown timers and strength of each abilities.
 *  This class only gather the target item from the camera, waits for the action key press
 *  and checks if the ability is ready to fire, if it is, it calls the appropriate function
 *  in the target's Item class. 
 *  The behavior of each abilities is in the Item class.
 * */
public sealed class Quinc : MonoBehaviour
{
//	/*
//START singleton code
	private static Quinc instance;

	static Quinc()
	{

	}

    void Awake()
    {
        instance = null;
    }

    public void Start()
    {
        abilitySelector = GameObject.Find("AbilityDock").GetComponent<AbilityDockController>();
    }

	public static Quinc Instance
	{
		get
		{
			return instance ?? (instance = GameObject.FindObjectOfType<Quinc>());
		}
	}
//	*/
//END singleton code
	
#pragma warning disable 0414
	public float pushRate = 1.0f;   //!< Push ability cooldown time.
	public float pushDistance = 5.0f;   //!< Push ability force.
	public float pushRange = 10.0f;
	private float nextPush = 1.0f;   //!< When Push ability will be ready.

	public float pullRate = 1.0f;
	public float pullDistance = 5.0f;
	public float pullRange = 10.0f;
	private float nextPull = 1.0f;

	public float cutRate = 0.5f;		//!< cut recharge rate
	public float cutRange = 20.0f;
	private float nextCut = 1.0f;		//!< timer for next cut

	public float soundRate = 0.5f;		//!< sound throw recharge rate
	private float nextSound = 1.0f;		//!< timer for sound throw
	public float soundRange = 20.0f;		//!< maximum distance between soundthrow location and enemy for it to have an effect

	public float stunRate = 0.5f;		//!< stun recharge rate
	public float stunRange = 2.0f;
	private float nextStun = 1.0f;		//!< timer for stun recharge
	public float stunTime = 20f;		//!< duration of stun effect

	public float heatRate = 0.5f;		//!< heat recharge rate
	public float heatRange = 20;
	private float nextHeat = 1.0f;		//!< timer for heat ability

	public float coldRate = 0.5f;		//!< freezing recharge rate
	public float coldRange = 20;
	private float nextCold = 1.0f;		//!< timer for cold ability

	private float blastRate = 0.5f;		//!< smoke/light blast recharge rate
	private int blastRange = 20;		//!< maximum distance from player to trigger blast
	private float nextBlast = 1.0f;		//!< timer for smoke/light blast

	private int blastRadius = 10;		//!< size of blast

	public quincy_ability activeAbility = quincy_ability.Push;

	public AbilityDockController abilitySelector;

	void FixedUpdate()
	{
		activeAbility = (quincy_ability)abilitySelector.getSelectedAbility();
		if(InputManager.input.AbilityPressed() && PoPCamera.State == PoPCamera.CameraState.TargetLock)
		{
		    try_ability(activeAbility);
		}
	}

	/*!
	 * This will call the apropriate behavior function in the Item class of an object.
	 * @param ability The ability type for wich it will call the apropriate function in Item.
	 * @param newTarget Optional parameter if you wish to use this function on an object that is not the camera target.
	 */
	void try_ability(quincy_ability ability, GameObject newTarget = null){
		if (ability_ready (ability)) {
			GameObject target = PoPCamera.instance.CurrentTarget();
			if(target && target.GetComponent<Item>()){
				Item script = target.GetComponent<Item>();

				switch(ability){
				case quincy_ability.Cut:
					script.Cut();
					break;

				case quincy_ability.SoundThrow:
					script.SoundThrow(soundRange);
					break;

				case quincy_ability.Stun:
					script.Stun(stunTime);
					break;

				case quincy_ability.Push:
					script.Push(gameObject.transform.position, pushDistance);
					break;

				case quincy_ability.Pull:
					script.Pull(gameObject.transform.position, pushDistance);
					break;

				case quincy_ability.Heat:
					script.Heat();
					break;

				case quincy_ability.Cool:
					script.Cool();
					break;

				case quincy_ability.Blast:
					script.Blast();
					break;
				}
			}
		}
	}

	/*!
	 * Checks if the given ability is ready to be used, and if it is, resets the cooldown.
	 * @param ability The ability that is checked.
	 */
	bool ability_ready(quincy_ability ability){

		switch (ability) {
		case quincy_ability.Cut:
			if(Time.time > nextCut){
				nextCut = Time.time + cutRate;
				return true;
			}
			else return false;

		case quincy_ability.SoundThrow:
			if(Time.time > nextSound){
				nextSound = Time.time + soundRate;
				return true;
			}
			else return false;

		case quincy_ability.Stun:
			if(Time.time > nextStun){
				nextStun = Time.time + stunRate;
				return true;
			}
			else return false;

		case quincy_ability.Push:
			if(Time.time > nextPush){
				nextPush = Time.time + pushRate;
				return true;
			}
			else return false;

		case quincy_ability.Pull:
			if(Time.time > nextPull){
				nextPull = Time.time + pullRate;
				return true;
			}
			else return false;

		case quincy_ability.Heat:
			if(Time.time > nextHeat){
				nextHeat = Time.time + heatRate;
				return true;
			}
			else return false;

		case quincy_ability.Cool:
			if(Time.time > nextCold){
				nextCold = Time.time + coldRate;
				return true;
			}
			else return false;

		case quincy_ability.Blast:
			if(Time.time > nextBlast){
				nextBlast = Time.time + blastRate;
				return true;
			}
			else return false;
		default:
			return false;
		}
	}
}
