using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class GridCell
{
    public GridType gridType;
    public VagonGrid parentGrid; //store parent grid of the cell
    public Vector2 coordinates;
    public Vector3 cellCenter; //transform of the cell regarding to the Vagon body
    public float angle;
    public GameObject cellGizmo; //associated gizmo object
    private bool isOccupied;
    public bool IsOccupied { get => isOccupied; set { isOccupied = value; if (material != null) { int i = isOccupied ? 1 : 2; material.SetInt("isOccupied", i); if (cellGizmo != null) { cellGizmo.gameObject.GetComponentInChildren<MeshRenderer>().sortingOrder = isOccupied ? 1 : 2; } } } }
    public GridBuilding building; //reference to the building that is built on this cel
    public List<GridCell> neighborCells;
    public Material material;   
} //a class to contain information about cells

/// <summary>
/// Class to hold information about an entire grid
/// </summary>
[System.Serializable]
public class VagonGrid
{
    public string gridName;
    public bool isBuildable;
    public Vagon parentVagon; //store a link to the parent vagon script
    public GridType gridType;
    public GameObject gridHolder; //game object to store all drid objects in for organisation
    public GameObject cellObject; //place to store a prefab for this grid
    public GameObject gridBuildingsHolder;
    [HideInInspector] 
    public int actualRows;
    public GridCell[,] grid; //2d array used to store grid cells
}

public enum GridType { main, road, cross }

public class BuildingSystem : MonoBehaviour
{
    private UImanager uiManager;
    private ResourseManager resourseManager;

    public static List<Vagon> allVagons = new List<Vagon>();
    public static List<GridBuilding> allBuildings;

    [System.Serializable] public class BuildingType { public GridBuilding[] buildings; }
    public static List<BuildingType> buildings;

    public static bool isBuilding = false;
    
    private static GridType currentGridType = GridType.main;
    public static GridType CurrentGridType { get => currentGridType; set => currentGridType = value; }

    private static GridBuilding currentTargetBuilding;
    public static GridBuilding CurrentTargetBuilding { get => currentTargetBuilding; set { currentTargetBuilding = value; if (value != null) { CurrentGridType = currentTargetBuilding.gridType; } } }

    enum CurrentBuildState { none, building, demolishing } //current state of the Building System
    private CurrentBuildState CurrentBuildState1 { get; set; } = CurrentBuildState.none;

    private GridCell[] targetGridAreaCash;

    private GameObject currentHoverGizmo;

    public int hoverShadowSize = 3;
    public bool drawGizmos = false;

    public static List<List<GridCell>> elevators = new List<List<GridCell>>();

    private void Start()
    {
        uiManager = FindObjectOfType<UImanager>();
        resourseManager = FindObjectOfType<ResourseManager>();

        isBuilding = false;
    }

    void Update()
    {
        if (isBuilding)
        {
            BuildingUpdate();

            if(Input.GetKeyDown("escape"))
            {
                BuildingStateTrigger();
            } //if escape pressed whilst in build mode, exit
        }
    }

    /// <summary>
    /// This method has to be called by UI buttons and accepts a building
    /// </summary>
    public void BuildingStateTrigger(GridBuilding gridBuilding = null)
    {
        if (!isBuilding)
        {
            isBuilding = true;
            CurrentTargetBuilding = gridBuilding;

            foreach (Vagon vagon in allVagons)
            {
                foreach (VagonGrid grid in vagon.Grids)
                {
                    if (grid.gridType == CurrentTargetBuilding.gridType) grid.gridHolder.SetActive(true);
                    else if (grid.isBuildable) grid.gridHolder.SetActive(false);
                }
            }
        }
        else if (gridBuilding != null)
        {
            if (currentHoverGizmo != null) Destroy(currentHoverGizmo);
            CurrentTargetBuilding = gridBuilding;

            foreach (Vagon vagon in allVagons)
            {
                foreach (VagonGrid grid in vagon.Grids)
                {
                    if (grid.gridType == CurrentTargetBuilding.gridType ) grid.gridHolder.SetActive(true);
                    else if (grid.isBuildable) grid.gridHolder.SetActive(false);
                }
            }
        } //if called when isBuilding active and with a new grid building, check it's type
        else
        {
            isBuilding = false;
            if (CurrentTargetBuilding != null) CurrentTargetBuilding = null;
            if (currentHoverGizmo != null) Destroy(currentHoverGizmo);

            foreach (Vagon vagon in allVagons)
            {
                foreach (VagonGrid grid in vagon.Grids)
                {
                    if (grid.isBuildable) grid.gridHolder.SetActive(false);
                }
            }
        } //if called when isBuilding already active and without passing a building, flip back to inactive state
    }

