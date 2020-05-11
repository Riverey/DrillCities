using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class Vagon : MonoBehaviour
{
    [HideInInspector]
    public GameObject vagonGridsHolder;

    public Transform centerPivot;

    public CapsuleCollider vagonCollider;

    public List<Building> vagonBuildings = new List<Building>(); //list to stare all buildings built on this vagon

    [SerializeField]
    private int segmentsAmmount = 12; public int SegmentsAmmount { get => segmentsAmmount; set { if (segmentsAmmount != value) { segmentsAmmount = value; RecalculateVariables(); } } }
    [SerializeField]
    private int rowsAmmount = 6; public int RowsAmmount { get => rowsAmmount; set { if (rowsAmmount != value) { rowsAmmount = value; RecalculateVariables(); } } }
    private int cellProportion; public int CellProportion { get => cellProportion; set { if (cellProportion != value) { cellProportion = value; RecalculateVariables(); } } }

    private float radius; public float Radius { get => radius; set { if (radius != value) { radius = value; } } }
    private float length; public float Length { get => length; set { if (length != value) { length = value; } } }

    [SerializeField] private VagonGrid[] grids; public VagonGrid[] Grids { get => grids; set => grids = value; }

    [System.Serializable]
    public class BuildingStartSpawn
    {
        public GameObject buildingPrefab;
        public Vector2 desiredStartCell;
    }
    [SerializeField]
    public List<BuildingStartSpawn> buildingsToSpawnOnStart;

    public GameObject elevatorCabin;

    private void Start()
    {
        RecalculateVariables();

        foreach (VagonGrid grid in grids)
        {
            GenerateNeighbors(grid);
        }

        BuildingSystem.allVagons.Add(this);

        SpawnAtStart();
    }

    /// <summary>
    /// Method to recalculate all Vagon variables, called when any one of them is changed
    /// </summary>
    private void RecalculateVariables()
    {
        if (centerPivot == null) centerPivot = gameObject.transform;
        if (vagonCollider == null) vagonCollider = gameObject.GetComponent<CapsuleCollider>();

        Radius = SegmentsAmmount / (2 * Mathf.PI); //making sure we always have an integer number of segments
        Length = RowsAmmount;
        vagonCollider.radius = radius;
        vagonCollider.height = length + (radius * 2.0f);
        
        foreach (VagonGrid grid in Grids) DrawGrid(grid); //redraw all the grids
    }

    /// <summary>
    /// Function to populate arrays with grid objects
    /// </summary>
    public void DrawGrid(VagonGrid grid)
    {
        EraseGrid(grid); //dumping the previous grid

        grid.parentVagon = this;

        if (vagonGridsHolder == null) 
        {
            vagonGridsHolder = new GameObject("Vagon Grids");
            vagonGridsHolder.transform.parent = gameObject.transform;
            vagonGridsHolder.transform.localPosition = Vector3.zero;
            vagonGridsHolder.transform.localRotation = Quaternion.identity;
        } //checking if the hierarchy gameobjects exists and if not creates a new one to store all grid cells

        if (grid.gridHolder == null)
        {
            grid.gridHolder = new GameObject(grid.gridName + " Grid");
            grid.gridHolder.transform.parent = vagonGridsHolder.transform;
            grid.gridHolder.transform.localPosition = Vector3.zero;
            grid.gridHolder.transform.localRotation = Quaternion.identity;
        } //check if grid holder exists

        if (grid.gridBuildingsHolder == null)
        {
            grid.gridBuildingsHolder = new GameObject(grid.gridName + "Grid Buildings Holder");
            grid.gridBuildingsHolder.transform.parent = vagonGridsHolder.transform;
            grid.gridBuildingsHolder.transform.localPosition = Vector3.zero;
            grid.gridBuildingsHolder.transform.localRotation = Quaternion.identity;
        } //checking if the hierarchy gameobjects exists and if not creates a new one to store all grid cells

        switch (grid.gridType)
        {
            case GridType.main:
                grid.actualRows = RowsAmmount;
                break;
            case GridType.road:
                grid.actualRows = RowsAmmount * 2 + 1;
                break;
            case GridType.cross:
                grid.actualRows = RowsAmmount + 1;
                break;
        } //calculating actual row ammounts for this type of grid

        grid.grid = new GridCell[grid.actualRows,SegmentsAmmount]; //creating a new 2d array to store all grid cells of this grid        

        for (int i = 0; i < grid.actualRows; i++) 
        {
            for (int j = 0; j < SegmentsAmmount; j++)
            {

                float angle = (j * Mathf.PI * 2 / SegmentsAmmount); //angle in radians

                float yOffset = i;

                switch (grid.gridType) 
                {
                    case GridType.main:
                        yOffset = yOffset.Remap(0.0f, grid.actualRows - 1, -Length / 2 + 0.5f, Length / 2 - 0.5f);
                        break; //no need for offset for regular buildings
                    case GridType.road:
                        int gridOffset = i % 2 == 0 ? 0 : 1; //each second row will be offset using this int
                        angle += gridOffset * (Mathf.PI / SegmentsAmmount);
                        yOffset = yOffset.Remap(0.0f, grid.actualRows - 1, -Length / 2, Length / 2);
                        break;
                    case GridType.cross:
                        angle += (Mathf.PI / SegmentsAmmount);
                        yOffset = yOffset.Remap(0.0f, grid.actualRows - 1, -Length / 2, Length / 2);
                        break;
                } //calculating grid row offsets for different types of grids

                Vector3 cellCenterTemp = new Vector3(yOffset, Mathf.Cos(angle) * Radius, -Mathf.Sin(angle) * Radius); //creating a vector for the grid cell coordinates

                grid.grid[i, j] = new GridCell
                {
                    parentGrid = grid,
                    coordinates = new Vector2(i, j), //storing cell numeric coordinates
                    angle = angle,
                    cellCenter = cellCenterTemp,                    
                    IsOccupied = false,
                    gridType = grid.gridType
            }; //creating the Grid Cell and storing it's parameters

                GridCell currentGridCell = grid.grid[i, j];

                GameObject cellGizmo = Instantiate(grid.cellObject, grid.gridHolder.transform); //storing the debug grid cell we are working on right now
                currentGridCell.cellGizmo = cellGizmo; //assigning the gizmo object to it's cell object

                cellGizmo.transform.localPosition = cellCenterTemp;

                if (grid.gridType == GridType.road) cellGizmo.transform.localRotation = i % 2 == 0 ? Quaternion.Euler(new Vector3(0, 90, -angle * 180 / Mathf.PI)) : Quaternion.Euler(new Vector3(-angle * 180 / Mathf.PI, 0, 0)); //spawning roads with different rotation depending on the row
                else cellGizmo.transform.localRotation = Quaternion.Euler(new Vector3(-angle * 180 / Mathf.PI, 0, 0)); //rotating buildings

                cellGizmo.name = grid.gridName + " [" + i + "][" + j + "]";

                if (Application.isPlaying) currentGridCell.material = cellGizmo.GetComponentInChildren<MeshRenderer>().material;
            }
        } //spawning cells and storing them

        if (grid.isBuildable) grid.gridHolder.SetActive(false);
    }

    public void EraseGrid(VagonGrid grid)
    {
        if (grid.gridHolder != null) DestroyImmediate(grid.gridHolder);
        grid.grid = null; //dumping the previous array
    }

    /// <summary>
    /// This methode is used to cycle through grid cells in a specific grid and generating neighbors for them
    /// </summary>
    /// <param name="targetGrid"></param>
    public void GenerateNeighbors(VagonGrid targetGrid)
    {
        switch (targetGrid.gridType)
        {
            case GridType.main:
                for (int i = 0; i < targetGrid.actualRows; i++)
                {
                    for (int j = 0; j < SegmentsAmmount; j++)
                    {
                        GridCell targetGridCell = targetGrid.grid[i, j];
                        targetGridCell.neighborCells = new List<GridCell>();

                        #region Lokating neighboring roads
                        VagonGrid targetRoadGrid = null;

                        foreach (VagonGrid grid in grids)
                        {
                            if (grid.gridType == GridType.road) { targetRoadGrid = grid; break; }//locating the first road type grid in this vagon and storing it  
                        }

                        if (targetRoadGrid != null)
                        {
                            targetGridCell.neighborCells.Add(targetRoadGrid.grid[2 * i, j]);
                            targetGridCell.neighborCells.Add(targetRoadGrid.grid[2 * i + 2, j]);
                            targetGridCell.neighborCells.Add(targetRoadGrid.grid[i * 2 + 1, wrapInt(j, -1)]);
                            targetGridCell.neighborCells.Add(targetRoadGrid.grid[i * 2 + 1, j]);
                        }
                        #endregion

                        foreach (GridCell gridCell in targetGridCell.neighborCells)
                        {
                            if (gridCell.neighborCells != null) gridCell.neighborCells.Add(targetGridCell);//add this building to all roads
                            else { gridCell.neighborCells = new List<GridCell>(); gridCell.neighborCells.Add(targetGridCell); }
                        }

                        #region Lokating neighboring cells
                        if (i > 0) targetGridCell.neighborCells.Add(targetGrid.grid[i - 1, j]);
                        if (i < targetGrid.actualRows - 1) targetGridCell.neighborCells.Add(targetGrid.grid[i + 1, j]);
                        targetGridCell.neighborCells.Add(targetGrid.grid[i, wrapInt(j, -1)]);
                        targetGridCell.neighborCells.Add(targetGrid.grid[i, wrapInt(j, 1)]);
                        #endregion
                    }
                }
                break;
            case GridType.road:
                for (int i = 0; i < targetGrid.actualRows; i++)
                {
                    for (int j = 0; j < SegmentsAmmount; j++)
                    {
                        GridCell targetGridCell = targetGrid.grid[i, j];
                        targetGridCell.neighborCells = new List<GridCell>();
                        VagonGrid targetCrossGrid = null;

                        foreach (VagonGrid grid in grids)
                        {
                            if (grid.gridType == GridType.cross) { targetCrossGrid = grid; break; }//locating the first road type grid in this vagon and storing it  
                        }

                        #region Locating neighboring roads
                        if (targetGridCell.coordinates.x % 2 == 0)
                        {
                            if (i > 0) { targetGridCell.neighborCells.Add(targetGrid.grid[i - 1, j]); targetGridCell.neighborCells.Add(targetGrid.grid[i - 1, wrapInt(j, - 1)]); }
                            targetGridCell.neighborCells.Add(targetGrid.grid[i, wrapInt(j, 1)]);
                            targetGridCell.neighborCells.Add(targetGrid.grid[i, wrapInt(j, -1)]);
                            if (i < targetGrid.actualRows - 1) { targetGridCell.neighborCells.Add(targetGrid.grid[i + 1, j]); targetGridCell.neighborCells.Add(targetGrid.grid[i + 1, wrapInt(j, -1)]); }

                            targetGridCell.neighborCells.Add(targetCrossGrid.grid[i / 2, wrapInt(j, -1)]);
                            targetGridCell.neighborCells.Add(targetCrossGrid.grid[i / 2, j]);
                        }
                        else
                        {
                            if (i > 1) { targetGridCell.neighborCells.Add(targetGrid.grid[i - 2, j]); }
                            targetGridCell.neighborCells.Add(targetGrid.grid[i - 1, j]); targetGridCell.neighborCells.Add(targetGrid.grid[i - 1, wrapInt(j, 1)]);
                            if (i < targetGrid.actualRows - 2) { targetGridCell.neighborCells.Add(targetGrid.grid[i + 2, j]); }
                            if (i < targetGrid.actualRows - 1) { targetGridCell.neighborCells.Add(targetGrid.grid[i + 1, j]); targetGridCell.neighborCells.Add(targetGrid.grid[i + 1, wrapInt(j, 1)]); }
                            
                            targetGridCell.neighborCells.Add(targetCrossGrid.grid[(i - 1) / 2, j]);
                            targetGridCell.neighborCells.Add(targetCrossGrid.grid[(i - 1) / 2 + 1, j]);
                        }
                        #endregion
                    }
                }
                
                        break;
            case GridType.cross:
                break;
            default:
                break;
        }
    }

    public int wrapInt(int target, int direction)
    {
        if (direction > 0) target = target < SegmentsAmmount - 1 ? target + 1 : 0;
        else if (direction < 0) target = target > 0 ? target - 1 : SegmentsAmmount - 1;
        return target;
    }

    public Vector3 GetLocalCoordinates(Vector3 point)
    {
        point = transform.InverseTransformPoint(point);
        return point;
    }

    public Vector3 GetWorldCoordinates(Vector3 point)
    {
        point = transform.TransformPoint(point);
        return point;
    }

    void SpawnAtStart()
    {
        foreach (BuildingStartSpawn spawn in buildingsToSpawnOnStart)
        {
            VagonGrid targetGrid = null;
            GridBuilding targetGridBuildingScript = spawn.buildingPrefab.GetComponent<GridBuilding>();
            GridCell[] targetedGridArea;
            foreach (VagonGrid grid in grids)
            {
                if (grid.gridType == targetGridBuildingScript.gridType)
                {
                    targetGrid = grid;
                    break;
                } //getting first matching grid. #tooptimize
            }
            if (targetGrid != null)
            {
                if (targetGrid.gridType == GridType.main) targetedGridArea = BuildingSystem.GenerateAreaFromGridCellCoords(targetGrid, spawn.desiredStartCell, ((Building)targetGridBuildingScript).size);
                else targetedGridArea = BuildingSystem.GenerateAreaFromGridCellCoords(targetGrid, spawn.desiredStartCell, new Vector2(1, 1));

                Vector4 centerAndAngle;
                if (targetedGridArea != null) centerAndAngle = BuildingSystem.FindCenterAndAngleGromArea(targetedGridArea);
                else break;

                Vector3 buildingCenter;
                float buildingAngle;

                if (centerAndAngle != null) 
                { 
                    buildingCenter = new Vector3(centerAndAngle.x, centerAndAngle.y, centerAndAngle.z); 
                    buildingAngle = centerAndAngle.w;
                    BuildingSystem.BuildRequest(targetedGridArea, buildingCenter, buildingAngle, targetGridBuildingScript);
                }
                else break;

            }
            else break;
        }
    }
}



public static class ExtensionMethods
{
    /// <summary>
    /// Use to remap a float from one range to another
    /// </summary>
    /// <returns></returns>
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}