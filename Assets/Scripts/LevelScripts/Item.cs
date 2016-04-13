using UnityEngine;
using System.Collections;

public enum item_type {
	Crate,
	Rope,
	Well,
	Enemy
};

[RequireComponent (typeof (Collider))]
[RequireComponent(typeof (Targetable))]
[System.Serializable]

/*
 *  Item Class
 *  This is used to store behaviors and variables specific to an item. There are currently
 *  four kinds of items defined by the enum above. Each item types has its own section for
 *  a different action caused by Quinc. The main functions Push, Pull, etc are called by Quinc.
 *  This script should ba added to any object that is potentially going to be targetted and used
 *  by Quinc.
 */

public class Item : MonoBehaviour
{
	public enum push_type {
		Free,
		TwoAxis,
		FourAxis,
		HeightAxis,
		Anim
	};

	public bool is_targeted = false;
    public item_type itemType;
    public push_type current_push_type;
	private Vector3 push_forward_direction = Vector3.forward;
	public float angle_offset = 0.0f;
	public int itemIndex;
    [EventVisible("Pushed X Times")]
    public int pushCounter;
    [EventVisible("Pulled X Times")]
    public int pullCounter;
    [EventVisible("Cut X Times")]
    public int cutCounter;
    [EventVisible("Sound Thrown X Times")]
    public int soundThrowCounter;
    [EventVisible("Stunned X Times")]
    public int stunCounter;
    [EventVisible("Affected by QuinC")]
    public bool quincAffected = false;
	public bool pushCompatible = false;
	public bool pullCompatible = false;
	public bool cutCompatible = false;
	public bool soundThrowCompatible = false;
	public bool stunCompatible = false;
	public bool heatCompatible = false;
	public bool coldCompatible = false;
	public bool blastCompatible = false;
	public float soundThrowRadius = 25f;
	
	//Crate variables
	public Vector3 startPosition;		//starting location of the crate
	public float pushPullRadius = 25f;	//maximum movement radius before crate snaps back to startPosition
	public float moveDist;				//distance moved by the crate
	public bool hasMoved;				//flag to track if crate has been moved by Quinc 
	public bool isSnapping;				//flag tracking if crate is currently snapping back
	Color originalColor;				//store the original color of the object while executing quinc functions
	float colorMax;						//maximum length of time the gameObject can remain a color other than the orginal color
	public float colorTime;				//the last time that the gameObject color was changed
	

	//Enemy variables
	public const float stunPeriod = 10.0f;
	public float curStunTimer;
	public bool stunState;
	public bool soundThrowAffected;

    public GameObject EffectPrefab;

    //!Player gathers X number of this item, (kill all enemies in area, use other item script to auto drop item into play inventory)
    protected virtual void GatherObjective (int count)
	{
		return;
	}

    //!Player arrives at the item, option with timed, use negative number for stall quest
    protected virtual void ArriveObjective (Vector3 buffer, float timer = 0.0f)
	{
		return;
	}

    //!This item is brought to target location
    protected virtual void EscortObjective (Vector3 location, Vector3 buffer)
	{
		return;
	}

	void Start(){
		startPosition = gameObject.transform.position;
	}

	void Update(){
		checkForSnapBack();
		if (is_targeted) {

		}
	}

	void OnGUI(){
		if (is_targeted) {
			GUI.color = Color.red;
			display_effects();
		}
	}

	void OnDrawGizmos(){
		drawPushPullDirections (current_push_type);
	}

