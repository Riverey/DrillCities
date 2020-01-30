using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridCell
{
    public VagonGrid parentGrid; //store parentgrid of the cell
    public Vector2 coordinates;
    public GameObject cellGizmo; //associated gizmo object
    public Vector3 cellCenter; //transform of the cell regarding to the Vagon body
    public Transform parent;
    public bool isOccupied;
    public GridBuilding building; //reference to the building that is built on this cel        
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
    public GridCell[,] grid; //2d array used to store grid cells
}

public enum GridType { main, edge, edgeCross }

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

    private GridType currentGridType = GridType.main;
    public GridType GetCurrentGridType() => currentGridType;
    public void SetCurrentGridType(GridType value) => currentGridType = value;    

    void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 150.0f) && hit.transform.gameObject.tag == "RaycastTarget") //9 is the Vagons layer
        {                
            CapsuleCollider colCollider = (CapsuleCollider)hit.collider;
            Vagon hitVagon = hit.transform.gameObject.GetComponent<Vagon>();

            Material hitGridMaterial = hitVagon.gridMaterial; //old highlight implementation with a shader

            VagonGrid targetGrid = null;

            for (int i = 0; i < hitVagon.Grids.Length; i++)
            {
                if (hitVagon.Grids[i].gridType == GridType.main)
                {
                    targetGrid = hitVagon.Grids[i];
                }
            }
                
            float maxX = hitVagon.RowsAmmount - 1;
            int localX = Mathf.RoundToInt(hit.point.x.Remap(hitVagon.transform.position.x - maxX / 2, hitVagon.transform.position.x + maxX / 2, 0, maxX));

            float maxY = 360.0f / hitVagon.SegmentsAmmount;
            Vector3 hitVector = new Vector3(0.0f, hitVagon.transform.position.y + 10.0f, hitVagon.transform.position.z);

            float hitY = Vector3.Angle(hitVector, new Vector3(0.0f, hit.point.y, hit.point.z));
            Debug.Log(hitY);
            if (hit.point.z > 0) { hitY = 360.0f - hitY; }
            int localY = (int)hitY.Remap(0.0f, 360.0f - maxY * 2, 0.0f, hitVagon.SegmentsAmmount - 1);

            Debug.DrawLine(hitVagon.transform.position, hitVector, Color.red);
            Debug.DrawLine(hitVagon.transform.position, new Vector3(hitVagon.transform.position.x, hit.point.y, hit.point.z), Color.blue);

            Debug.Log(hit.point + "   " + hitY + "  " + localX + " " + localY);

            hitGridMaterial.SetFloat("_GridOpacity", Mathf.Lerp(hitGridMaterial.GetFloat("_GridOpacity"), 1.0f, 0.5f));
            hitGridMaterial.SetVector("_shadowPosition", new Vector4(-localY, -localX, 0.0f, 0.0f));

            if (Input.GetKeyDown("1") && targetGrid!= null)
            {
                if (targetGrid.grid[localX, localY] != null && !targetGrid.grid[localX, localY].isOccupied)
                { 
                    //add a system to detect neighbors, should work across the zero line
                    GameObject building = Instantiate(tempBuilding[0], targetGrid.grid[localX, localY].parent);
                    building.transform.localPosition = targetGrid.grid[localX, localY].cellCenter;
                }
            }
            else if (Input.GetKeyDown("2") && targetGrid != null)
            {
                if (targetGrid.grid[localX, localY] != null && !targetGrid.grid[localX, localY].isOccupied)
                {
                    //add a system to detect neighbors, should work across the zero line
                    GameObject building = Instantiate(tempBuilding[1], targetGrid.grid[localX, localY].parent);
                    building.transform.localPosition = targetGrid.grid[localX, localY].cellCenter;
                }
            }
            else if (Input.GetKeyDown("3") && targetGrid != null)
            {
                if (targetGrid.grid[localX, localY] != null && !targetGrid.grid[localX, localY].isOccupied)
                {
                    //add a system to detect neighbors, should work across the zero line
                    GameObject building = Instantiate(tempBuilding[2], targetGrid.grid[localX, localY].parent);
                    building.transform.localPosition = targetGrid.grid[localX, localY].cellCenter;
                }
            }
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
                    if (isBuilding) { }
                    else { }
                    
                }
            }
        } //switch on vagon grids if it's the same as the current grid target
    }

    GridCell BuildRaycast()
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, MouseOrbitImproved.distance + 10f, 9) && hit.transform.gameObject.tag == "RaycastTarget") //9 is the Vagons layer
        {

        }

        return null;
    }


}

