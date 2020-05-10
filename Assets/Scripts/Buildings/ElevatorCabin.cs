using System.Security.Cryptography;
using UnityEngine;

public class ElevatorCabin : MonoBehaviour
{
    private Vector2 target;

    public Elevator parentElevator;

    private Vagon targetVagon;
    private VagonGrid targetGrid;

    private Vector2 currentCoordinates;

    public float speed = 0.5f;

    private Vector2 startPosition;
    private float desiredTime;
    private float timeLerped = 0.0f;

    private void Start()
    {
        targetGrid = parentElevator.ParentGrid;
        targetVagon = targetGrid.parentVagon;
        currentCoordinates = parentElevator.roads[0].ParentCells[0].coordinates;
    }

    private void Update()
    {
        if (timeLerped < desiredTime)
        {
            timeLerped += Time.deltaTime;

            currentCoordinates.x = Mathf.Lerp(startPosition.x, target.x, timeLerped / desiredTime);
            currentCoordinates.y = Mathf.Lerp(startPosition.y, target.y, timeLerped / desiredTime);

            float angle = (currentCoordinates.y * Mathf.PI * 2 / targetVagon.SegmentsAmmount); //angle in radians

            Vector3 desiredLocation = ConvertCoordinates(currentCoordinates.x, currentCoordinates.y, angle);
            Quaternion desiredRotation = RotateAround(currentCoordinates.x, currentCoordinates.y, angle);

            transform.position = targetVagon.GetWorldCoordinates(desiredLocation);
            transform.rotation = desiredRotation;
        }
        else
        {
            if (currentCoordinates != null) startPosition = currentCoordinates;
            else { currentCoordinates = parentElevator.coordinates; startPosition = currentCoordinates; }

            target = parentElevator.roads[Random.Range(0, parentElevator.roads.Count)].ParentCells[0].coordinates;

            desiredTime = Mathf.Abs((startPosition.x - target.x) + (startPosition.y - target.y)) / speed;
            timeLerped = 0.0f;
        }
    }

    Vector3 ConvertCoordinates(float x, float y, float angle) //this could be converted into a methode inside the Vagon - #todo
    {
        float yOffset = x;
        int gridOffset = x % 2 == 0 ? 0 : 1; //each second row will be offset using this int
        angle += gridOffset * (Mathf.PI / targetVagon.SegmentsAmmount);
        yOffset = yOffset.Remap(0.0f, targetGrid.actualRows - 1, -targetVagon.Length / 2, targetVagon.Length / 2);

        Vector3 cellCenterTemp = new Vector3(yOffset, Mathf.Cos(angle) * targetVagon.Radius, -Mathf.Sin(angle) * targetVagon.Radius); //creating a vector for the grid cell coordinates

        return cellCenterTemp;
    }

    Quaternion RotateAround (float x, float y, float angle)
    {
        Quaternion newAngles = x % 2 == 0 ? Quaternion.Euler(new Vector3(0, 90, -angle * 180 / Mathf.PI)) : Quaternion.Euler(new Vector3(-angle * 180 / Mathf.PI, 0, 0));
        return newAngles;
    }
}
