using System.Collections.Generic;
using UnityEngine;

public class Building : GridBuilding
{
    [Header("Building")]
    public Vector2 size = new Vector2(0.0f, 0.0f);

    [SerializeField]
    public List<BuildingModule> modules;

    [System.Serializable]
    public class BuildingModule
    {
        public string name;

        [Range(0, 100)]
        public int baseSpawnProbability = 0;

        [SerializeField]
        public List<BuildingModule> subModules;
    }
}
