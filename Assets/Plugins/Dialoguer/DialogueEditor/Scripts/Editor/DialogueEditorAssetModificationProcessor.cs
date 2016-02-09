using UnityEngine;
using UnityEditor;
using System.Collections;

using DialoguerEditor;

public class DialogueEditorAssetModificationProcessor : UnityEditor.AssetModificationProcessor {
	public static string[] OnWillSaveAssets(string[] paths){
		//DialogueEditorWindow window = (DialogueEditorWindow)EditorWindow.GetWindow(typeof(DialogueEditorWindow));
		if(EditorWindow.focusedWindow != null){
			if(EditorWindow.focusedWindow.titleContent == new GUIContent("Dialogue Editor") || EditorWindow.focusedWindow.titleContent == new GUIContent("Variable Editor") || EditorWindow.focusedWindow.titleContent == new GUIContent("Theme Editor")){
				DialogueEditorDataManager.save();
				DialoguerEnumGenerator.GenerateDialoguesEnum();
			}
		}
		return paths;
	}
}
