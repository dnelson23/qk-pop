using UnityEngine;
using System.Collections.Generic;

//! Base class for the different input types
public abstract class InputType : MonoBehaviour
{
    // Dictionaries that hold all inputs. Input list is read from JSON file
    public Dictionary<string, string> keyButtons { get; protected set; }
    public Dictionary<string, string> controllerButtons { get; protected set; }

    public virtual string InputString() {
		return "";
	}
	public virtual bool GetKey(string keyPressed) {
		return false;
	}
	public virtual float CameraVerticalAxis() {
		return 0;
	}
	public virtual float CameraHorizontalAxis() {
		return 0;
	}
	public virtual float MoveVerticalAxis() {
		return 0;
	}
    public virtual float MoveHorizontalAxis()
    {
		return 0;
	}
	public virtual bool isCrouched() {
		return false;
	}
	public virtual bool isSprinting() {
		return false;
	}
	public virtual bool isJumping() {
		return false;
	}
	public virtual bool isActionPressed() {
		return false;
	}
	public virtual bool AbilityPressed() {
		return false;
	}
	public virtual bool isTargetPressed() {
		return false;
	}
	public virtual bool isCameraResetPressed() {
		return false;
	}
	public virtual bool isAbilityEquip() {
		return false;
	}
	public virtual bool isNotifications() {
		return false;
	}
	public virtual bool isCompass() {
		return false;
	}
	public virtual bool isJournal() {
		return false;
	}
	public virtual int CameraScrollTarget() {
		return 0;
	}
	public virtual int ScrollTarget() {
		return 0;
	}
	public virtual bool isStart() {
		return false;
	}

	public virtual void SaveInput(){
	}

	public virtual void LoadInput(){
	}
}
