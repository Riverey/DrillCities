using UnityEngine;
using TMPro;

public class Vagon : MonoBehaviour
{
    public GameObject hitObject;
    private GameObject spawnerHitObject;

    public Material gridMaterial;

    private GameObject gridsHolder;

    public GameObject centerPivot;

    [SerializeField] private CapsuleCollider vagonCollider;

    [SerializeField]
    private int segmentsAmmount = 12; public int SegmentsAmmount { get => segmentsAmmount; set { if (segmentsAmmount != value) { segmentsAmmount = value; RecalculateVariables(); } } }
    [SerializeField]
    private int rowsAmmount = 6; public int RowsAmmount { get => rowsAmmount; set { if (rowsAmmount != value) { rowsAmmount = value; RecalculateVariables(); } } }
    private int cellProportion; public int CellProportion { get => cellProportion; set { if (cellProportion != value) { cellProportion = value; RecalculateVariables(); } } }

    private float radius; public float Radius { get => radius; set { if (radius != value) { radius = value; } } }
    private float length; public float Length { get => length; set { if (length != value) { length = value; } } }

    [SerializeField] private BuildingSystem.VagonGrid[] grids; public BuildingSystem.VagonGrid[] Grids { get => grids; set => grids = value; }

    private void Start()
    {
        RecalculateVariables();
        BuildingSystem.allVagons.Add(this);
    }

    /// <summary>
    /// Method to recalculate all Vagon variables, called when any one of them is changed
    /// </summary>
    private void RecalculateVariables()
    {
        Debug.Log("Recalculating");
        Radius = SegmentsAmmount / (2 * Mathf.PI); //making sure we always have an integer number of segments
        Length = RowsAmmount * (1.0f + cellProportion / 10.0f);
        vagonCollider.radius = radius;
        vagonCollider.height = length + (radius * 2.0f);

        gridMaterial.SetVector("_dimensions", new Vector4(segmentsAmmount, rowsAmmount, 0, 0 ));

        GridsReDrawRequest();
    }

    /// <summary>
    /// Method to draw grids
    /// </summary>
    private void GridsReDrawRequest()
    {
        if (gridsHolder == null)
        {
            gridsHolder = new GameObject("GridObjects Holder"); //if couldn't find an existing object, create a new one
            gridsHolder.transform.parent = gameObject.transform;
        }

        foreach (BuildingSystem.VagonGrid grid in Grids)
        {
            EraseGrid(grid); //erasing old grids
            DrawGrid(grid); //
        }
        centerPivot = gameObject; // temp solution, assigning central point as a center of the cilinder for now
    }

    /// <summary>
    /// Function to populate arrays with grid objects
    /// </summary>
    private void DrawGrid(BuildingSystem.VagonGrid grid)
    {
        int rows = 0;
        
        switch (grid.gridType) //calculating row ammounts for this type of grid
        {
            case BuildingSystem.GridType.main:
                rows = RowsAmmount;
                break;
            case BuildingSystem.GridType.edge:
                rows = RowsAmmount * 2 - 1;
                break;
            case BuildingSystem.GridType.edgeCross:
                rows = RowsAmmount - 1;
                break;
        }

        grid.grid = new BuildingSystem.GridCell[rows,SegmentsAmmount]; //creating a new 2d array to store all grid cells of this grid

        if (grid.parentObject == null)
        {
            grid.parentObject = new GameObject(grid.gridName + "GridObjects Holder");
            grid.parentObject.transform.parent = gridsHolder.transform;
        }

        for (int i = 0; i < rows; i++) 
        {
            for (int j = 0; j < SegmentsAmmount; j++)
            {

                float angle = (j * Mathf.PI * 2 / SegmentsAmmount); //angle in radians

                float yOffset = i;
                yOffset = yOffset.Remap(0.0f, rows - 1, -Length / 2 + (0.5f + cellProportion / 20), Length / 2 - (0.5f + cellProportion / 20));

                switch (grid.gridType) //calculating grid offsets for different types of grids
                {
                    case BuildingSystem.GridType.main:
                        break; //no need for offset for regular buildings
                    case BuildingSystem.GridType.edge:
                        int gridOffset = i % 2 == 0 ? 1 : 0;
                        angle += gridOffset * (Mathf.PI / SegmentsAmmount); //adding an offset for sub and cross grids;
                        break;
                    case BuildingSystem.GridType.edgeCross:
                        angle += (Mathf.PI / SegmentsAmmount);
                        yOffset = i;
                        yOffset = yOffset.Remap(0.0f, rows - 1, -Length / 2 + (1 + cellProportion / 20), Length / 2 - (1 + cellProportion / 20));
                        break;
                }

                Vector3 cellCenterTemp = new Vector3(yOffset / Radius, -Mathf.Sin(angle), -Mathf.Cos(angle)) * Radius;

                grid.grid[i, j] = new BuildingSystem.GridCell
                {
                    parentGrid = grid,
                    coordinates = new Vector2(i, j), //storing cell coordinates
                    cellCenter = cellCenterTemp,
                    parent = grid.parentObject.transform,
                    isOccupied = false
                };
                
                GameObject cellGizmo = Instantiate(grid.cellObject, grid.parentObject.transform); //storing the debug grid cell we are working on right now
                grid.grid[i, j].cellGizmo = cellGizmo; //assigning the gizmo object to it's cell object

                cellGizmo.transform.localPosition = cellCenterTemp;

                if (grid.gridType == BuildingSystem.GridType.edge) cellGizmo.transform.rotation = i % 2 == 0 ? Quaternion.Euler(new Vector3(-angle * 180 / Mathf.PI, 0, 0)) : Quaternion.Euler(new Vector3(0, 90, -angle * 180 / Mathf.PI)); //spawning roads with different rotation depending on the row
                else cellGizmo.transform.rotation = Quaternion.Euler(new Vector3(-angle * 180 / Mathf.PI + 90, 0, 0)); //rotating buildings

                cellGizmo.name = grid.gridName + " [" + i + "][" + j + "]";
                
            }
        }
    }

    public void EraseGrid(BuildingSystem.VagonGrid grid)
    {
       DestroyImmediate(grid.parentObject);
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
