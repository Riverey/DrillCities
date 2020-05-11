using UnityEngine;

public class ResourseGenerator : MonoBehaviour
{
    public int energyProduction;
    public int foodProduction;
    public int scienceProduction;

    private ResourseManager resourseManager;

    private void Start()
    {
        resourseManager = FindObjectOfType<ResourseManager>();
        resourseManager.resourseGenerators.Add(this);
    }
}
