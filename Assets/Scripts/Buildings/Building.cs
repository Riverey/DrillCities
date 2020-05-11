using System.Collections.Generic;
using UnityEngine;

public class Building : GridBuilding
{
    [Header("Building")]
    public Vector2 size = new Vector2(0.0f, 0.0f);

    public List<BuildingModule> initialBuildingModulesPool; //use this list to store building modules that has to be spawned initially
    public int initialBuildingModulesAmmount;

    public string description;
    public Sprite imageDescription;
    [HideInInspector]
    public List<GridCell> connectedRoads = new List<GridCell>();
    [HideInInspector]
    public List<GridCell> blockedRoads = new List<GridCell>();
    [HideInInspector]
    public List<GridCell> blockedCrossings = new List<GridCell>();

    public bool requiresAHole = false;

    private void Start()
    {
        SpawnInitialModules();

        if (requiresAHole)
        {
            VagonGrid targetGrid = null;

            foreach (VagonGrid grid in ParentGrid.parentVagon.Grids)
            {
                if (!grid.isBuildable && grid.gridName == "Hull")
                {
                    targetGrid = grid;
                }
            }
            if (targetGrid != null)
            {
                foreach (GridCell cell in ParentCells)
                {
                    targetGrid.grid[(int)cell.coordinates.x, (int)cell.coordinates.y].cellGizmo.SetActive(false);
                }
            }
        }
    }

    void SpawnInitialModules()
    {
        while (spawnedBuildingModules.Count < initialBuildingModulesAmmount)
        {
            BuildingModule randomModule = initialBuildingModulesPool[(Random.Range(0, initialBuildingModulesPool.Count))];
            if (randomModule != null && !randomModule.isOccupied) randomModule.SpawnModule();
        } //while there are less modules then desired ammount, keep trying to spawn at a random Building Point
        
    }

    void DetectOccupiedRoads (GridCell[] gridCells)
    {
        for (int i = 0; i < gridCells.Length; i++)
        {
            //look up all neighbors of cells and if some of them are stored more then twice, mark them as
        }
    }
}
