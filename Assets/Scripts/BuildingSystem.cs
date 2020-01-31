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
    public bool isOccupied;
    public GridBuilding building; //reference to the building that is built on this cel    l
    public Material material;
} //a class to contain infromation about cells

/// <summary>
/// Class to hold information about an entire grid
/// </summary>
[System.Serializable]
public class VagonGrid
{
    private Vagon parentVagon; //store a link to the parent vagon script
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
    public GameObject[] tempBuilding;
    public static List<Vagon> allVagons = new List<Vagon>();

    [System.Serializable] public class BuildingType { public BuildingSO[] buildings; }
    public List<BuildingType> buildings;

    public bool isBuilding = false;
    public BuildingSO currentBuildingSO; //use to store the currently selected building

    enum CurrentBuildState { none, building, demolishing } //current state of the Building System

    private CurrentBuildState CurrentBuildState1 { get; set; } = CurrentBuildState.none;

    private GridType currentGridType = GridType.road;
    public GridType GetCurrentGridType() => currentGridType;
    public void SetCurrentGridType(GridType value) => currentGridType = value;

    private GridCell targetGridCell;
    private GridCell oldTargetCell;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            GetTargetGridCell();
        }
        if (Input.GetKeyDown("1"))
        {
            currentGridType = GridType.main;
        }
        else if (Input.GetKeyDown("2"))
        {
            currentGridType = GridType.road;
        }
        else if (Input.GetKeyDown("3"))
        {
            currentGridType = GridType.cross;
        }
    }

    void BuildStateSwitch()
    {
        isBuilding = !isBuilding;
        foreach (Vagon vagon in allVagons)
        {
            foreach (VagonGrid grid in vagon.Grids)
            {
                if (grid.gridType == currentGridType)
                {
                    if (isBuilding) { grid.gridHolder.SetActive(true); }
                    else { grid.gridHolder.SetActive(false); }
                }
            }
        } //switch on vagon grids if it's the same as the current grid target
    }

    void BuildingUpdate()
    {

    }

    private void GetTargetGridCell()
    {

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit targetHitPoint, MouseOrbitImproved.distance + 10f) && targetHitPoint.transform.gameObject.tag == "RaycastTarget") //9 is the Vagons layer
        {
            CapsuleCollider colCollider = (CapsuleCollider)targetHitPoint.collider;
            Vagon targetVagon = targetHitPoint.transform.gameObject.GetComponent<Vagon>();
            
            VagonGrid targetGrid = null; //object to store the vagon that we're pointing at

            Vector3 localHitPoint = targetVagon.GetLocalCoordinates(targetHitPoint.point); //storing local coordinates of the hit point
            Vector3 vagonHit = new Vector3(0.0f, localHitPoint.y, localHitPoint.z);

            foreach (VagonGrid grid in targetVagon.Grids)
            {
                if (grid.gridType == currentGridType) targetGrid = grid;
            }
            int gridCellX = 0;
            switch (currentGridType)
            {
                case GridType.main:
                    gridCellX = Mathf.RoundToInt(Mathf.Clamp(localHitPoint.x.Remap(-targetVagon.Length / 2.0f + 0.5f, targetVagon.Length / 2.0f - 0.5f, 0.0f, targetGrid.actualRows - 1), 0.0f, targetGrid.actualRows - 1)); //getting an X coordinate of the cell by remaping the position of the hit to local coordinates of the vagon
                    break;
                case GridType.road:
                    gridCellX = Mathf.RoundToInt(Mathf.Clamp(localHitPoint.x.Remap(-targetVagon.Length / 2.0f, targetVagon.Length / 2.0f, 0.0f, targetGrid.actualRows - 1), 0.0f, targetGrid.actualRows - 1)); //getting an X coordinate of the cell by remaping the position of the hit to local coordinates of the vagon
                    break;
                case GridType.cross:
                    gridCellX = Mathf.RoundToInt(Mathf.Clamp(localHitPoint.x.Remap(-targetVagon.Length / 2.0f, targetVagon.Length / 2.0f, 0.0f, targetGrid.actualRows - 1), 0.0f, targetGrid.actualRows - 1)); //getting an X coordinate of the cell by remaping the position of the hit to local coordinates of the vagon
                    break;
            }
            
            float gridCellYAngle = localHitPoint.z <= 0.0f ? Vector3.Angle(targetVagon.transform.up * 100.0f, vagonHit) : 360.0f - Vector3.Angle(targetVagon.transform.up * 100.0f, vagonHit); //for anges greater than 180
            
            float segmentAngle = 180.0f / targetVagon.SegmentsAmmount; //half of the segment angle

            Debug.Log(localHitPoint + " " + targetHitPoint.point + " Hit angle: " + gridCellYAngle);

            for (int i = 0; i < targetVagon.SegmentsAmmount; i++)
            {
                GridCell tempGridCell = targetGrid.grid[gridCellX, i];

                if (gridCellYAngle - tempGridCell.angle <= segmentAngle)
                {
                    if (tempGridCell != targetGridCell || targetGridCell == null)
                    {
                        if (targetGridCell != null && targetGridCell.material != null)
                        {
                            targetGridCell.material.SetInt("isHovered", 0);
                        }

                        targetGridCell = tempGridCell;

                        if (targetGridCell.material != null)
                        {
                            targetGridCell.material.SetInt("isHovered", 1);
                        }
                        
                    }

                    break;
                }
            }
            

            Debug.DrawRay(targetVagon.transform.position, targetVagon.transform.up * 100.0f);
            Debug.DrawRay(targetVagon.transform.position, vagonHit, Color.red);

        }

        //return targetGridCell;
    }


}

