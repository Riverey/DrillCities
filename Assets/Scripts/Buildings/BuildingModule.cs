using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
/// <summary>
/// Class used to spawn and manage modules of a building
/// </summary>
public class BuildingModule : MonoBehaviour
{   
    [SerializeField]
    public List<BuildingModuleVariant> variants;

    [System.Serializable]
    public class BuildingModuleVariant
    {
        public string name;
        public GameObject objectModel;

        [Range(0, 100)]
        public int spawnChanceModifier = 50;
    }

    public bool respondToEthicsChange = false; //used to determine if this module should be included in ethics recalculation

    public GameObject currentModule;

    private void Awake()
    {
        
    }
}


