using System.Collections.Generic;
using UnityEngine;

public class Building : GridBuilding
{
    [Header("Building")]
    public Vector2 size = new Vector2(0.0f, 0.0f);

    public List<BuildingModule> initialBuildingModulesPool; //use this list to store building modules that has to be spawned initially
    public int initialBuildingModulesAmmount;

    private void Start()
    {
        SpawnInitialModules();
    }

    void SpawnInitialModules()
    {
        while (spawnedBuildingModules.Count < initialBuildingModulesAmmount)
        {
            BuildingModule randomModule = initialBuildingModulesPool[(Random.Range(0, initialBuildingModulesPool.Count))];
            if (randomModule != null && !randomModule.isOccupied) randomModule.SpawnModule();
        } //while there are less modules then desired ammount, keep trying to spawn at a random Building Point
        
    }
}
