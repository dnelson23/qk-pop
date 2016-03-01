using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[EventVisibleAttribute]
public class QuestManager : MonoBehaviour {

	public List <Quest> currentQuests;
	public List <Quest> failedQuests;
	public List <Quest> completedQuests;
	Quest _quest;
	QuestSaveManager _questSaveManager;
	public int questCount;
	GameObject questManagerUI;
	QuestManagerUIController qmUI;

    void Awake()
    {
        gameObject.AddComponent<DebugOnScreen>();
		currentQuests = new List<Quest> ();
		failedQuests = new List<Quest> ();
		completedQuests = new List<Quest> ();
    }

    void Start() {
		questManagerUI = GameObject.Find("QuestManagerUI");
		if (questManagerUI) {
			qmUI = questManagerUI.GetComponent<QuestManagerUIController>();
			questManagerUI.SetActive(false);
		}
		else {
			Debug.LogError("QuestManager script attached to the player could not find the 'QuestManagerUI' UI GameObject in the scene: " + Application.loadedLevelName);
		}
		_quest = new Quest (null, null, null, -1, null);
		/*currentQuests = new List<Quest> ();
		failedQuests = new List<Quest> ();
		completedQuests = new List<Quest> ();
		*/
		//_questSaveManager = (QuestSaveManager)FindObjectOfType (typeof(QuestSaveManager));
		_questSaveManager = GameObject.Find ("qmSaveManager").GetComponent<QuestSaveManager> ();
		if (!_questSaveManager) {
			Debug.LogError("Could not find the '_questSaveManagerComponent' in the scene: " + Application.loadedLevelName);
		}
	}

	[EventVisibleAttribute]
	public void LoadQuests() {
		int x = _questSaveManager.DebugFunctionDeleteLater ();
		print (x);

		List<Quest> newQuestList = _questSaveManager.LoadQuests();
		if (newQuestList != null) {
			currentQuests = newQuestList;
		}
		DebugOnScreen.Log (currentQuests.Count + " quests loaded!");
		return;
	}

	[EventVisibleAttribute]
	public void SaveQuests() {
		_questSaveManager.SaveQuests (currentQuests);
		DebugOnScreen.Log ("Quests saved!");
		return;
	}
	
	public List<Quest> CurrentQuests() {
		return currentQuests;
	}

	[EventVisibleAttribute]
	public void UpdateQuests() {

		DebugOnScreen.Log ("Checking Quests for Completion!");
		if (currentQuests.Count == 0) {
			DebugOnScreen.Log("No quests in List");
			return;
		}

		for (int count = currentQuests.Count - 1; count > -1; count--) {

			if(currentQuests[count].IsFailed() == true) {
				DebugOnScreen.Log(currentQuests[count].GetName() + " quest has failed and removed fom Current Quests List and added to Failed Quests List!");
				failedQuests.Add(currentQuests[count]);
				currentQuests.RemoveAt(count);
				qmUI.showQuests();
				continue;
			}

			if(currentQuests[count].IsCompleted() == true) {
				DebugOnScreen.Log (currentQuests[count].GetName() + " quest is completed, removed from Current Quests List and added to Completed Quests List!");
				_questSaveManager.SaveCompletedQuest(currentQuests[count]);
				completedQuests.Add(currentQuests[count]);
				currentQuests.RemoveAt(count);
				qmUI.showQuests();
			}
		}

		return;
	}

	[EventVisibleAttribute]
	public void CompleteGoalInQuest(int questID, int goalIndex) {
		DebugOnScreen.Log("IN COMPLETE GOAL IN QUEST!");
		if (currentQuests.Count < 1) {
			DebugOnScreen.Log("No quests in List!");
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
			DebugOnScreen.Log("No quests in List!");
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

	[EventVisibleAttribute]
	public void AddQuest(int questID) {

		if (_questSaveManager.CompletedQuest (questID) == true) {
			DebugOnScreen.Log("Quest has already been completed. Delete in PlayerPrefs probably");
			return;
		}

		Quest newQuest = _quest.AddQuest (questID);

		if (newQuest == null) {
			DebugOnScreen.Log("New Quest is null. Not adding to List!");
			return;
		}

		currentQuests.Add (newQuest);
		DebugOnScreen.Log ("Added quest!");

		if (newQuest.HasTimer () == true) {

			StartCoroutine("StartTimer", newQuest);
		}

		return;
	}

	IEnumerator StartTimer(Quest q) {
		DebugOnScreen.Log ("Starting timer for " + q.GetTimerLength() + " seconds.");

		yield return new WaitForSeconds ((float)q.GetTimerLength());

		if (q.IsCompleted () == false) {
			q.Fail();
			UpdateQuests();
		} else {
			UpdateQuests();
		}
	}
}
