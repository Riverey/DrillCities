using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
/// <summary>
/// Class used to spawn and manage modules of a building
/// </summary>
public class BuildingModule : MonoBehaviour
{
    [Tooltip("Set false if thisis the first layer of Modules for the buildings")]
    public bool autoSpawn = true;
    [System.Serializable]
    public class BuildingModuleVariant
    {
        public string name;
        public GameObject objectModel;
        [Range(0,180)]
        public int randomizeRotation = 0;

        [Range(1, 5)]
        public int spawnChanceModifier = 50;
    }
    
    [Tooltip("If on, this module will be a subject for re-spawn if ethics change - not implemented yet")]
    public bool respondToEthicsChange = false; //used to determine if this module should be included in ethics recalculation

    [Tooltip("If off, module rotation will not be affected by the position on the cylinder")]
    public bool affectedByAngle = false;

    [Tooltip("If not zero, this module has a chance to not spawn a variant")]
    [Range(0,5)]
    public int emptySpawnChance = 0;

    [SerializeField]
    public List<BuildingModuleVariant> variants;

    private Building parentBuilding;
    [HideInInspector]
    public bool isOccupied = false;

    private void Start()
    {
        LinkParentBuilding();

        if (autoSpawn) SpawnModule();
    }

    private void LinkParentBuilding()
    {
        if (parentBuilding == null)
        {
            parentBuilding = GetComponentInParent<Building>();
            if (parentBuilding == null) parentBuilding = GetComponentInParent<BuildingModule>().parentBuilding; //in case no Building found look for a parent module and get it's parent Building
        }
        if (parentBuilding != null && parentBuilding.spawnedBuildingModules.Contains(this) == false) { parentBuilding.spawnedBuildingModules.Add(this); }
    }

    public void SpawnModule()
    {
        if (!isOccupied)
        {
            if (parentBuilding == null) LinkParentBuilding();

            isOccupied = true;
            List<GameObject> variantsRandomizer = new List<GameObject>();
            for (int i = 0; i < variants.Count; i++)
            {
                for (int j = 0; j < variants[i].spawnChanceModifier; j++)
                {
                    variantsRandomizer.Add(variants[i].objectModel);
                }
            } //creating a new list with all variants multiplied by their spawn chance
            int randomIndex = Mathf.RoundToInt(Random.Range(0.0f, variantsRandomizer.Count - 1));
            GameObject targetBuildingModule = Instantiate(variantsRandomizer[randomIndex]); //spawning a variant

            if (affectedByAngle)
            {
                targetBuildingModule.transform.parent = parentBuilding.transform; //temporary storing inside the parent building to rotate correctly
            }
            else
            {
                targetBuildingModule.transform.parent = parentBuilding.ParentGrid.parentVagon.transform; //temporary storing inside the parent grid to rotate correctly
            }
            targetBuildingModule.transform.localRotation = Quaternion.identity;

            targetBuildingModule.transform.parent = gameObject.transform;
            targetBuildingModule.transform.localPosition = Vector3.zero;
        }
    }
}


