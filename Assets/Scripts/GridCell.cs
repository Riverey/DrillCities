using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GridCell : MonoBehaviour
{

    public Vector2 coordinates;
    public Vagon vagon;
    public BuildingSystem.VagonGrid grid;

    public GridCell[] neighbors;

    [SerializeField]
    public TextMeshPro cellName;

    public void FindNeighbors()
    {
        /*switch (cellType)
        {
            case Vagon.GridType.building:
                neighbors = new GridCell[4];

                int upIndex = (int)coordinates.y == 0 ? vagon.SegmentsAmmount - 1 : (int)coordinates.y - 1;
                neighbors[0] = grid.array[(int)(coordinates.x)].cellsArray[upIndex].gridCell;

                int downIndex = (int)coordinates.y == vagon.SegmentsAmmount - 1 ? 0 : (int)coordinates.y + 1;
                neighbors[1] = grid.array[(int)(coordinates.x)].cellsArray[downIndex].gridCell;

                if ((int)coordinates.x >= 1) neighbors[2] = grid.array[(int)coordinates.x - 1].cellsArray[(int)coordinates.y].gridCell;
                if ((int)coordinates.x <= vagon.RowsAmmount - 2) neighbors[3] = grid.array[(int)coordinates.x + 1].cellsArray[(int)coordinates.y].gridCell;
                break;
            case Vagon.GridType.road:

                break;
            case Vagon.GridType.cross:

                break;
        }*/
    }
}
