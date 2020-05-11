using UnityEngine;

public class ResourseConsuptor : MonoBehaviour
{
    public int energyConsuption;
    public int foodConsumption;
    public int scienceConsumption;

    private ResourseManager resourseManager;

    private void Start()
    {
        resourseManager = FindObjectOfType<ResourseManager>();
        resourseManager.resourseConsuptors.Add(this);
    }
}
