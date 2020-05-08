using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourseManager : MonoBehaviour
{
    [System.Serializable]
    public class ResourseSystems
    {
        public string resourseName;
        public ResourseSystem resourseSystem;
    }
    [SerializeField]
    private ResourseSystems[] myResourseSystems;
    /// <summary>Readonly - use to access all Resourse Systems that were set at startup</summary>
    public ResourseSystems[] resourseSystems { get { return myResourseSystems; } }
    
    public static List<Building> allBuildings = new List<Building>();
}