    void BuildingUpdate()
    {
        ClearOldArea();
        GridCell[] targetGridArea = null;

        if (CurrentGridType == GridType.main) { Vector2 targetSize = new Vector2(((Building)CurrentTargetBuilding).size.x, ((Building)CurrentTargetBuilding).size.y);  targetGridArea = VagonRaycast(targetSize.x, targetSize.y); }
        else targetGridArea = VagonRaycast();

        if (targetGridArea != null) 
        {
            ClearOldArea(); //wipe storage for old area
            targetGridAreaCash = targetGridArea; //storing the current gridArea in the old Area
            bool isOccupied = false; //use to store info about current target area
            VagonGrid targetGrid = targetGridArea[0].parentGrid;
            Vagon targetVagon = targetGrid.parentVagon;

            #region Calculating the center of the target area
            Vector4 centerAndAngle = FindCenterAndAngleGromArea(targetGridArea);
            Vector3 buildingCenter = new Vector3(centerAndAngle.x, centerAndAngle.y, centerAndAngle.z);
            float buildingAngle = centerAndAngle.w;
            #endregion

            #region Displaying gizmo
            if (drawGizmos)
            {
                if (currentHoverGizmo == null) currentHoverGizmo = Instantiate(CurrentTargetBuilding.gizmo);
                currentHoverGizmo.transform.parent = targetVagon.transform;
                currentHoverGizmo.transform.localPosition = buildingCenter;
                switch (CurrentGridType)
                {
                    case GridType.main:
                        currentHoverGizmo.transform.localRotation = Quaternion.Euler(new Vector3(-buildingAngle * 180 / Mathf.PI, 0, 0));
                        break;
                    case GridType.road:
                        Debug.Log(targetGridAreaCash[0].coordinates);
                        currentHoverGizmo.transform.localRotation = targetGridArea[0].coordinates.x % 2 == 0 ? Quaternion.Euler(new Vector3(0, 90, -buildingAngle * 180 / Mathf.PI)) : Quaternion.Euler(new Vector3(-buildingAngle * 180 / Mathf.PI, 0, 0));
                        break;
                    case GridType.cross:
                        currentHoverGizmo.transform.localRotation = Quaternion.Euler(new Vector3(-buildingAngle * 180 / Mathf.PI, 0, 0));
                        break;
                } //setting the rotation according to the type of the building
            }
            #endregion

            #region Checking if all cells are unoccupied
            foreach (GridCell gridCell in targetGridArea)
            {
                gridCell.material.SetInt("isHovered", 1); //checking the material property to reflect that the cell is being hovered
                gridCell.material.SetFloat("Opacity", 1);
                if (gridCell.IsOccupied)
                {
                    gridCell.material.SetInt("isOccupied", 1);
                    isOccupied = true; //if one of the cells is occupied, building can not be build
                }
            } //managing the color of of all cells in the target area
            #endregion    

            #region If button is pressed, call building request
            if (!isOccupied && Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                BuildRequest(targetGridArea, buildingCenter, buildingAngle); //request building in the target area an target center
            }
            else if (isOccupied && Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                uiManager.LogRequest("BuildError","Space is occupied");
            }
            #endregion

        } //if succesfully found an area, proceed to highlighting the area and drawing a gizmo
        else if (currentHoverGizmo != null) DestroyImmediate(currentHoverGizmo);

    } //if isBuilding is true, this code will do the building update

