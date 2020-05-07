using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridCell
{
    public VagonGrid parentGrid; //store parent grid of the cell
    public Vector2 coordinates;
    public float angle;
    public GameObject cellGizmo; //associated gizmo object
    public Vector3 cellCenter; //transform of the cell regarding to the Vagon body
    public Transform parent;
    private bool isOccupied;
    public bool IsOccupied { get => isOccupied; set { isOccupied = value; if (material != null) { int i = isOccupied ? 1 : 2; material.SetInt("isOccupied", i); if (cellGizmo != null) { Debug.Log("Success!"); cellGizmo.gameObject.GetComponentInChildren<MeshRenderer>().sortingOrder = isOccupied ? 1 : 2; } } } }
    public bool isBlocked; //use for cells that are blocked but something non-removable like leg pillars or roads inside buildings
    public GridBuilding building; //reference to the building that is built on this cel
    public Material material;   
} //a class to contain information about cells

/// <summary>
/// Class to hold information about an entire grid
/// </summary>
[System.Serializable]
public class VagonGrid
{
    public Vagon parentVagon; //store a link to the parent vagon script
    public string gridName;
    public GridType gridType;
    public GameObject gridHolder; //game object to store all drid objects in for organisation
    public GameObject cellObject; //place to store a prefab for this grid
    public int actualRows;
    public GridCell[,] grid; //2d array used to store grid cells
}

public enum GridType { main, road, cross }

public class BuildingSystem : MonoBehaviour
{
    private UImanager uiManager;

    public GameObject tempGridBuilding;

    public GameObject[] tempBuilding;
    public static List<Vagon> allVagons = new List<Vagon>();

    [System.Serializable] public class BuildingType { public GridBuilding[] buildings; }
    public List<BuildingType> buildings;

    public bool isBuilding = false;
    
    private GridType currentGridType = GridType.main;
    private GridBuilding currentTargetBuildingScript;
    private GameObject currentTargetBuilding; //use to store the currently selected building
    public GameObject CurrentTargetBuilding { get => currentTargetBuilding; set { currentTargetBuilding = value; currentTargetBuildingScript = currentTargetBuilding.GetComponent<GridBuilding>();  currentGridType = currentTargetBuildingScript.gridType; } }

    enum CurrentBuildState { none, building, demolishing } //current state of the Building System
    private CurrentBuildState CurrentBuildState1 { get; set; } = CurrentBuildState.none;
    
    private GridCell[] oldTargetGridArea;

    private GameObject currentHoverGizmo;

    private void Start()
    {
        CurrentTargetBuilding = tempGridBuilding;
        uiManager = FindObjectOfType<UImanager>();

        isBuilding = false;
    }

    void Update()
    {
        if (isBuilding)
        {
            BuildingUpdate();
        }

        if (Input.GetKeyDown("1"))
        {
            currentGridType = GridType.main;
            if (currentHoverGizmo) { DestroyImmediate(currentHoverGizmo); currentHoverGizmo = null; }
        }
        else if (Input.GetKeyDown("2"))
        {
            currentGridType = GridType.road;
            if (currentHoverGizmo) { DestroyImmediate(currentHoverGizmo); currentHoverGizmo = null; }
        }
    }

    /// <summary>
    /// This method has to be called by UI buttons and accepts a building
    /// </summary>
    public void BuildingStateTrigger(GameObject gridBuilding = null)
    {
        if (!isBuilding)
        {
            isBuilding = true;
            CurrentTargetBuilding = gridBuilding;

            foreach (Vagon vagon in allVagons)
            {
                foreach (VagonGrid grid in vagon.Grids)
                {
                    if (grid.gridType != GridType.cross)
                    {
                        if (isBuilding) { grid.gridHolder.SetActive(true); }
                        else { grid.gridHolder.SetActive(false); }
                    }
                }
            } //switch on vagon grids if it's the same as the current grid target
        }
        else
        {
            isBuilding = false;
            currentTargetBuilding = null;
            currentTargetBuildingScript = null;
        } //if called when isBuilding already active, flip back to inactive state
    }


