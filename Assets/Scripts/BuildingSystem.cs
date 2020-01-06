using System.Collections.Generic;
using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    public readonly List<Vagon> vagons;

    public GameObject[] tempBuilding;

    public enum GridType { main, edge, edgeCross }

    [System.Serializable] public class GridCell { public GridType gridType; public GameObject cellDebugGizmo; public Vector3 cellCenter; public Transform parent; public bool isOccupied; public Vector2 coordinates; public GridBuilding building; } //a class to contain infromation about cells
    [System.Serializable] public class DebugGridRow { public GridCell[] debugCellsArray; }

    /// <summary>
    /// Class to hold information about an entire grid
    /// </summary>
    [System.Serializable] public class VagonGrid { public string gridName; public GridType gridType; public bool isEnabledForThisVagon = true;  public DebugGridRow[] debugGrid; public GameObject parentObject; public GameObject cellObject; public GridCell[,] grid; }

    public static List<Vagon> allVagons = new List<Vagon>();

    [System.Serializable] public class BuildingType { public BuildingSO[] buildings; }
    public List<BuildingType> buildings;

    enum CurrentMouseState { none, building, demolishing }
    CurrentMouseState currentMouseState = CurrentMouseState.none;
    
    void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 150.0f) && hit.transform.gameObject.tag == "RaycastTarget") //9 is the Vagons layer
        {                
            CapsuleCollider colCollider = (CapsuleCollider)hit.collider;
            Vagon hitVagon = hit.transform.gameObject.GetComponent<Vagon>();
            Material hitGridMaterial = hitVagon.gridMaterial;

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

}