    /// <summary>
    /// Methode used to generate a center of a grid cell area as well as the appropriate cylinder angle. Angle is stored as z component of the vector
    /// </summary>
    /// <param name="targetGridArea"></param>
    /// <returns></returns>
    public static Vector4 FindCenterAndAngleGromArea (GridCell[] targetGridArea)
    {
        Vector3 buildingCenter;
        float buildingAngle;
        Vagon targetVagon = targetGridArea[0].parentGrid.parentVagon;

        if (CurrentGridType == GridType.main)
        {
            float centX = 0.0f;
            float centY = 0.0f;

            for (int i = 0; i < targetGridArea.Length; i++)
            {
                centX += targetGridArea[i].coordinates.x;

                if (targetGridArea[0].angle - targetGridArea[i].angle < Mathf.PI)
                {
                    centY = centY - (2 * Mathf.PI - targetGridArea[i].angle);
                } //checking if the object is on the edge
                else centY += targetGridArea[i].angle; //if not on the edge - all cool, proceed to adding and averaging angles
            }

            centX = 0.0f - (targetGridArea[0].parentGrid.parentVagon.Length / 2) + centX / targetGridArea.Length + 0.5f;

            centY /= targetGridArea.Length;

            if (centY < 0) centY = 2 * Mathf.PI + centY;

            //Debug.Log(debugString + " / " + centY);

            buildingAngle = centY;

            buildingCenter = new Vector3(centX, Mathf.Cos(centY) * targetVagon.Radius, -Mathf.Sin(centY) * targetVagon.Radius);
            ;
        } //calculating the center of the building for buildings
        else
        {
            buildingCenter = targetGridArea[0].cellCenter;
            buildingAngle = targetGridArea[0].angle;
        }//getting the center of the cell for roads and crosses

        return new Vector4(buildingCenter.x, buildingCenter.y, buildingCenter.z, buildingAngle);
    }

    /// <summary>
    /// Clear the 2d array with GridCells
    /// </summary>
    public void ClearOldArea()
    {
        if (targetGridAreaCash != null)
        {
            foreach (GridCell gridCell in targetGridAreaCash)
            {
                gridCell.material.SetInt("isHovered", 0);
                gridCell.material.SetFloat("Opacity", 0.1f);
            }
        }//returning materials of the previous grid back to normal

    }

