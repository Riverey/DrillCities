using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;

[CustomEditor(typeof(Vagon))]
public class Vagon_editor : Editor
{
    public override void OnInspectorGUI()
    {
        Vagon myVagon = (Vagon)target;

        DrawDefaultInspector();
        
        EditorGUILayout.PrefixLabel("Vagon grid editor", EditorStyles.boldLabel);

        int segmentsAmmount = EditorGUILayout.IntSlider("Segments", myVagon.SegmentsAmmount, 0, 64);
        int rowsAmmount = EditorGUILayout.IntSlider("Rows", myVagon.RowsAmmount, 0, 64);

        if (segmentsAmmount > 1) myVagon.SegmentsAmmount = segmentsAmmount;
        if (rowsAmmount > 1) myVagon.RowsAmmount = rowsAmmount;

        EditorGUILayout.LabelField("Radius", myVagon.Radius.ToString());
        EditorGUILayout.LabelField("Length", myVagon.Length.ToString());

        EditorGUILayout.Space();
        if (GUILayout.Button("EraseGrid") && myVagon.DebugIsOn)
        {
            foreach (BuildingSystem.VagonGrid grid in myVagon.Grids)
            {
                myVagon.EraseGrid(grid);
            }
        }
    }
}

[CustomEditor(typeof(ResourseSystem))]
public class ResourseSystem_editor : Editor
{
    public override void OnInspectorGUI()
    {
        ResourseSystem myResourseSystem = (ResourseSystem)target;
        EditorGUILayout.LabelField("Name", myResourseSystem.resourseName);
    }
}

[CustomEditor(typeof(GridCell))]
public class GridCell_editor : Editor
{
    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();
        GridCell cell = (GridCell)target;
        EditorGUILayout.LabelField("Coordinates", cell.coordinates.ToString());

        EditorGUILayout.PrefixLabel("Neighbors", EditorStyles.boldLabel);

        for (int i = 0; i < cell.neighbors.Length; i++)
        {
            if (cell.neighbors[i] != null)
            {
                EditorGUILayout.LabelField("Neighbor " + i, cell.neighbors[i].name);
            }
        }

        cell.cellName = (TextMeshPro)EditorGUILayout.ObjectField("Cell name", cell.cellName, typeof(TextMeshPro), true);
    }
}




