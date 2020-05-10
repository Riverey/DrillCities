using System.Collections.Generic;
using UnityEngine;

public class Road : GridBuilding
{
    [Header("Road")]
    public float traverseSpeed = 1.0f;
    [HideInInspector]
    public List<GridCell> connectedBuildings = new List<GridCell>();
    [HideInInspector] 
    public List<GridCell> adjacentRoads = new List<GridCell>();
    [HideInInspector] 
    public List<GridCell> crossings;

    public GridBuilding crossingObject;

    private void Start()
    {
        foreach (GridCell cell in ParentCells[0].neighborCells)
        {
            if (cell.gridType == GridType.cross) crossings.Add(cell);
        }
        SpawnCrossings();
    }

    public void SpawnCrossings()
    {
        GridCell targetGridCell = ParentCells[0];
        VagonGrid targetGrid = targetGridCell.parentGrid;
        List<GridCell> perpendicularRoads = new List<GridCell>();
        int x = (int)targetGridCell.coordinates.x;
        int y = (int)targetGridCell.coordinates.y;

        #region Locating neighboring roads
        if (x % 2 == 0)
        {
            if (x > 0) { perpendicularRoads.Add(targetGrid.grid[x - 1, y]); perpendicularRoads.Add(targetGrid.grid[x - 1, targetGrid.parentVagon.wrapInt(y, -1)]); }
            if (x < targetGrid.actualRows - 1) { perpendicularRoads.Add(targetGrid.grid[x + 1, y]); perpendicularRoads.Add(targetGrid.grid[x + 1, targetGrid.parentVagon.wrapInt(y, -1)]); }
        }
        else
        {
            perpendicularRoads.Add(targetGrid.grid[x - 1, y]); perpendicularRoads.Add(targetGrid.grid[x - 1, targetGrid.parentVagon.wrapInt(y, 1)]);
            if (x < targetGrid.actualRows - 1) { perpendicularRoads.Add(targetGrid.grid[x + 1, y]); perpendicularRoads.Add(targetGrid.grid[x + 1, targetGrid.parentVagon.wrapInt(y, 1)]); }
        }

        foreach (GridCell cell in perpendicularRoads)
        {
            if (cell.IsOccupied)
            {
                foreach (GridCell crossCell in crossings)
                {
                    if (cell.neighborCells.Contains(crossCell)) BuildingSystem.BuildRequest(new GridCell[1] { crossCell }, crossCell.cellCenter, crossCell.angle, crossingObject);
                }
            }
        }
        #endregion
    }
}


