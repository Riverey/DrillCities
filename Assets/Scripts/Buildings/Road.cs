using System.Collections.Generic;
using UnityEngine;

public class Road : GridBuilding
{
    [Header("Road")]
    public float traverseSpeed = 1.0f;
    [HideInInspector]
    public List<GridCell> connectedBuildings = new List<GridCell>();
    [HideInInspector] 
    public List<GridCell> crossings;
    [HideInInspector]
    public List<GridCell> perpendicularRoads;
    [HideInInspector]
    public List<GridCell> tangentRoads;
    
    public Elevator elevator = null;

    public GridBuilding crossingObject;

    private void Start()
    {
        foreach (GridCell cell in ParentCells[0].neighborCells)
        {
            if (cell.gridType == GridType.cross) crossings.Add(cell);
        }
        SpawnCrossings();
        RecalculateElevators();

        if (ParentGrid == null) ParentGrid = ParentCells[0].parentGrid;
    }

    public void SpawnCrossings()
    {
        GridCell targetGridCell = ParentCells[0];
        VagonGrid targetGrid = targetGridCell.parentGrid;

        perpendicularRoads = new List<GridCell>();
        int x = (int)targetGridCell.coordinates.x;
        int y = (int)targetGridCell.coordinates.y;

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
                        if (cell.neighborCells.Contains(crossCell) && !crossCell.IsOccupied) BuildingSystem.BuildRequest(new GridCell[1] { crossCell }, crossCell.cellCenter, crossCell.angle, crossingObject);
                    }
                }
            }
    }

    public void RecalculateElevators()
    {
        GridCell targetGridCell = ParentCells[0];
        VagonGrid targetGrid = targetGridCell.parentGrid;

        tangentRoads = new List<GridCell>();

        int x = (int)targetGridCell.coordinates.x;
        int y = (int)targetGridCell.coordinates.y;

        if (targetGridCell.coordinates.x % 2 == 0)
        {
            tangentRoads.Add(targetGrid.grid[x, targetGrid.parentVagon.wrapInt(y, 1)]);
            tangentRoads.Add(targetGrid.grid[x, targetGrid.parentVagon.wrapInt(y, -1)]);
        }
        else
        {
            if (x > 1) { tangentRoads.Add(targetGrid.grid[x - 2, y]); }
            if (x < targetGrid.actualRows - 2) { tangentRoads.Add(targetGrid.grid[x + 2, y]); }
        }

        List<Road> tangentBuiltRoads = new List<Road>();
        List<Elevator> foundElevators = new List<Elevator>();

        foreach (GridCell cell in tangentRoads)
        {
            if (cell.IsOccupied && cell.building != null)
            {
                tangentBuiltRoads.Add((Road)cell.building);
                //Debug.Log("Found one neighbor with a road!");
            }
        }

        foreach (Road road in tangentBuiltRoads)
        {
            if (road.elevator != null)
            {
                foundElevators.Add(road.elevator);
            } //if one of the tangent road already has an elevator, add this cell to the list of it's roads
        }
                
        switch (foundElevators.Count)
        {
            case 0:
                {
                    if (tangentBuiltRoads.Count == 0) return;

                    //Debug.Log("Found no elevators so spawning one");
                    GameObject futureElevator = new GameObject("Elevator");
                    futureElevator.transform.parent = targetGrid.gridBuildingsHolder.transform;
                    futureElevator.transform.position = targetGridCell.cellCenter;
                    futureElevator.transform.rotation = transform.rotation; // match the rotation of the new elevator with the current road
                    Elevator elevatorScript = futureElevator.AddComponent<Elevator>();
                    elevatorScript.roads = new List<Road>();
                    elevatorScript.ParentGrid = targetGrid;
                    elevatorScript.roads.Add(this);
                    elevatorScript.gridType = GridType.road;
                    elevatorScript.coordinates = targetGridCell.coordinates;
                    
                    elevator = elevatorScript;
                    foreach (Road road in tangentBuiltRoads)
                    {
                        elevatorScript.roads.Add(road);
                        road.elevator = elevatorScript;
                    }
                    break;
                }
            case 1:
                if (!foundElevators[0].roads.Contains(this))
                {
                    foundElevators[0].roads.Add(this);
                    elevator = foundElevators[0];
                }
                break;
            case 2:
                {
                    List<Road> tempList = foundElevators[1].roads;
                    Destroy(foundElevators[1].gameObject);
                    foreach (Road road in tempList)
                    {
                        if (!foundElevators[0].roads.Contains(road)) foundElevators[0].roads.Add(road);
                        road.elevator = foundElevators[0];
                    }
                    elevator = foundElevators[0];
                    foundElevators[0].roads.Add(this);
                    break;
                } //if more then one elevator found, destroy the second one and add it's roads to the                 
        }
    }

    public void RequestDestroy()
    {
        UnnoccupyCells();
        if (elevator != null) elevator.RequestDestroy(this);
        Destroy(gameObject);
    }

    public void SpawnNewElevator(GridCell[] roads)
    {

    }
}


