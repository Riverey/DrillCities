using System.Collections.Generic;
using UnityEngine;

public class GridBuilding : MonoBehaviour
{
    new public string name;
    public GridType gridType;

    public GameObject gizmo;

    [HideInInspector]
    public float angle;
    
    

    [HideInInspector]
    public List<BuildingModule> spawnedBuildingModules; //this list is used to store all nested Building Modules
    private VagonGrid parentGrid;
    public VagonGrid ParentGrid { get => parentGrid; set => parentGrid = value; }
    private List<GridCell> parentCells;
    public List<GridCell> ParentCells { get => parentCells; set => parentCells = value; }

   

    
}
