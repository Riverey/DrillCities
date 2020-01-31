using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Vagon))]
public class Vagon_editor : Editor
{
    public override void OnInspectorGUI()
    {
        Vagon myVagon = (Vagon)target;

        DrawDefaultInspector();
        
        EditorGUILayout.PrefixLabel("Vagon grid editor", EditorStyles.boldLabel);

        int segmentsAmmount = EditorGUILayout.IntSlider("Segments", myVagon.SegmentsAmmount, 0, 63);
        int rowsAmmount = EditorGUILayout.IntSlider("Rows", myVagon.RowsAmmount, 0, 33);

        if (segmentsAmmount > 1) myVagon.SegmentsAmmount = segmentsAmmount;
        if (rowsAmmount > 1) myVagon.RowsAmmount = rowsAmmount;

        EditorGUILayout.LabelField("Radius", myVagon.Radius.ToString());
        EditorGUILayout.LabelField("Length", myVagon.Length.ToString());

        EditorGUILayout.Space();
        if (GUILayout.Button("Erase Grid"))
        {
            foreach (VagonGrid grid in myVagon.Grids)
            {
                myVagon.EraseGrid(grid);
            }
        }
        if (GUILayout.Button("ReDraw Grid"))
        {
            foreach (VagonGrid grid in myVagon.Grids)
            {
                myVagon.DrawGrid(grid);
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




