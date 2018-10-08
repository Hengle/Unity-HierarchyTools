using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class HierarchyTools : EditorWindow
{
	/* Fields: Private */
	private Vector2 mainScrollPosition;
	private string groupName = "Group";

	/* Methods: Public (Static) */
	[MenuItem("Window/Tools/Hierarchy")]
	public static void ShowWindow()
	{
		EditorWindow window = GetWindow<HierarchyTools>("Hierarchy");
	}

	/* Methods: Private */
	private void OnGUI()
	{
		// Main scrollbar.
		mainScrollPosition = EditorGUILayout.BeginScrollView(mainScrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar);

		// Prefab: options.
		GUILayout.Label("Prefab", EditorStyles.boldLabel);

		bool revertPrefab = GUILayout.Button("Revert");

		// Prefab.
		if (revertPrefab)
		{
			GameObject[] selectionObjects = Array.ConvertAll(Selection.GetTransforms(SelectionMode.Editable), item => item.gameObject);

			foreach (GameObject selectionObject in selectionObjects)
			{
				Undo.RecordObject(selectionObject, "Hierarchy Tools - Revert");

				PrefabUtility.RevertPrefabInstance(selectionObject);
			}
		}

		// Selection: options.
		GUILayout.Label("Select", EditorStyles.boldLabel);

		bool selectNone = GUILayout.Button("None");
		bool selectParent = GUILayout.Button("Parent");
		bool selectChildrenAll = GUILayout.Button("Children (All)");
		bool selectChildrenNext = GUILayout.Button("Children (Next)");
		
		// Selection.
		if (selectNone)
		{
			Selection.activeTransform = null;
		}

		if (selectChildrenAll)
		{
			Transform[] selectionTransforms = Selection.GetTransforms(SelectionMode.Editable);
			List<Transform> selectionChildren = new List<Transform>();

			foreach (Transform selectionTransform in selectionTransforms)
			{
				selectionChildren.AddRange(selectionTransform.GetComponentsInChildren<Transform>());
			}

			Selection.objects = selectionChildren.ConvertAll((Transform transform) => transform.gameObject).ToArray();
		}

		if (selectChildrenNext)
		{
			Transform[] selectionTransforms = Selection.GetTransforms(SelectionMode.Editable);
			List<Transform> selectionChildren = new List<Transform>();

			foreach (Transform selectionTransform in selectionTransforms)
			{
				foreach (Transform child in selectionTransform)
				{
					selectionChildren.Add(child);
				}
			}

			Selection.objects = selectionChildren.ConvertAll((Transform transform) => transform.gameObject).ToArray();
		}
		
		if (selectParent)
		{
			Selection.activeTransform = Selection.activeTransform == null ? null : Selection.activeTransform.parent;
		}

		// Sort: options.
		GUILayout.Label("Sort", EditorStyles.boldLabel);
		
		bool sortAlphabetically = GUILayout.Button("Alphabetically");
		bool sortInvert = GUILayout.Button("Invert");
		
		// Sort.
		if (sortAlphabetically || sortInvert)
		{
			List<Transform> selectionTransforms = new List<Transform>(Selection.GetTransforms(SelectionMode.Editable));
			int minSiblingIndex = selectionTransforms.Min(transform => transform.GetSiblingIndex());

			if (sortAlphabetically)
			{
				selectionTransforms.Sort((Transform x, Transform y) => x.name.CompareTo(y.name));
			}
			else if (sortInvert)
			{
				selectionTransforms.Sort((Transform x, Transform y) => y.GetSiblingIndex().CompareTo(x.GetSiblingIndex()));
			}
			
			for (int selectionIndex = 0; selectionIndex < selectionTransforms.Count; selectionIndex++)
			{
				Undo.RecordObject(selectionTransforms[selectionIndex], "Hierarchy Tools - Sort");
				
				selectionTransforms[selectionIndex].SetSiblingIndex(minSiblingIndex + selectionIndex);
			}
		}

		// Other: options.
		GUILayout.Label("Other", EditorStyles.boldLabel);
		
		groupName = GUILayout.TextField(groupName);
		bool shouldGroup = GUILayout.Button("Group");

		if (shouldGroup && Selection.activeTransform != null)
		{
			Transform[] selectionTransforms = Selection.GetTransforms(SelectionMode.Editable);
			Vector3 selectionMin = Selection.activeTransform.position;
			Vector3 selectionMax = selectionMin;

			foreach (Transform selectionTransform in selectionTransforms)
			{
				selectionMin = Vector3.Min(selectionMin, selectionTransform.position);
				selectionMax = Vector3.Max(selectionMax, selectionTransform.position);
			}

			Transform groupRoot = new GameObject(groupName).transform;
			groupRoot.SetParent(Selection.activeTransform.parent);
			groupRoot.localPosition = (selectionMax - selectionMin) / 2f + selectionMin;

			Undo.RegisterCreatedObjectUndo(groupRoot.gameObject, "Hiearchy Tools - Group");

			foreach (Transform selectionTransform in selectionTransforms)
			{
				Undo.SetTransformParent(selectionTransform, groupRoot, "Hiearchy Tools - Group");
			}

			Selection.activeTransform = groupRoot;
		}
		
		// Other.

		// Main scrollbar.
		EditorGUILayout.EndScrollView();
	} 
}
