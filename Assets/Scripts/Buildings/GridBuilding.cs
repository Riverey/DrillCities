using System.Collections.Generic;
using UnityEngine;

public class GridBuilding : MonoBehaviour
{
    new public string name;
    public GridType gridType;

    public GameObject gizmo;

    [HideInInspector]
    public float angle;

    public int cost = 20;

    [HideInInspector]
    public List<BuildingModule> spawnedBuildingModules; //this list is used to store all nested Building Modules
    private VagonGrid parentGrid;
    [HideInInspector]
    public VagonGrid ParentGrid { get => parentGrid; set => parentGrid = value; }
    private List<GridCell> parentCells;
    [HideInInspector]
    public List<GridCell> ParentCells { get => parentCells; set => parentCells = value; }

    public void UnnoccupyCells()
    {
        foreach (GridCell cell in ParentCells)
        {
            cell.IsOccupied = false;
            cell.building = null;
        }
    }
}
