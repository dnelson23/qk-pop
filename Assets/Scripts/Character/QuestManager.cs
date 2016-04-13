using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Debug = FFP.Debug;

[EventVisibleAttribute]
public class QuestManager : MonoBehaviour {

	public static QuestManager instance;

	public List <Quest> currentQuests;
	public List <Quest> failedQuests;
	public List <Quest> completedQuests;
	public GameObject compassTargetPrefab;
	
	Quest _quest;
	QuestSaveManager _questSaveManager;
	public int questCount;
	GameObject questManagerUI;
	QuestManagerUIController qmUI;
	GameObject compassTarget;
	
    void Awake()
    {
        gameObject.AddComponent<DebugOnScreen>();
		currentQuests = new List<Quest> ();
		failedQuests = new List<Quest> ();
		completedQuests = new List<Quest> ();
		instance = this;
		compassTarget = Instantiate (compassTargetPrefab);
    }

    void Start() {
		questManagerUI = GameObject.Find("QuestManagerUI");
		if (questManagerUI) {
			qmUI = questManagerUI.GetComponent<QuestManagerUIController>();
			questManagerUI.SetActive(false);
		}
		else {
			Debug.Error("ui","QuestManager script attached to the player could not find the 'QuestManagerUI' UI GameObject in the scene: " + Application.loadedLevelName);
		}
		_quest = new Quest (null, null, null, -1, null);
		//_questSaveManager = Object.FindObjectOfType<QuestSaveManager> ();
		_questSaveManager = QuestSaveManager.S;
		if (!_questSaveManager) {
			Debug.Error("ui","Could not find the 'QuestSaveManager' singleton in the scene: " + Application.loadedLevelName);
		}
	}

	[EventVisibleAttribute]
	public void LoadQuests() {
		List<Quest> newQuestList = _questSaveManager.LoadQuests();
		if (newQuestList != null) {
			currentQuests = newQuestList;
		}
		return;
	}

	[EventVisibleAttribute]
	public void SaveQuests() {
		_questSaveManager.SaveQuests (currentQuests);
		return;
	}
	
	public List<Quest> CurrentQuests() {
		return currentQuests;
	}

	[EventVisibleAttribute]
	public void UpdateQuests() {

		if (currentQuests.Count == 0) {
			return;
		}

		for (int count = currentQuests.Count - 1; count > -1; count--) {

			if(currentQuests[count].IsFailed() == true) {
				failedQuests.Add(currentQuests[count]);
				currentQuests.RemoveAt(count);
				continue;
			}

			if(currentQuests[count].IsCompleted() == true) {
				_questSaveManager.SaveCompletedQuest(currentQuests[count]);
				completedQuests.Add(currentQuests[count]);
				currentQuests.RemoveAt(count);
			}
		}

		return;
	}

	[EventVisibleAttribute]
	public void CompleteGoalInQuest(int questID, int goalIndex) {
		if (currentQuests.Count < 1) {
			return;
		}

		foreach (Quest q in currentQuests) {
			if(q.GetID() == questID) {
				q.CompleteGoalInQuest(goalIndex);
			}
		}
	}

	[EventVisibleAttribute]
	public void ProgressGoalInQuest(int questID, int goalIndex) {
		if (currentQuests.Count < 1) {
			return;
		}

		foreach (Quest q in currentQuests) {
			if(q.GetID() == questID) {
				q.ProgressGoalInQuest(goalIndex);
			}
		}
	}

	void Update() {
		questCount = currentQuests.Count;
	}
	
	/*void MoveCompassTargetPoint(GameObject NextQuestLocation){
		compassTarget.transform.position = NextQuestLocation.transform.position;
		GameHUD.Instance.calcCompass = true;
		return;
	}*/

	[EventVisibleAttribute]
	public void AddQuest(int questID)
    {
		if (_questSaveManager.CompletedQuest (questID) == true) {
			Debug.Log("ui", "Quest has already been completed. Delete in PlayerPrefs probably");
			return;
		}

		Quest newQuest = _quest.AddQuest (questID);

		if (newQuest == null) {
			Debug.Log("ui", "New Quest is null. Not adding to List!");
			return;
		}

		currentQuests.Add (newQuest);

		if (newQuest.HasTimer () == true) {

			StartCoroutine("StartTimer", newQuest);
		}

		return;
	}

	

	IEnumerator StartTimer(Quest q) {

		yield return new WaitForSeconds ((float)q.GetTimerLength());

		if (q.IsCompleted () == false) {
			q.Fail();
			UpdateQuests();
		} else {
			UpdateQuests();
		}
	}
}
