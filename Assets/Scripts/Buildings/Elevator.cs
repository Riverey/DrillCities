using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : GridBuilding
{
    public List<Road> roads;

    private GameObject elevatorCabin;

    public Vector2 coordinates;

    private void Start()
    {
        Debug.Log("Little baby elevator was born!");
        if (elevatorCabin == null)
        {
            if (roads != null && roads.Count > 0)
            {
                elevatorCabin = Instantiate(roads[0].ParentGrid.parentVagon.elevatorCabin);

                ElevatorCabin cabin = elevatorCabin.GetComponent<ElevatorCabin>();
                elevatorCabin.transform.parent = gameObject.transform;
                elevatorCabin.transform.localPosition = Vector3.zero;
                elevatorCabin.transform.localRotation = Quaternion.identity;

                cabin = elevatorCabin.GetComponent<ElevatorCabin>();
                cabin.parentElevator = this;
            }
        }
    }

    public void RequestDestroy(Road targetRoad = null)
    {
        if (targetRoad != null && roads.Contains(targetRoad)) roads.Remove(targetRoad);

        List<Road> tempListRoads = roads; //store all roads in a temporary array

        foreach (Road road in roads) road.elevator = null; //for each road clear it's elevator

        roads = new List<Road>(); //clear main roads array

        foreach (Road road in tempListRoads) road.RecalculateElevators(); 

        if (elevatorCabin != null) Destroy(elevatorCabin);
    }
}
