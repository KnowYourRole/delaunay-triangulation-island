using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CreateMap))]
public class VoronoiScript : Editor
{

    public override void OnInspectorGUI()
    {
        CreateMap voronoiDiagram = (CreateMap)target;
        
        if (DrawDefaultInspector())
        {
            if (voronoiDiagram.autoUpdate)  //if the auto button is activated
                voronoiDiagram.Create();  //then Create
        }

        if (GUILayout.Button("Create"))   //if Create button in the edditor is pressed
            voronoiDiagram.Create();      //then Create
    }
}
