using System.Collections.Generic;
using UnityEngine;

public class GridBuilding : MonoBehaviour
{
    new public string name;
    public GridType gridType;

    public GameObject gizmo;

    public float angle;
    
    private GridCell[] parentCells;
    [HideInInspector]
    public List<BuildingModule> spawnedBuildingModules; //this list is used to store all nested Building Modules
    private VagonGrid parentGrid;
    public VagonGrid ParentGrid { get => parentGrid; set => parentGrid = value; }
    public GridCell[] ParentCells { get => parentCells; set => parentCells = value; }

   

    
}
