using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UImanager : MonoBehaviour
{
    public Image trainDiagramBody;
    public Image trainDiagramArrow;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        TrainDiagram();
    }

    void TrainDiagram()
    {
        float uiY = MouseOrbitImproved.Y;
        if (uiY < 0) uiY = 360 + uiY;
        trainDiagramArrow.rectTransform.rotation = Quaternion.Euler(0.0f, 0.0f, uiY - 90.0f);
    }
    /// <summary>
    /// Call this to send a message to the log
    /// </summary>
    /// <param name="message"></param>
    public void LogRequest(string message)
    {

    }
}
