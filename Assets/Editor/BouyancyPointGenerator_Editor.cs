using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BouyancyPointGenerator))]
public class BouyancyPointGenerator_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BouyancyPointGenerator bouyancyPointGenerator = (BouyancyPointGenerator)target;

        if (GUILayout.Button("Generate"))
        {
            if(!EditorApplication.isPlaying)
            {
                bouyancyPointGenerator.Generate();
            }

            else
            {
                Debug.Log("Editor Application is running");
            }
        }
    }
}
