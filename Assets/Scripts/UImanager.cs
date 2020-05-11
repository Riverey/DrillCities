using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UImanager : MonoBehaviour
{
    public AudioSource successResearch;

    public Image trainDiagramBody;
    public Image trainDiagramArrow;

    public Image trainSide;
    public Image trainSideFrame;
    public Vector2 sideConstrains = new Vector2(-53, 53);
    public float zoomFactor = 3.0f;

    [System.Serializable]
    public class LogMessage
    {
        public string type;
        public string messageText;
        public Sprite icon;
    }

    public Sprite defaultIcon;
    public List<LogMessage> logMessages;

    public GameObject buildingInfoPanel;
    public Text buildingName;
    public Text buildingDescription;

    public GameObject buildingEnergyAmmount;
    public Text buildingEnergyAmmountText;

    public GameObject buildingFoodAmmount;
    public Text buildingFoodAmmountText;

    public GameObject buildingScienceAmmount;
    public Text buildingScienceAmmountText;

    public Text allEnergyCount;
    public Text allFoodCount;
    public Text allScienceCount;

    public Text totalEnergyChange;
    public Text totalFoodChange;
    public Text totalScienceChange;

    bool showingBuildingInfo = false;

    public ResourseManager resourseManager;

    private bool raycastOn = true;

    [SerializeField]
    List<ResearchNode> researchTree;
    [System.Serializable]
    public class ResearchNode //temporary implementation of research system 
    {
        public Button button;
        public Text buttonNumber;
        public int cost = 0;
        public Button researchButton;
    }

    public void UnlockRequest(Button button)
    {
        foreach (ResearchNode node in researchTree)
        {
            if (node.button == button) 
            { 
                button.gameObject.SetActive(false);
                node.researchButton.interactable = true;
                ResourseManager.totalScience -= node.cost;
            } 
        }
    }

    private void Start()
    {
        resourseManager = FindObjectOfType<ResourseManager>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (ResearchNode researchNode in researchTree)
        {
            if (researchNode.button.gameObject.activeSelf)
            {
                if (researchNode.researchButton.gameObject.activeSelf) researchNode.researchButton.interactable = false;
                researchNode.buttonNumber.text = researchNode.cost.ToString();
                if (ResourseManager.totalScience >= researchNode.cost) researchNode.button.interactable = true;
                else { researchNode.button.interactable = false; }
            }
        }

        TrainDiagram();
        TrainSideDiagram();

        if (!BuildingSystem.isBuilding && raycastOn) BuildingRaycast();

        allEnergyCount.text = ResourseManager.totalEnergy.ToString();
        allFoodCount.text = ResourseManager.totalFood.ToString();
        allScienceCount.text = ResourseManager.totalScience.ToString();

        totalEnergyChange.text = resourseManager.totalEnergyChange.ToString();
        totalFoodChange.text = resourseManager.totalFoodChange.ToString();
        totalScienceChange.text = resourseManager.totalScienceChange.ToString();

        if (!raycastOn && buildingInfoPanel.activeSelf)
        {
            if(Input.GetKeyDown("escape"))
            {
                buildingInfoPanel.SetActive(false);
                raycastOn = true;
            }
        }
    }

    void TrainSideDiagram()
    {
        float zoom = MouseOrbitImproved.distance;
        zoom.Remap(MouseOrbitImproved.distanceConstrains.x, MouseOrbitImproved.distanceConstrains.y, 0.0f, zoomFactor);


        float x = MouseOrbitImproved.HorizontalMove;
        x.Remap(MouseOrbitImproved.SideBorders.x, MouseOrbitImproved.SideBorders.y, sideConstrains.x, sideConstrains.y);

        if (x < sideConstrains.x) x = sideConstrains.x;
        if (x > sideConstrains.y) x = sideConstrains.y;

        trainSideFrame.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, 0);
        trainSideFrame.GetComponent<RectTransform>().sizeDelta = new Vector2(zoom, zoom / Screen.width * Screen.height);


        if (MouseOrbitImproved.IsFlipped) trainSide.GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 0);
        else trainSide.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 0);
    }

    public void hideBuildingPanel()
    {
        buildingInfoPanel.SetActive(false);
        buildingEnergyAmmount.SetActive(false);
        buildingFoodAmmount.SetActive(false);
        buildingScienceAmmount.SetActive(false);
        raycastOn = true;
    }

    void BuildingRaycast()
    {
        int layerMask = 1 << 10;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit targetHitPoint, MouseOrbitImproved.distance + 10f, layerMask) && Input.GetMouseButtonDown(0))
        {
            showingBuildingInfo = true;
            ShowAndUpdateBuildingPanel(targetHitPoint.collider.GetComponent<GridBuilding>());
        }
    }

    void ShowAndUpdateBuildingPanel(GridBuilding building)
    {
        buildingInfoPanel.SetActive(true);
        buildingName.text = building.name;
        buildingDescription.text = ((Building)building).description;

        ResourseConsuptor tempResourseConsuptor = building.gameObject.GetComponent<ResourseConsuptor>();
        ResourseGenerator tempResourseGenerator = building.gameObject.GetComponent<ResourseGenerator>();

        if (tempResourseConsuptor != null)
        {
            if (tempResourseConsuptor.energyConsuption != 0)
            {
                buildingEnergyAmmount.SetActive(true);
                buildingEnergyAmmountText.text = "-" + tempResourseConsuptor.energyConsuption.ToString();
            }
            if (tempResourseConsuptor.foodConsumption != 0)
            {
                buildingFoodAmmount.SetActive(true);
                buildingFoodAmmountText.text = "-" + tempResourseConsuptor.foodConsumption.ToString();
            }
        }

        if (tempResourseGenerator != null)
        {
            if (tempResourseGenerator != null && tempResourseGenerator.energyProduction != 0)
            {
                buildingEnergyAmmount.SetActive(true);
                buildingEnergyAmmountText.text = tempResourseGenerator.energyProduction.ToString();
            }
            if (tempResourseGenerator != null && tempResourseGenerator.foodProduction != 0)
            {
                buildingFoodAmmount.SetActive(true);
                buildingFoodAmmountText.text = tempResourseGenerator.foodProduction.ToString();
            }
            if (tempResourseGenerator != null && tempResourseGenerator.scienceProduction != 0)
            {
                buildingScienceAmmount.SetActive(true);
                buildingScienceAmmountText.text = tempResourseGenerator.scienceProduction.ToString();
            }
        }

        
        raycastOn = false;
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
