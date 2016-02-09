using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SECTR_StreamWindow : SECTR_Window
{
	private Vector2 scrollPosition;
	private string sectorSearch = "";
	private SECTR_Sector selectedSector = null;

	#region Unity Interface
	protected override void OnGUI()
	{
		base.OnGUI();

		List<SECTR_Sector> sortedSectors = new List<SECTR_Sector>(SECTR_Sector.All);
		sortedSectors.Sort(delegate(SECTR_Sector a, SECTR_Sector b) { return a.name.CompareTo(b.name); });
		int numSectors = sortedSectors.Count;
		bool sceneHasSectors = numSectors > 0;

		EditorGUILayout.BeginVertical();
		DrawHeader("SECTORS", ref sectorSearch, 100, true);
		Rect r = EditorGUILayout.BeginVertical();
		r.y -= lineHeight;
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
		bool wasEnabled = GUI.enabled;
		GUI.enabled = false;
		GUI.Button(r, sceneHasSectors ? "" : "Current Scene Has No Sectors");
		GUI.enabled = wasEnabled;
		bool allExported = true;
		bool allImported = true;
		SECTR_Sector newSelectedSector =  Selection.activeGameObject ?  Selection.activeGameObject.GetComponent<SECTR_Sector>() : null;;
		bool mouseDown = Event.current.type == EventType.MouseDown && Event.current.button == 0;
		if(mouseDown)
		{
			newSelectedSector = null;
		}

		for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
		{
			SECTR_Sector sector = sortedSectors[sectorIndex];
			if(sector.name.ToLower().Contains(sectorSearch.ToLower()))
			{
				bool selected = sector == selectedSector;
				Rect clipRect = EditorGUILayout.BeginHorizontal();
				if(selected)
				{
					Rect selectionRect = clipRect;
					selectionRect.y += 1;
					selectionRect.height -= 1;
					GUI.Box(selectionRect, "", selectionBoxStyle);
				}
				if(sector.Frozen)
				{
					allImported = false;
				}
				else
				{
					allExported = false;
				}

				elementStyle.normal.textColor = selected ? Color.white : UnselectedItemColor;
				elementStyle.alignment = TextAnchor.MiddleCenter;
				EditorGUILayout.LabelField(sector.name, elementStyle);

				EditorGUILayout.EndHorizontal();

				if(sector.gameObject.isStatic)
				{
					float buttonWidth = 50; 
					SECTR_Chunk chunk = sector.GetComponent<SECTR_Chunk>();
					bool alreadyExported = chunk && System.IO.File.Exists(SECTR_Asset.UnityToOSPath(chunk.NodeName));
					if(sector.Frozen)
					{
						// Import
						if(alreadyExported && 
						   GUI.Button(new Rect(0, clipRect.yMin, buttonWidth, clipRect.height), new GUIContent("Import", "Imports this Sector into the scene.")))
						{
							SECTR_StreamExport.ImportFromChunk(sector);
							break;
						}
					}
					else
					{
						// Revert
						if(alreadyExported && 
						   GUI.Button(new Rect(0, clipRect.yMin, buttonWidth, clipRect.height), new GUIContent("Revert", "Discards changes to this Sector.")))
						{
							SECTR_StreamExport.DeleteExportedSector(sector);
							chunk.enabled = true;
							break;
						}
						// Export
						if(GUI.Button(new Rect(clipRect.xMax - buttonWidth, clipRect.yMin, buttonWidth, clipRect.height), new GUIContent("Export", "Exports this Sector into a Chunk scene.")))
						{
							SECTR_StreamExport.ExportToChunk(sector);
							break;
						}
					}
				}

				if(mouseDown && clipRect.Contains(Event.current.mousePosition) )
				{
					newSelectedSector = sector;
				}
			}
		}
		if(newSelectedSector != selectedSector)
		{
			selectedSector = newSelectedSector;
			Selection.activeGameObject = selectedSector ? selectedSector.gameObject : null;
			if(SceneView.lastActiveSceneView)
			{
				SceneView.lastActiveSceneView.FrameSelected();
			}
			Repaint();
		}
		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndVertical();

		string nullSearch = null;
		DrawHeader("EXPORT", ref nullSearch, 0, true);
		wasEnabled = GUI.enabled;
		bool editMode = !EditorApplication.isPlaying && !EditorApplication.isPaused;
		GUI.enabled = sceneHasSectors && !allExported && wasEnabled && editMode;
		if(GUILayout.Button(new GUIContent("Export All Sectors", "Exports all static Sectors into Chunk scenes and prepares them for streaming.")))
		{
			SECTR_StreamExport.ExportSceneChunksUI();
			Repaint();
		}
		GUI.enabled = sceneHasSectors && !allImported && wasEnabled && editMode;
		if(GUILayout.Button(new GUIContent("Import All Sectors", "Imports all exported Chunks back into the scene.")))
		{
			SECTR_StreamExport.ImportSceneChunksUI();
			Repaint();
		}
		GUI.enabled = sceneHasSectors && !allExported && wasEnabled && editMode;
		if(GUILayout.Button(new GUIContent("Revert All Sectors", "Reverts all exported Chunks to their exported state.")))
		{
			SECTR_StreamExport.RevertSceneChunksUI();
			Repaint();
		}
		GUI.enabled = sceneHasSectors;
		if(GUILayout.Button(new GUIContent("Export Sector Graph Visualization", "Writes out a .dot file of the Sector/Portal graph, which can be visualized in GraphViz.")))
		{
			SECTR_StreamExport.WriteGraphDot();
		}
		GUI.enabled = wasEnabled;
		EditorGUILayout.EndVertical();
	}
	#endregion
}