	public void Stun(float time)
	{
		switch (itemType) {
		case item_type.Crate:
			if(stunCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;

		case item_type.Rope:
			if(stunCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;

		case item_type.Well:
			if(stunCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;

		case item_type.Enemy:
			StartCoroutine (_Stun (time));
			break;
		}
	}

	public void Cut(){
		switch (itemType) {
		case item_type.Crate:
			if(cutCompatible){
				print("Crate was destroyed by cut.");
			}
			else {
				NoEffect();
			}
			break;
			
		case item_type.Rope:
			if(cutCompatible){
				print("Rope was cut.");
			}
			else {
				NoEffect();
			}
			break;
			
		case item_type.Well:
			if(cutCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;
			
		case item_type.Enemy:
			if(cutCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;
		}
		//cutCompatible = false;
	}

	public void SoundThrow(float sound_range){
		switch (itemType) {
		case item_type.Crate:
			if(soundThrowCompatible){
				emmit_sound(sound_range);
			}
			else {
				NoEffect();
			}
			break;
			
		case item_type.Rope:
			if(soundThrowCompatible){
				emmit_sound(sound_range);
			}
			else {
				NoEffect();
			}
			break;
			
		case item_type.Well:
			if(soundThrowCompatible){
				emmit_sound(sound_range);
			}
			else {
				NoEffect();
			}
			break;
			
		case item_type.Enemy:
			if(soundThrowCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;
		}
	}

	public void Push(Vector3 player_position, float push_force){
		switch (itemType) {
		case item_type.Crate:	
			if(pushCompatible){
				if(!checkForSnapBack()){
					crate_pushPull(player_position, push_force, current_push_type, false);
				}
			}
			else {
				NoEffect();
			}
			break;
			
		case item_type.Rope:
			if(pushCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;
			
		case item_type.Well:
			if(pushCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;
			
		case item_type.Enemy:
			if(pushCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;
		}
	}

	public void Pull(Vector3 player_position, float push_force){
		switch (itemType) {
		case item_type.Crate:
			if(pullCompatible){
				checkForSnapBack();
				crate_pushPull(player_position, push_force, current_push_type, true);
			}
			else {
				NoEffect();
			}
			break;
			
		case item_type.Rope:
			if(pullCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;
			
		case item_type.Well:
			if(pullCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;
			
		case item_type.Enemy:
			if(pullCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;
		}
	}

	public void Heat(){
		switch (itemType) {
		case item_type.Crate:
			if(heatCompatible){
				heat();
			}
			else {
				NoEffect();
			}
			break;
			
		case item_type.Rope:
			if(heatCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;
			
		case item_type.Well:
			if(heatCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;
			
		case item_type.Enemy:
			if(heatCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;
		}
	}

	public void Cool(){
		switch (itemType) {
		case item_type.Crate:
			if(coldCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;
			
		case item_type.Rope:
			if(coldCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;
			
		case item_type.Well:
			if(coldCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;
			
		case item_type.Enemy:
			if(coldCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;
		}
	}

	public void Blast(){
		switch (itemType) {
		case item_type.Crate:
			if(blastCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;
			
		case item_type.Rope:
			if(blastCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;
			
		case item_type.Well:
			if(blastCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;
			
		case item_type.Enemy:
			if(blastCompatible){
				// --insert behavior here--
			}
			else {
				NoEffect();
			}
			break;
		}
	}

	private void start_stun(){
        if (gameObject.GetComponent<AIMainTrimmed>())
        {
            gameObject.GetComponent<AIMainTrimmed>().Daze();
        }
		stunState = true;
		// Play Animation and Sound for enemy being stunned
		// Laying Enemy sideways for now
		Vector3 tempAngles = transform.eulerAngles;
		tempAngles.x = 90.0f;
		transform.eulerAngles = tempAngles;
	}
	
	private void end_stun(){
		stunState = false;
		Vector3 tempAngles = transform.eulerAngles;
		tempAngles.x = 0.0f;
		transform.eulerAngles = tempAngles;
		StopCoroutine ("_Stun");
	}

	private void heat(){
		StartCoroutine (start_heat(3.0f));
	}

	IEnumerator _Stun(float time){
		start_stun ();
		yield return new WaitForSeconds(time);
		end_stun ();
	}

	IEnumerator start_heat(float time){
		float elapsed = 0.0f;
		Color from = Color.black;
		Color to = Color.red;
		while (elapsed <= time) {
			gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.Lerp(from, to, elapsed / time));
			elapsed += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
	}

	private void crate_pushPull(Vector3 player_pos, float magnitude, push_type type, bool pull){
		Vector3 heading = new Vector3(0.0f, 0.0f, 0.0f);
		float angle = 0.0f;
		float offset = (angle_offset / 180) * Mathf.PI;
		Vector3 pos = Vector3.zero;

		switch (type) {
		case push_type.Free:
			heading = gameObject.transform.position - player_pos;
			break;
		case push_type.TwoAxis:
			heading = gameObject.transform.position - player_pos;
			angle = Vector3.Angle(heading, Vector3.Normalize(Vector3.forward + new Vector3(Mathf.Sin(offset), 0.0f, Mathf.Cos(offset))));
			if(angle > 0.0f && angle <= 90.0f) heading = new Vector3(Mathf.Sin(offset), 0.0f, Mathf.Cos(offset));
			else if(angle > 90.0f && angle <= 180.0f) heading = new Vector3(Mathf.Sin(offset + Mathf.PI), 0.0f, Mathf.Cos(offset + Mathf.PI));
			else heading = Vector3.zero;
			break;
		case push_type.FourAxis:
			heading = gameObject.transform.position - player_pos;
			pos = gameObject.transform.position;
			angle = Vector3.Angle(heading, Vector3.Normalize(Vector3.forward + new Vector3(Mathf.Sin(offset), 0.0f, Mathf.Cos(offset))));
			if(angle > 0.0f && angle <= 45.0f) heading = new Vector3(Mathf.Sin(offset), 0.0f, Mathf.Cos(offset));
			else if(angle > 135.0f && angle <= 180.0f) heading = new Vector3(Mathf.Sin(offset + Mathf.PI), 0.0f, Mathf.Cos(offset + Mathf.PI));
			else if(angle > 45.0f && angle <= 135.0f && pos.x > player_pos.x) heading = new Vector3(Mathf.Sin(offset + Mathf.PI/2), 0.0f, Mathf.Cos(offset + Mathf.PI/2));
			else if(angle > 45.0f && angle <= 135.0f && pos.x < player_pos.x) heading = new Vector3(Mathf.Sin(offset + (3*Mathf.PI/2)), 0.0f, Mathf.Cos(offset + (3*Mathf.PI/2)));
			break;
		case push_type.HeightAxis:
			heading = gameObject.transform.position - player_pos;
			pos = gameObject.transform.position;
			angle = Vector3.Angle(heading, Vector3.Normalize(Vector3.forward + new Vector3(Mathf.Sin(offset), 0.0f, Mathf.Cos(offset))));
			if(angle > 0.0f && angle <= 22.5f) heading = Vector3.forward;
			else if(angle > 22.5f && angle <= 45.0f && pos.x > player_pos.x) heading = new Vector3(Mathf.Sin(Mathf.PI / 4), 0.0f, Mathf.Cos(Mathf.PI / 4));
			else if(angle > 22.5f && angle <= 45.0f && pos.x < player_pos.x) heading = new Vector3(-Mathf.Sin(Mathf.PI / 4), 0.0f, Mathf.Cos(Mathf.PI / 4));
			else if(angle > 45.0f && angle <= 135.0f && pos.x > player_pos.x) heading = Vector3.right;
			else if(angle > 45.0f && angle <= 135.0f && pos.x < player_pos.x) heading = Vector3.left;
			else if(angle > 135.0f && angle <= 157.5f && pos.x > player_pos.x) heading = new Vector3(Mathf.Sin(Mathf.PI / 4), 0.0f, -Mathf.Cos(Mathf.PI / 4));
			else if(angle > 135.0f && angle <= 157.5f && pos.x < player_pos.x) heading = new Vector3(-Mathf.Sin(Mathf.PI / 4), 0.0f, -Mathf.Cos(Mathf.PI / 4));
			else if(angle > 135.0f && angle <= 180.0f) heading = Vector3.back;
			else heading = Vector3.zero;
			break;
		case push_type.Anim:
            if (gameObject.transform.parent.gameObject.GetComponent<ItemAnimator>())
            {
                gameObject.transform.parent.gameObject.GetComponent<ItemAnimator>().set_in_motion();
            }
            return;
		}

		heading.y = 0.0f;
		float distance = heading.magnitude;
		Vector3 direction = heading / distance;

		if (pull) direction *= -1;

		Rigidbody rb = gameObject.GetComponent<Rigidbody> ();
		rb.AddForce(direction * (magnitude * 100));
	}

	private void drawPushPullDirections(push_type type){
		Vector3 origin = gameObject.transform.position;
		float heading = (angle_offset / 180) * Mathf.PI;
		float line_length = 5.0f;
		Gizmos.color = Color.red;
		switch (type) {
		case push_type.Free:
			Vector3 end = new Vector3(0.0f, line_length, 0.0f);
			Gizmos.DrawLine(origin, origin + end);
			break;
		case push_type.TwoAxis:
			for(int i = 0; i < 2; i++){
				Gizmos.DrawLine(origin, origin + new Vector3(Mathf.Sin(heading + (Mathf.PI) * i)* line_length, 0.0f, Mathf.Cos(heading + (Mathf.PI) * i) * line_length));
			}
			break;
		case push_type.FourAxis:
			for(int i = 0; i < 4; i++){
				Gizmos.DrawLine(origin, origin + new Vector3(Mathf.Sin(heading + (Mathf.PI/2) * i)* line_length, 0.0f, Mathf.Cos(heading + (Mathf.PI/2) * i) * line_length));
			}
			break;
		case push_type.HeightAxis:;
			for(int i = 0; i < 8; i++){
				Gizmos.DrawLine(origin, origin + new Vector3(Mathf.Sin(heading + (Mathf.PI/4) * i)* line_length, 0.0f, Mathf.Cos(heading + (Mathf.PI/4) * i) * line_length));
			}
			break;
		}
	}

	private bool checkForSnapBack(){
		Vector3 currentPosition = gameObject.transform.position;
		if (Vector3.Distance (currentPosition, startPosition) >= pushPullRadius) {
			Rigidbody rb = gameObject.GetComponent<Rigidbody>();
			rb.velocity = Vector3.zero;
			gameObject.transform.position = startPosition;
			return true;
		}
		return false;
	}

	public void toggle_targeted(){
		is_targeted = !is_targeted;
	}

	private void display_effects(){
		Camera main_camera = Camera.current;
		if (main_camera && quincAffected) {
			Vector3 position = main_camera.WorldToScreenPoint (gameObject.transform.position);
			float start_y = 0.0f;
			float margin = 5.0f;
			float message_height = 20.0f;

			if(pushCompatible){
				GUI.Label (new Rect (position.x, position.y + start_y + margin, 100, 20), "Can be pushed.");
				start_y += message_height;
			}

			if(pullCompatible){
				GUI.Label (new Rect (position.x, position.y + start_y + margin, 100, 20), "Can be pulled.");
				start_y += message_height;
			}

			if(cutCompatible){
				GUI.Label (new Rect (position.x, position.y + start_y + margin, 100, 20), "Can be cut.");
				start_y += message_height;
			}

			if(heatCompatible){
				GUI.Label (new Rect (position.x, position.y + start_y + margin, 100, 20), "Can be heat-up.");
				start_y += message_height;
			}

			if(coldCompatible){
				GUI.Label (new Rect (position.x, position.y + start_y + margin, 100, 20), "Can be frozen.");
				start_y += message_height;
			}

			if(stunCompatible){
				GUI.Label (new Rect (position.x, position.y + start_y + margin, 100, 20), "Can be stunned.");
				start_y += message_height;
			}

			if(soundThrowCompatible){
				GUI.Label (new Rect (position.x, position.y + start_y + margin, 100, 20), "Can be sound thrown.");
				start_y += message_height;
			}
		}
	}

	private void emmit_sound(float radius){
        if (EffectPrefab != null)
        {
            Instantiate(EffectPrefab, gameObject.transform.position, Quaternion.identity);
        }
        Vector3 center = gameObject.transform.position;
		Collider[] colliders = Physics.OverlapSphere(center, radius);
		foreach (Collider c in colliders){
			AIMainTrimmed ai = c.gameObject.GetComponent<AIMainTrimmed>();
			if(ai != null){
				ai.NoiseHeard(gameObject);
			}
		}
	}

	private void NoEffect(){
		print("I'm afraid I can't do that Dave.");
	}
}




