using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RPGTalkCinematicBehaviour))]
public class RPGTalkCinematicDrawer : PropertyDrawer
{
    public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
    {
        int fieldCount = 0;
        return fieldCount * EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
    {

		SerializedProperty txtToParseProp= property.FindPropertyRelative("txtToParse");
		SerializedProperty lineToStartProp = property.FindPropertyRelative("lineToStart");
		SerializedProperty lineToBreakProp = property.FindPropertyRelative("lineToBreak");
		SerializedProperty textSpeedProp = property.FindPropertyRelative("textSpeed");
		SerializedProperty pauseUntilTalkEndProp = property.FindPropertyRelative("pauseUntilTalkEnd");

		EditorGUILayout.LabelField("Put below the Text file to be parsed and become the talks!");
		EditorGUILayout.PropertyField (txtToParseProp,GUIContent.none);
		if (txtToParseProp.objectReferenceValue == null) {
			EditorGUILayout.HelpBox("If no text is setted, it will be used the same that already is in the RPGTalk reference", MessageType.Info, true);
		}

		pauseUntilTalkEndProp.boolValue = GUILayout.Toggle(pauseUntilTalkEndProp.boolValue, "Pause Timeline while waiting for player's action?");

		EditorGUILayout.LabelField("What line should the talk start? And in what should it end?");
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.PropertyField (lineToStartProp,GUIContent.none);
		if (pauseUntilTalkEndProp.boolValue) {
			EditorGUILayout.PropertyField (lineToBreakProp, GUIContent.none);
		} else {
			EditorGUILayout.HelpBox("If you not wait for player input, each RPGTalk Cinematic Clip can only contain one line", MessageType.Info, true);
		}
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.PropertyField (textSpeedProp);
    }
}
