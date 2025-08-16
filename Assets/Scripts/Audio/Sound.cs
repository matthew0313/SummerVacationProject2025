using UnityEngine;
using UnityEngine.SceneManagement;
using MEC;
using System.Collections.Generic;



#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class Sound
{
    public AudioClip clip;
    public float volume = 1.0f;
    public float pitch = 1.0f;
    public float startTime = 0.0f;

    [HideInInspector] public bool foldout = false;
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(Sound))]
public class Sound_Drawer : PropertyDrawer
{
    AudioSource testSource;
    const float rightBoxWidth = 50.0f;
    const float foldoutHeight = 20.0f;
    const float contentHeight = 80.0f;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty foldout = property.FindPropertyRelative("foldout");
        foldout.boolValue = EditorGUI.Foldout(new Rect(position) { height = foldoutHeight }, foldout.boolValue, label);
        if (foldout.boolValue)
        {
            position.y += foldoutHeight;
            SerializedProperty clipProperty = property.FindPropertyRelative("clip");
            SerializedProperty volumeProperty = property.FindPropertyRelative("volume");
            SerializedProperty pitchProperty = property.FindPropertyRelative("pitch");
            SerializedProperty startTimeProperty = property.FindPropertyRelative("startTime");

            Rect leftBox = position, rightBox = position;
            leftBox.width = position.width - rightBoxWidth;
            rightBox.x = position.x + position.width - rightBoxWidth;
            rightBox.width = rightBoxWidth;

            EditorGUI.PropertyField(new Rect(leftBox) { height = 20 }, clipProperty);
            volumeProperty.floatValue = EditorGUI.Slider(new Rect(leftBox) { y = leftBox.y + 20, height = 20 }, new GUIContent() { text = "Volume" }, volumeProperty.floatValue, 0, 1);
            pitchProperty.floatValue = EditorGUI.Slider(new Rect(leftBox) { y = leftBox.y + 40, height = 20 }, new GUIContent() { text = "Pitch" }, pitchProperty.floatValue, 0, 2);
            if(clipProperty.objectReferenceValue is AudioClip clip)
            {
                startTimeProperty.floatValue = EditorGUI.Slider(new Rect(leftBox) { y = leftBox.y + 60, height = 20 }, new GUIContent() { text = "Start Time" }, startTimeProperty.floatValue, 0, clip.length);
            }

            if (GUI.Button(new Rect(rightBox) { height = 40 }, "Play"))
            {
                if (testSource == null)
                {
                    testSource = new GameObject() { hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild }.AddComponent<AudioSource>();
                    testSource.playOnAwake = false;
                }
                testSource.clip = clipProperty.objectReferenceValue as AudioClip;
                testSource.volume = volumeProperty.floatValue;
                testSource.pitch = pitchProperty.floatValue;
                testSource.Play();
                testSource.time = startTimeProperty.floatValue;
            }
            if (GUI.Button(new Rect(rightBox) { y = rightBox.y + 40, height = 40 }, "Stop"))
            {
                if(testSource != null) testSource.Pause();
            }
        }
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = foldoutHeight;
        SerializedProperty foldout = property.FindPropertyRelative("foldout");
        if (foldout.boolValue)
        {
            height += contentHeight;
        }
        return height;
    }
}
#endif