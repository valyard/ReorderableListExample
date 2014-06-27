using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor {
	private ReorderableList list;
	
	private void OnEnable() {
		list = new ReorderableList(serializedObject, 
		                           serializedObject.FindProperty("Waves"), 
		                           true, true, true, true);
		list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
			var element = list.serializedProperty.GetArrayElementAtIndex(index);
			rect.y += 2;
			EditorGUI.PropertyField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("Type"), GUIContent.none);
			EditorGUI.PropertyField(new Rect(rect.x + 60, rect.y, rect.width - 60 - 30, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("Prefab"), GUIContent.none);
			EditorGUI.PropertyField(new Rect(rect.x + rect.width - 30, rect.y, 30, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("Count"), GUIContent.none);
		};
		list.drawHeaderCallback = (Rect rect) => {
			EditorGUI.LabelField(rect, "Monster Waves");
		};
		list.onSelectCallback = (ReorderableList l) => {
			var prefab = l.serializedProperty.GetArrayElementAtIndex(l.index).FindPropertyRelative("Prefab").objectReferenceValue as GameObject;
			if (prefab) EditorGUIUtility.PingObject(prefab.gameObject);
		};
		list.onCanRemoveCallback = (ReorderableList l) => {
			return l.count > 1;
		};
		list.onRemoveCallback = (ReorderableList l) => {
			if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete the wave?", "Yes", "No"))
			{
				ReorderableList.defaultBehaviours.DoRemoveButton(l);
			}
		};
		list.onAddCallback = (ReorderableList l) => {
			var index = l.serializedProperty.arraySize;
			l.serializedProperty.arraySize++;
			l.index = index;
			var element = l.serializedProperty.GetArrayElementAtIndex(index);
			element.FindPropertyRelative("Type").enumValueIndex = 0;
			element.FindPropertyRelative("Count").intValue = 20;
			element.FindPropertyRelative("Prefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Mobs/Cube.prefab", typeof(GameObject)) as GameObject;
		};
		list.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) => {
			var menu = new GenericMenu();
			var guids = AssetDatabase.FindAssets("", new[]{"Assets/Prefabs/Mobs"});
			foreach (var guid in guids) {
				var path = AssetDatabase.GUIDToAssetPath(guid);
				menu.AddItem(new GUIContent("Mobs/" + Path.GetFileNameWithoutExtension(path)), false, clickHandler, new WaveCreationParams() {Type = MobWave.WaveType.Mobs, Path = path});
			}
			guids = AssetDatabase.FindAssets("", new[]{"Assets/Prefabs/Bosses"});
			foreach (var guid in guids) {
				var path = AssetDatabase.GUIDToAssetPath(guid);
				menu.AddItem(new GUIContent("Bosses/" + Path.GetFileNameWithoutExtension(path)), false, clickHandler, new WaveCreationParams() {Type = MobWave.WaveType.Boss, Path = path});
			}
			menu.ShowAsContext();
		};
	}
	
	public override void OnInspectorGUI() {
		serializedObject.Update();
		list.DoLayoutList();
		serializedObject.ApplyModifiedProperties();
	}

	private void clickHandler(object target) {
		var data = (WaveCreationParams)target;
		var index = list.serializedProperty.arraySize;
		list.serializedProperty.arraySize++;
		list.index = index;
		var element = list.serializedProperty.GetArrayElementAtIndex(index);
		element.FindPropertyRelative("Type").enumValueIndex = (int)data.Type;
		element.FindPropertyRelative("Count").intValue = data.Type == MobWave.WaveType.Boss ? 1 : 20;
		element.FindPropertyRelative("Prefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath(data.Path, typeof(GameObject)) as GameObject;
		serializedObject.ApplyModifiedProperties();
	}
	
	private struct WaveCreationParams {
		public MobWave.WaveType Type;
		public string Path;
	}
}