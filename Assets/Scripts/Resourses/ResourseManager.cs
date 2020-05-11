using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourseManager : MonoBehaviour
{
    public List<ResourseGenerator> resourseGenerators;
    public List<ResourseConsuptor> resourseConsuptors;

    public int startEnergy;
    public int startFood;
    public int startScience;

    public static int totalEnergy = 0;
    public static int totalFood = 0;
    public static int totalScience = 0;

    public float timeBetweenSteps = 10.0f;
    private float timeSinceLastStep = 0;

    [HideInInspector]
    public int totalEnergyChange;
    [HideInInspector]
    public int totalFoodChange;
    [HideInInspector]
    public int totalScienceChange;

    private void Start()
    {
        totalEnergy = startEnergy;
        totalFood = startFood;
        totalScience = startScience;
    }

    // Update is called once per frame
    void Update()
    {
        int tempEnergyChange = 0;
        int tempFoodChange = 0;
        int tempScienceChange = 0;

        foreach (ResourseGenerator resourseGenerator in resourseGenerators)
        {
            tempEnergyChange += resourseGenerator.energyProduction;
            tempFoodChange += resourseGenerator.foodProduction;
            tempScienceChange += resourseGenerator.scienceProduction;
        }

        foreach (ResourseConsuptor resourseConsumptor in resourseConsuptors)
        {
            tempEnergyChange -= resourseConsumptor.energyConsuption;
            tempFoodChange -= resourseConsumptor.foodConsumption;
            totalScienceChange += resourseConsumptor.scienceConsumption;
        }

        if (tempEnergyChange != totalEnergyChange) totalEnergyChange= tempEnergyChange;
        if (tempFoodChange != totalFoodChange) totalFoodChange = tempFoodChange;
        if (tempScienceChange != totalScienceChange) totalScienceChange = tempScienceChange;



        if (timeSinceLastStep > timeBetweenSteps)
        {
            totalEnergy += totalEnergyChange;
            totalFood += totalFoodChange;
            totalScience += totalScienceChange;

            timeSinceLastStep = 0.0f;
        }
        else
        {
            timeSinceLastStep += Time.deltaTime;
        }
    }

    public static void purchase(int cost)
    {
        totalEnergy -= cost;
    }
}
