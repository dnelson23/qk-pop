using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PopEvent : MonoBehaviour {

    public List<EventHalf> conditions;
    public List<EventHalf> actions;

    public string[] andOrCompare = new string[] { "Every Condition", "One or More", "Exactly One" };
    public string andOrCompareString;
    public int andOrCompareIndex;

    public string uniqueId; //!<    Used to identify an event from anywhere in the scene

    public bool isActive = true;
    public bool isRegional = true;
    public bool executeOnce = true;
    public bool hasExecuted = false;

    private PopEvent nextEvent;

    public float totalTimeActive = 0;
    private float timer = 0;
    public float delay = 1;

    public float regionRadius = 1;
    public bool drawRegionTwo = false;
    public Vector3 conditionRegionCenter = new Vector3(0, 0, 0);
    public float conditionRegionRadius = 1;

    public PopEvent() {
        conditions = new List<EventHalf>();
        conditions.Add(new EventHalf());
        actions = new List<EventHalf>();
        actions.Add(new EventHalf());
    }

    void Awake() {
        EventListener.AddPopEvent(this);
        PopEvent[] popEvents = gameObject.GetComponents<PopEvent>();
        for (int i = 0; i < popEvents.Length - 1; i++) { //  Don't check the last element
            if (this.Equals(popEvents[i])) {
                nextEvent = popEvents[i + 1];
                break;
            }
        }

        foreach (EventHalf condition in conditions){
            if (condition.e_classString == "Player Enters Area" || condition.e_classString == "Player Leaves Area") {
                SphereCollider newCollider = gameObject.AddComponent<SphereCollider>();
                newCollider.radius = conditionRegionRadius;
                newCollider.isTrigger = true;
                gameObject.layer = 2;
                break;
            }
        }
    }

    void Update() {
        if (isActive == false) { return; }
        if (executeOnce == true && hasExecuted == true) { return; }

        totalTimeActive += Time.deltaTime;

        if (timer < delay) {
            timer += Time.deltaTime;
        }
        if (timer >= delay) {
            timer = 0;
            EventListener.SlowUpdate(this);
        }
    }

    void OnTriggerEnter(Collider other) {
        if (isActive == false) { return; }
        if (other.gameObject.GetComponent<QK_Character_Movement>())
        {
            EventListener.SlowUpdate(this);
        }
    }
    void OnTriggerExit(Collider other) {
        if (isActive == false) { return; }
		if (other.gameObject.GetComponent<QK_Character_Movement>())
        {
            EventListener.SlowUpdate(this);
        }
    }

    void OnDrawGizmosSelected() {
        // The other portion of the custom Editor graphics are found in PopEventEditor.cs
        if (isRegional == true) {
        }

        if (drawRegionTwo) {
            Gizmos.color = new Color(0, 1, 0, 0.3F);
            Gizmos.DrawSphere(transform.position, conditionRegionRadius);
        }
    }

    public void ActivateNextEvent() {
        if (nextEvent != null) {
            nextEvent.MakeActive(true);
        }
    }

    public void MakeActive(bool active, bool force = false) {
        if (active == true && executeOnce == true && hasExecuted == true && force == false) { return; }
        isActive = active;
    }
}