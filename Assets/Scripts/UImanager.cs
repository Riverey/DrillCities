using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UImanager : MonoBehaviour
{
    public Image trainDiagramBody;
    public Image trainDiagramArrow;

    [System.Serializable]
    public class LogMessage
    {
        public string type;
        public string messageText;
        public Sprite icon;
    }

    public Sprite defaultIcon;
    public List<LogMessage> logMessages;

    // Update is called once per frame
    void Update()
    {
        TrainDiagram();
        //if (!BuildingSystem.isBuilding) BuildingRaycast();
    }

    void BuildingRaycast()
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit targetHitPoint, MouseOrbitImproved.distance + 10f, 10))
        {
            Debug.Log(targetHitPoint.collider.gameObject.name);
        }
        
    }

    void TrainDiagram()
    {
        float uiY = MouseOrbitImproved.Y;
        if (uiY < 0) uiY = 360 + uiY;
        trainDiagramArrow.rectTransform.rotation = Quaternion.Euler(0.0f, 0.0f, uiY - 90.0f);
    }
    /// <summary>
    /// This method is used to send a message to the UI Log
    /// </summary>
    /// <param name="messageText"></param>
    public void LogRequest(string messageType = null, string messageText = null)
    {
        LogMessage targetLogMessage = new LogMessage { icon = defaultIcon };

        if (messageType != null)
        {
            foreach (LogMessage logMessage in logMessages)
            {
                if (messageType == logMessage.type)
                {
                    targetLogMessage.icon = logMessage.icon;
                    targetLogMessage.messageText = logMessage.messageText;
                    break;
                } //if requested type matches the type of the message, get and assign the corresponding icon
            }
        } //defining the message type

        if (messageText != null) targetLogMessage.messageText = messageText;
    }
}