    /// <summary>
    /// Method to Raycast and locate a point on the surface of a vagon. If no size specified, will return a closest cell.
    /// </summary>
    /// <returns>A hit point with target grid and target vagon</returns>
    private GridCell[] VagonRaycast(float sizeX = 1.0f, float sizeY = 1.0f)
    {
        GridCell[] targetedGridArea = null;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit targetHitPoint, MouseOrbitImproved.distance + 10f) && targetHitPoint.transform.gameObject.tag == "RaycastTarget") //9 is the Vagons layer
        {
            CapsuleCollider colCollider = (CapsuleCollider)targetHitPoint.collider;
            Vagon targetVagon = targetHitPoint.transform.gameObject.GetComponent<Vagon>();

            VagonGrid targetGrid = null; //object to store the grid that we're pointing at

            Vector3 localHitPoint = targetVagon.GetLocalCoordinates(targetHitPoint.point); //storing local coordinates of the hit point
            Vector3 vagonHit = new Vector3(0.0f, localHitPoint.y, localHitPoint.z);

            foreach (VagonGrid grid in targetVagon.Grids)
            {
                if (grid.gridType == CurrentGridType && grid.isBuildable) targetGrid = grid;
            }

            if (targetGrid == null) return null; //stop if no grid found

            float hitX = Mathf.Clamp(localHitPoint.x.Remap(-targetVagon.Length / 2.0f, targetVagon.Length / 2.0f, 0.0f, targetGrid.actualRows - 1.0f), 0.0f, targetGrid.actualRows - 1.0f); //getting an X coordinate of the cell by remaping the position of the hit to local coordinates of the 
            float hitY = localHitPoint.z <= 0.0f ? Vector3.Angle(targetVagon.transform.up, vagonHit) : 360.0f - Vector3.Angle(targetVagon.transform.up, vagonHit); //for anges greater than 180 

            if (CurrentGridType == GridType.main)
            {
                targetedGridArea = GetGridArea(new Vector2(hitX, hitY), new Vector2(sizeX, sizeY), targetGrid);
            }
            else
            {
                hitX = Mathf.RoundToInt(hitX);
                for (int i = 0; i < targetGrid.parentVagon.SegmentsAmmount; i++)
                {
                    GridCell targetGridCell = targetGrid.grid[(int)hitX, i];
                    float angle = targetGridCell.angle * 180 / Mathf.PI; //converting from radians to angles

                    if (hitY - angle <= 180.0f / targetVagon.SegmentsAmmount)
                    {
                        targetedGridArea = new GridCell[1];
                        targetedGridArea[0] = targetGridCell;
                        break;
                    }
                }
            } //if a road or a cross, return a single cell

            //Debug.DrawRay(targetVagon.transform.position, targetVagon.transform.up * 100.0f);
            //Debug.DrawRay(targetVagon.transform.position, vagonHit, Color.red);

        } //if succesfully found a vagon

        return targetedGridArea;
    }
    /// <summary>
    /// Returns an array of all Grid Cells in the set area
    /// </summary>
    /// <param name="targetCoordinates">Center coordinate of the area requested</param>
    /// <param name="targetGrid"></param>
    /// <param name="allowXOversize">If true, will not return null if the area is out of X bounds</param>
    /// <returns></returns>
    GridCell[] GetGridArea(Vector3 targetCoordinates, Vector2 size, VagonGrid targetGrid, bool allowXOversize = false)
    {
        if (size.x > targetGrid.actualRows) size.x = targetGrid.actualRows;
        if (size.y > targetGrid.parentVagon.SegmentsAmmount) size.y = targetGrid.parentVagon.SegmentsAmmount;

        GridCell[] targetedGridArea;
        Vagon targetVagon = targetGrid.parentVagon;

        float hitX = targetCoordinates.x;
        float hitY = targetCoordinates.y;

        int minX = Mathf.RoundToInt(hitX - ((size.x - 1.0f) / 2.0f));
        int maxX = Mathf.RoundToInt(minX + size.x);
        if ((minX < 0 || maxX > targetVagon.RowsAmmount) && !allowXOversize)
        {
            ClearOldArea();
            return null; //if out of bounds return null
        }
        else if (minX < 0) { minX = 0; }
        else if (maxX > targetVagon.RowsAmmount) { maxX = targetVagon.RowsAmmount; }

        int minY = Mathf.RoundToInt((hitY / (360.0f / targetVagon.SegmentsAmmount)) - ((size.y - 1.0f) / 2.0f));

        targetedGridArea = GenerateAreaFromGridCellCoords(targetGrid, new Vector2(minX, minY), size);

        return targetedGridArea;
    }


    public static void BuildRequest(GridCell[] targetGridArea, Vector3 buildingCenter, float angle, GridBuilding directBuildingSpawn = null)
    {
        GameObject spawnedBuilding = directBuildingSpawn == null ? Instantiate(CurrentTargetBuilding.gameObject) : Instantiate(directBuildingSpawn.gameObject);
        spawnedBuilding.transform.parent = targetGridArea[0].parentGrid.gridBuildingsHolder.transform;
        spawnedBuilding.transform.localPosition = buildingCenter;

        GridBuilding spawnedBuildingScript = spawnedBuilding.GetComponent<GridBuilding>();
        spawnedBuildingScript.angle = angle;
        spawnedBuildingScript.ParentGrid = targetGridArea[0].parentGrid;

        GridType tempGridType = directBuildingSpawn == null ? CurrentGridType : directBuildingSpawn.gridType;

        switch (tempGridType)
        {
            case GridType.main:
                spawnedBuilding.transform.localRotation = angle > Mathf.PI ? Quaternion.Euler(new Vector3(angle * 180 / Mathf.PI, 180, 0)) : Quaternion.Euler(new Vector3(-angle * 180 / Mathf.PI, 0, 0));

                List <GridCell> connectedRoads = new List<GridCell>();
                List <GridCell> blockedCrossings = new List<GridCell>();
                foreach (GridCell cell in targetGridArea)
                {
                    for (int i = 0; i < 4; i++) //first 4 indexes in the neighbors array are occupied by roads
                    {
                        GridCell targetCell = cell.neighborCells[i];
                        if (connectedRoads.Contains<GridCell>(targetCell)) 
                        {
                            foreach (GridCell crossingCell in targetCell.neighborCells)
                            {
                                if (crossingCell.gridType == GridType.cross)
                                {
                                    if (!blockedCrossings.Contains(crossingCell))
                                    {
                                        blockedCrossings.Add(crossingCell);
                                    }
                                    else
                                    {
                                        if (crossingCell.IsOccupied)
                                        {
                                            if (crossingCell.building != null) Destroy(crossingCell.building.gameObject);
                                        }
                                        crossingCell.IsOccupied = true;
                                        ((Building)spawnedBuildingScript).blockedCrossings.Add(crossingCell);
                                        blockedCrossings.Remove(crossingCell);
                                    }
                                }
                            }
                            if (targetCell.IsOccupied)
                            {
                                ((Road)targetCell.building).RequestDestroy();
                            }
                            targetCell.IsOccupied = true;
                            connectedRoads.Remove(targetCell);
                            ((Building)spawnedBuildingScript).blockedRoads.Add(targetCell);
                        } //if encountered more then once, remove from the connected array and set isOccupied to true
                        else connectedRoads.Add(targetCell);
                    }

                    if (spawnedBuildingScript.ParentCells == null) spawnedBuildingScript.ParentCells = new List<GridCell>();

                    spawnedBuildingScript.ParentCells.Add(cell);
                } //foreach cell in target cell area, get it's neighbors and check their neighbors. If it was encountered more than once, it's blocked by the building
                foreach (GridCell cell in connectedRoads)
                {
                    ((Building)spawnedBuildingScript).connectedRoads.Add(cell);
                }
                
                break;
            case GridType.road:
                spawnedBuilding.transform.localRotation = targetGridArea[0].coordinates.x % 2 == 0 ? Quaternion.Euler(new Vector3(0, 90, -angle * 180 / Mathf.PI)) : Quaternion.Euler(new Vector3(-angle * 180 / Mathf.PI, 0, 0));
                break;
            case GridType.cross:
                spawnedBuilding.transform.localRotation = Quaternion.Euler(new Vector3(-angle * 180 / Mathf.PI, 0, 0));

                break;
        } //setting the rotation according to the type of the building

        foreach (GridCell gridCell in targetGridArea)
        {
            gridCell.IsOccupied = true;
            gridCell.material.SetInt("isOccupied", 1);
            gridCell.building = spawnedBuildingScript;
            if (spawnedBuildingScript.ParentCells == null) spawnedBuildingScript.ParentCells = new List<GridCell>(); 
            spawnedBuildingScript.ParentCells.Add(gridCell);
        }

        if (spawnedBuildingScript.cost != 0) ResourseManager.purchase(spawnedBuildingScript.cost);
    }

    /// <summary>
    /// This methode returns a grid Area based on the coordinates of a first cell
    /// </summary>
    /// <param name="targetGrid"></param>
    /// <param name="coordinates"></param>
    /// <param name="size"></param>
    public static GridCell[] GenerateAreaFromGridCellCoords(VagonGrid targetGrid, Vector2 coordinates, Vector2 size)
    {
        GridCell[] targetedGridArea = new GridCell[(int)(size.x * size.y)];

        Vagon targetVagon = targetGrid.parentVagon;
        int minX = (int)coordinates.x;
        int minY = (int)coordinates.y;
        int maxX = Mathf.RoundToInt(minX + size.x);
        int index = 0;

        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < minY + size.y; y++)
            {
                int tempY = y;
                if (y < 0)
                {
                    tempY = targetVagon.SegmentsAmmount + y;
                }
                else if (y > targetVagon.SegmentsAmmount - 1)
                {
                    tempY = y % targetVagon.SegmentsAmmount;
                }
                targetedGridArea[index] = targetGrid.grid[x, tempY];
                Mathf.Clamp(index++, 0, size.x * size.y - 1);
            }
        }
        return targetedGridArea;
    }

}

