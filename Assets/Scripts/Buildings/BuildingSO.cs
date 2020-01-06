using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BuildingSO : ScriptableObject
{
    [Tooltip("Building size")]
    public Vector2 objectSize;
}

[CreateAssetMenu(fileName = "NewBuilding", menuName = "Building")]
public class Building : BuildingSO
{

}

[CreateAssetMenu(fileName = "NewBuilding", menuName = "Road")]
public class Road : BuildingSO
{

}