using UnityEngine;
using TMPro;

public class Vagon : MonoBehaviour
{
    public GameObject hitObject;
    private GameObject spawnerHitObject;

    public Material gridMaterial;

    private GameObject vagonGridsHolder;

    public GameObject centerPivot;

    [SerializeField] private CapsuleCollider vagonCollider;

    [SerializeField]
    private int segmentsAmmount = 12; public int SegmentsAmmount { get => segmentsAmmount; set { if (segmentsAmmount != value) { segmentsAmmount = value; RecalculateVariables(); } } }
    [SerializeField]
    private int rowsAmmount = 6; public int RowsAmmount { get => rowsAmmount; set { if (rowsAmmount != value) { rowsAmmount = value; RecalculateVariables(); } } }
    private int cellProportion; public int CellProportion { get => cellProportion; set { if (cellProportion != value) { cellProportion = value; RecalculateVariables(); } } }

    private float radius; public float Radius { get => radius; set { if (radius != value) { radius = value; } } }
    private float length; public float Length { get => length; set { if (length != value) { length = value; } } }

    [SerializeField] private VagonGrid[] grids; public VagonGrid[] Grids { get => grids; set => grids = value; }

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

        //gridMaterial.SetVector("_dimensions", new Vector4(segmentsAmmount, rowsAmmount, 0, 0 )); //legacy system to change the grid of the shader

        centerPivot = gameObject;

        foreach (VagonGrid grid in Grids) DrawGrid(grid); //redraw all the grids
    }

    /// <summary>
    /// Function to populate arrays with grid objects
    /// </summary>
    private void DrawGrid(VagonGrid grid)
    {
        EraseGrid(grid); //dumping the previous grid

        //checking if the hierarchy gameobjects exists and if not creates a new one to store all grid cells
        if (vagonGridsHolder == null) 
        {
            vagonGridsHolder = new GameObject("Vagon Grids");
            vagonGridsHolder.transform.parent = gameObject.transform;
        }

        if (grid.gridHolder == null)
        {
            grid.gridHolder = new GameObject(grid.gridName + " Grid");
            grid.gridHolder.transform.parent = vagonGridsHolder.transform;
        }

        int actualRows = 0;        
        switch (grid.gridType)
        {
            case GridType.main:
                actualRows = RowsAmmount;
                break;
            case GridType.edge:
                actualRows = RowsAmmount * 2 - 1;
                break;
            case GridType.edgeCross:
                actualRows = RowsAmmount - 1;
                break;
        } //calculating actual row ammounts for this type of grid

        grid.grid = new GridCell[actualRows,SegmentsAmmount]; //creating a new 2d array to store all grid cells of this grid        

        for (int i = 0; i < actualRows; i++) 
        {
            for (int j = 0; j < SegmentsAmmount; j++)
            {

                float angle = (j * Mathf.PI * 2 / SegmentsAmmount); //angle in radians

                float yOffset = i;
                yOffset = yOffset.Remap(0.0f, actualRows - 1, -Length / 2 + (0.5f + cellProportion / 20), Length / 2 - (0.5f + cellProportion / 20)); //remap the offset from abstract value to actual transform value depending on the length of the vagon

                switch (grid.gridType) //calculating grid offsets for different types of grids
                {
                    case GridType.main:
                        break; //no need for offset for regular buildings
                    case GridType.edge:
                        int gridOffset = i % 2 == 0 ? 1 : 0; //each second row will be offset using this int
                        angle += gridOffset * (Mathf.PI / SegmentsAmmount);
                        break;
                    case GridType.edgeCross:
                        angle += (Mathf.PI / SegmentsAmmount);
                        yOffset = i;
                        yOffset = yOffset.Remap(0.0f, actualRows - 1, -Length / 2 + (1 + cellProportion / 20), Length / 2 - (1 + cellProportion / 20));
                        break;
                }

                Vector3 cellCenterTemp = new Vector3(yOffset, Mathf.Cos(angle) * Radius, -Mathf.Sin(angle) * Radius); //creating a vector for the grid cell coordinates

                grid.grid[i, j] = new GridCell
                {
                    parentGrid = grid,
                    coordinates = new Vector2(i, j), //storing cell coordinates
                    cellCenter = cellCenterTemp,
                    parent = grid.gridHolder.transform,
                    isOccupied = false
                };
                
                GameObject cellGizmo = Instantiate(grid.cellObject, grid.gridHolder.transform); //storing the debug grid cell we are working on right now
                grid.grid[i, j].cellGizmo = cellGizmo; //assigning the gizmo object to it's cell object

                cellGizmo.transform.localPosition = cellCenterTemp;

                if (grid.gridType == GridType.edge) cellGizmo.transform.rotation = i % 2 == 0 ? Quaternion.Euler(new Vector3(-angle * 180 / Mathf.PI, 0, 0)) : Quaternion.Euler(new Vector3(0, 90, -angle * 180 / Mathf.PI)); //spawning roads with different rotation depending on the row
                else cellGizmo.transform.rotation = Quaternion.Euler(new Vector3(-angle * 180 / Mathf.PI, 0, 0)); //rotating buildings

                cellGizmo.name = grid.gridName + " [" + i + "][" + j + "]";
                
            }
        } //spawning cells and storing them
    }

    public void EraseGrid(VagonGrid grid)
    {
        DestroyImmediate(grid.gridHolder);
        grid.grid = null; //dumping the previous array
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
