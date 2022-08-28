using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;
using System.Reflection;
using System.Linq;
using System;

[CustomEditor(typeof(InputRebindButton))]
public class InputRebindButtonEditor : ButtonEditor {


    protected override void OnEnable()
    {
        base.OnEnable();




    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUIContent content = new GUIContent();
        content.text = "Input Rebind Settings";

        GUIStyle style = EditorStyles.boldLabel;

        GUILayout.Label(content, style);

        var it = serializedObject.GetIterator();

        it.Next(true);

        while (it.NextVisible(false))
        {
            if (it.name.Contains("inputToRebind") ||
                it.name.Contains("rebindMouse") ||
                it.name.Contains("rebindComposite") ||
                it.name.Contains("componsiteDirection"))
                EditorGUILayout.PropertyField(it);


        }

        serializedObject.ApplyModifiedProperties();

    }
}
