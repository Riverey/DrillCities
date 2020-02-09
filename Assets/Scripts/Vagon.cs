using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class Vagon : MonoBehaviour
{
    [HideInInspector]
    public GameObject vagonGridsHolder;

    public Transform centerPivot;

    public CapsuleCollider vagonCollider;

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

        foreach (VagonGrid vagonGrid in grids)
        {
            vagonGrid.parentVagon = this;

            foreach (GridCell cell in vagonGrid.grid)
            {
                cell.material = cell.cellGizmo.GetComponentInChildren<MeshRenderer>().material;
            }
        }        
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
                    parent = grid.gridHolder.transform,
                    IsOccupied = false
                }; //creating the Grid Cell and storing it's parameters

                GridCell currentGridCell = grid.grid[i, j];

                GameObject cellGizmo = Instantiate(grid.cellObject, grid.gridHolder.transform); //storing the debug grid cell we are working on right now
                currentGridCell.cellGizmo = cellGizmo; //assigning the gizmo object to it's cell object

                cellGizmo.transform.localPosition = cellCenterTemp;

                if (grid.gridType == GridType.road) cellGizmo.transform.localRotation = i % 2 == 0 ? Quaternion.Euler(new Vector3(0, 90, -angle * 180 / Mathf.PI)) : Quaternion.Euler(new Vector3(-angle * 180 / Mathf.PI, 0, 0)); //spawning roads with different rotation depending on the row
                else cellGizmo.transform.localRotation = Quaternion.Euler(new Vector3(-angle * 180 / Mathf.PI, 0, 0)); //rotating buildings

                cellGizmo.name = grid.gridName + " [" + i + "][" + j + "]";                
            }
        } //spawning cells and storing them
    }

    public void EraseGrid(VagonGrid grid)
    {
        if (grid.gridHolder != null) DestroyImmediate(grid.gridHolder);
        grid.grid = null; //dumping the previous array
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