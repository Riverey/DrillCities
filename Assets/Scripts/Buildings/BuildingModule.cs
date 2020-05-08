using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
/// <summary>
/// Class used to spawn and manage modules of a building
/// </summary>
public class BuildingModule : MonoBehaviour
{   
    [System.Serializable]
    public class BuildingModuleVariant
    {
        public string name;
        public GameObject objectModel;

        [Range(0, 5)]
        public int spawnChanceModifier = 50;
    }
    [SerializeField]
    public List<BuildingModuleVariant> variants;

    public bool respondToEthicsChange = false; //used to determine if this module should be included in ethics recalculation
    public bool affectedByAngle = false;

    private Building parentBuilding;
    public GameObject currentModule;

    private void Start()
    {
        parentBuilding = GetComponentInParent<Building>();
        if (parentBuilding == null) parentBuilding = GetComponentInParent<BuildingModule>().parentBuilding; //in case no Building found look for a parent module and get it's parent Building

        //SpawnModule();
    }

    private void SpawnModule()
    {
        List<GameObject> variantsRandomizer = new List<GameObject>();
        for (int i = 0; i < variants.Count; i++)
        {
            for (int j = 0; j < variants[i].spawnChanceModifier; j++)
            {
                variantsRandomizer[i + j] = variants[i].objectModel;
            }
        }
        GameObject targetBuildingModule = Instantiate(variantsRandomizer[(int)Random.Range(0.0f, variantsRandomizer.Count)]);
    }
}