    void BuildingUpdate()
    {
        ClearOldArea();
        GridCell[] targetGridArea = null;
        if (currentGridType == GridType.main) targetGridArea = VagonRaycast(((Building)currentTargetBuildingScript).size.x, ((Building)currentTargetBuildingScript).size.y);
        else targetGridArea = VagonRaycast();

        if (targetGridArea != null) 
        {
            ClearOldArea(); //wipe storage for old area

            oldTargetGridArea = targetGridArea; //storing the current gridArea in the old Area
            bool isOccupied = false; //use to store info about current target area

            #region Checking if all cells are unoccupied
            foreach (GridCell gridCell in targetGridArea)
            {
                gridCell.material.SetInt("isHovered", 1); //checking the material property to reflect that the cell is being hovered
                if (gridCell.IsOccupied)
                {
                    gridCell.material.SetInt("isOccupied", 1);
                    isOccupied = true; //if one of the cells is occupied, building can not be build
                }
            } //managing the color of of all cells in the target area
            #endregion               

            #region Calculating the center of the target area and displaying gizmo
            Vector3 buildingCenter = new Vector3(0.0f, 0.0f, 0.0f);
            float buildingAngle = 0.0f;

            if (currentGridType == GridType.main)
            {
                if (currentHoverGizmo == null) currentHoverGizmo = Instantiate(currentTargetBuildingScript.gizmo);

                float centX = 0.0f;
                float centY = 0.0f;

                for (int i = 0; i < targetGridArea.Length; i++)
                {
                    centX = centX + targetGridArea[i].coordinates.x;
                    centY = centY + targetGridArea[i].angle;
                }

                centX = 0.0f - (targetGridArea[0].parentGrid.parentVagon.Length / 2) + centX / targetGridArea.Length + 0.5f;
                centY /= targetGridArea.Length;

                buildingAngle = centY;

                Vagon targetVagon = targetGridArea[0].parentGrid.parentVagon;

                buildingCenter = new Vector3(centX, Mathf.Cos(centY) * targetVagon.Radius, -Mathf.Sin(centY) * targetVagon.Radius);

                currentHoverGizmo.transform.parent = targetVagon.transform;
                currentHoverGizmo.transform.localPosition = buildingCenter;

                transform.localRotation = Quaternion.identity;
                ;
            } //calculating the center of the building for buildings
            else
            {
                buildingCenter = targetGridArea[0].cellCenter;
            }//getting the center of the cell for roads and crosses

            #endregion

            #region If button is pressed, call building request
            if (!isOccupied && Input.GetMouseButtonDown(0))
            {
                BuildRequest(targetGridArea, buildingCenter, buildingAngle); //request building in the target area an target center
            }
            else if (isOccupied && Input.GetMouseButtonDown(0))
            {
                uiManager.LogRequest("BuildError","Space is occupied");
            }
            #endregion

        } //if succesfully found an area, proceed to highlighting the area and drawing a gizmo
        else if (currentHoverGizmo != null) DestroyImmediate(currentHoverGizmo);

    } //if isBuilding is true, this code will do the building update

    /// <summary>
    /// Clear the 2d array with GridCells
    /// </summary>
    public void ClearOldArea()
    {
        if (oldTargetGridArea != null)
        {
            foreach (GridCell gridCell in oldTargetGridArea)
            {
                gridCell.material.SetInt("isHovered", 0);
                if (!gridCell.IsOccupied) gridCell.material.SetInt("isOccupied", 0);
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
                if (grid.gridType == currentGridType) targetGrid = grid;
            }

            if (targetGrid == null) return null; //stop if no grid found

            float hitX = Mathf.Clamp(localHitPoint.x.Remap(-targetVagon.Length / 2.0f, targetVagon.Length / 2.0f, 0.0f, targetGrid.actualRows - 1.0f), 0.0f, targetGrid.actualRows - 1.0f); //getting an X coordinate of the cell by remaping the position of the hit to local coordinates of the 
            float hitY = localHitPoint.z <= 0.0f ? Vector3.Angle(targetVagon.transform.up, vagonHit) : 360.0f - Vector3.Angle(targetVagon.transform.up, vagonHit); //for anges greater than 180 

            if (currentGridType == GridType.main)
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

            Debug.DrawRay(targetVagon.transform.position, targetVagon.transform.up * 100.0f);
            Debug.DrawRay(targetVagon.transform.position, vagonHit, Color.red);

        } //if succesfully found a vagon

        return targetedGridArea;
    }
    /// <summary>
    /// Returns an array of all Grid Cells in the set area
    /// </summary>
    /// <param name="targetCoordinates">Center coordinate of the area requested</param>
    /// <param name="targetGrid"></param>
    /// <returns></returns>
    GridCell[] GetGridArea(Vector3 targetCoordinates, Vector2 size, VagonGrid targetGrid)
    {
        if (size.x > targetGrid.actualRows) size.x = targetGrid.actualRows;
        if (size.y > targetGrid.parentVagon.SegmentsAmmount) size.y = targetGrid.parentVagon.SegmentsAmmount;

        GridCell[] targetedGridArea = null;
        Vagon targetVagon = targetGrid.parentVagon;

        float hitX = targetCoordinates.x;
        float hitY = targetCoordinates.y;

        int minX = Mathf.RoundToInt(hitX - ((size.x - 1.0f) / 2.0f));
        if (minX < 0 || minX + size.x > targetVagon.RowsAmmount)
        {
            ClearOldArea();
            return null; //if out of bounds return null
        }
        int minY = Mathf.RoundToInt((hitY / (360.0f / targetVagon.SegmentsAmmount)) - ((size.y - 1.0f) / 2.0f));

        int index = 0;

        targetedGridArea = new GridCell[(int)(size.x * size.y)];

        for (int x = minX; x < minX + size.x; x++)
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


    void BuildRequest(GridCell[] targetGridArea, Vector3 buildingCenter, float angle)
    {
        foreach (GridCell gridCell in targetGridArea)
        {
            gridCell.IsOccupied = true;
            gridCell.material.SetInt("isOccupied", 1);
        }

        GameObject targetBuilding = Instantiate(currentTargetBuilding);
        targetBuilding.transform.position = buildingCenter;
        currentTargetBuildingScript.angle = angle;
    }

}

