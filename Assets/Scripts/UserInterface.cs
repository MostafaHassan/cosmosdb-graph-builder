using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.ComponentModel;
using System.Linq;
using System;
using GraphsonToGremlin;
using System.Threading.Tasks;

public class UserInterface : MonoBehaviour
{
    public SceneCreator sceneCreator;

    public ArrowImporter arrowImporter;
    public ArrowExporter arrowExporter;

    public GameObject appBarBtn;
    private bool appBarOpen = false;

    public GameObject editNodePanel;
    public InputField editNodeCaptionText;
    public InputField editNodeProperties;
    public Dropdown selectNodeTypeDD;

    public GameObject editEdgePanel;
    public InputField editEdgeCaptionText;
    public InputField editEdgeProperties;
    public GameObject editEdgeArrowBottomViz;
    public GameObject editEdgeNodeUpperViz;
    public GameObject editEdgeNodeBottomViz;
    public Dropdown selectEdgeTypeDD;


    private Node currentEntity;
    private Connect currentEdge;
    // Used to go back to original values of user clicks cancel
    private GameObject originalCurrentEdge_ToObj;

    public GameObject codeText;
    public GameObject codePanel;

    public GameObject zoomButtonPlus;
    public GameObject zoomButtonMinus;

    public GameObject exportedDataPanel;
    public TMPro.TMP_InputField exportedData;

    public GameObject importedDataPanel;
    public TMPro.TMP_InputField importedData;

    public GameObject uploadToDBPanel;

    public GameObject msgToUserPanel;
    public Text msgToUserText;

    public ColorPicker colPick;

    // Start is called before the first frame update
    void Start()
    {
        codeText.GetComponent<RectTransform>().transform.position = codeText.transform.parent.GetChild(1).transform.position;

        GernerateNodeDropDownContent();
        GernerateEdgeDropDownContent();

        colPick.CurrentColor = Color.white;
    }

    public void ShowMessage(string msg, Color col)
    {
        msgToUserPanel.GetComponent<Animator>().Play("showmsg");
        msgToUserText.color = col;
        msgToUserText.text = msg;
    }

    void GernerateNodeDropDownContent()
    {
        string[] nodeTypeNames = Enum.GetNames(typeof(Node.NodeTypes));
        List<Dropdown.OptionData> dropdowns = new List<Dropdown.OptionData>();

        foreach (string d in nodeTypeNames)
        {
            Dropdown.OptionData dd = new Dropdown.OptionData(d.ToLower());
            dropdowns.Add(dd);
        }
        selectNodeTypeDD.AddOptions(dropdowns);
    }
    void GernerateEdgeDropDownContent()
    {
        string[] edgeTypeNames = Enum.GetNames(typeof(Connect.EdgeTypes));
        List<Dropdown.OptionData> dropdowns = new List<Dropdown.OptionData>();
        foreach (string d in edgeTypeNames)
        {
            Dropdown.OptionData dd = new Dropdown.OptionData(d.ToLower());
            dropdowns.Add(dd);
        }
        selectEdgeTypeDD.AddOptions(dropdowns);
    }

    void ToggleVisibilityUIElements()
    {
        ShowOrHideUIElements(!codePanel.gameObject.activeSelf);
    }

    void ShowOrHideUIElements(bool visibility)
    {
        codePanel.gameObject.SetActive(visibility);

        zoomButtonPlus.SetActive(visibility);
        zoomButtonMinus.SetActive(visibility);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H) && !Globals.inMenu)
        {
            ToggleVisibilityUIElements();
        }

        // Toggle show dialog bubbles
        if(Input.GetKeyDown(KeyCode.B) && !Globals.inMenu)
        {
            EntityManager em = Camera.main.GetComponent<EntityManager>();
            Globals.showDialogBubbles = !Globals.showDialogBubbles;

            if (Globals.showDialogBubbles)
            {
                foreach (GameObject e in em.entities)
                {
                    e.GetComponent<Node>().ShowDialogBubble();
                }
            }
            else
            {
                foreach (GameObject e in em.entities)
                {
                    e.GetComponent<Node>().HideDialogBubble();
                }
            }
        }
    }

    public void OpenEditNode(Node curEntity)
    {
        Globals.inMenu = true;
        editNodePanel.SetActive(true);
        currentEntity = curEntity;

        colPick.CurrentColor = curEntity.GetComponent<Node>().color;

        LoadNode();
    }

    public void SetCurrentEntity(Node curEntity)
    {
        currentEntity = curEntity;
        LoadNode();
    }

    public void SetCurrentEdge(Connect curEdge)
    {
        currentEdge = curEdge;
        LoadEdge();
    }

    public void OpenEditEdge(Connect curEdge)
    {
        Globals.inMenu = true;
        editEdgePanel.SetActive(true);
        currentEdge = curEdge;
        LoadEdge();

        originalCurrentEdge_ToObj = curEdge.to;
    }

    public void CancelButtonNode()
    {
        Globals.inMenu = false;
        currentEntity = null;
        editNodePanel.SetActive(false);
    }

    public void CancelButtonEdge()
    {
        Globals.inMenu = false;
        currentEntity = null;
        editNodePanel.SetActive(false);

        if(currentEdge.to != originalCurrentEdge_ToObj)
        {
            //print("The user has swapped the connection between nodes, revert back to original conenction!");
            ReverseLine();
        }
        

        // Reset connection
        editEdgePanel.SetActive(false);
        currentEdge = null;
        originalCurrentEdge_ToObj = null;
    }

    public void SaveNodeButton()
    {
        // Saving data
        currentEntity.label = editNodeCaptionText.text;
        DataUtility.UpdatePropertiesFromText(ref currentEntity.propertiesDict,
            editNodeProperties.text, ref currentEntity.GetPropertiesREF());

        //currentEntity.UpdatePropertiesFromText(editNodeProperties.text);

        currentEntity.SetVisualLabel();

        currentEntity.ShowDialogBubble();

        currentEntity.GetComponent<Node>().color = colPick.CurrentColor;

        UpdateGremlinCodeText(currentEntity);

        // Reset color picker
        colPick.CurrentColor = Color.white;

        // Set node icon
        currentEntity.SetNodeIcon();

        currentEntity = null;
        editNodePanel.SetActive(false);
        Globals.inMenu = false;
    }

    public void SaveEdgeButton()
    {
        // Saving data
        currentEdge.label = editEdgeCaptionText.text;
        DataUtility.UpdatePropertiesFromText(ref currentEdge.propertiesDict, editEdgeProperties.text, ref currentEdge.GetPropertiesREF());
        //currentEdge.properties = editEdgeProperties.text;

        //currentEdge.gameObject.transform.GetChild(0).GetChild(0).GetComponent<TextMesh>().text = currentEdge.label;

        Camera.main.GetComponent<EntityManager>().DisplayEdgeType(currentEdge.gameObject);

        UpdateGremlinCodeText(currentEdge);

        currentEdge = null;
        editEdgePanel.SetActive(false);
        Globals.inMenu = false;
    }

    public void DeleteNodeButton()
    {
        EntityManager em = Camera.main.GetComponent<EntityManager>();

        for (int i = 0; i < em.entities.Count; i++)
        {
            if(em.entities[i] == currentEntity.gameObject)
            {
                //print("Found entity to remove");
                em.entities[i].GetComponent<Node>().DeleteConnections();
                em.entities.RemoveAt(i);
                break;
            }
        }
        

        Destroy(currentEntity.gameObject);
        currentEntity = null;
        editNodePanel.SetActive(false);
        Globals.inMenu = false;
    }

    public void DeleteEdgeButton()
    {
        Camera.main.GetComponent<EntityManager>().DeleteEdge(currentEdge);

        currentEdge = null;
        editEdgePanel.SetActive(false);
        Globals.inMenu = false;
    }

    public void SelectNodeType()
    {
        if (selectNodeTypeDD.value == 0)
        {
            editNodeCaptionText.text = "";
        }   
        else
        {
            // Set label to same as type
            editNodeCaptionText.text = ((Node.NodeTypes)selectNodeTypeDD.value).ToString().ToLower();

            // Load template for properties
        }
    }

    public void SelectEdgeType()
    {
        if (selectEdgeTypeDD.value == 0)
        {
            editEdgeCaptionText.text = "";
        }
        else
        {
            // Set label to same as type
            editEdgeCaptionText.text = ((Connect.EdgeTypes)selectEdgeTypeDD.value).ToString().ToLower();

            // Load template for properties
        }
    }

    public void LoadNode()
    {
        try
        {
            editNodeCaptionText.text = currentEntity.label;
            editNodeProperties.text = DataUtility.GetPropertiesAsString(currentEntity.propertiesDict, ref currentEntity.GetPropertiesREF());

            // Setting dropdown to be the same as the node caption text
            if (currentEntity.label != "")
            {
                // Load dropdown value as well (get first one in DD if it could not be identified)
                int index = Node.FindValueInEnumNodeTypes(currentEntity.label);
                selectNodeTypeDD.value = index == -1 ? 0 : index;
                // Changing dropdown value will result in editing the edit caption text. So chaning back...
                editNodeCaptionText.text = currentEntity.label;
            }
            else
            {
                selectNodeTypeDD.value = 0;
            }
        }
        catch
        {
            print("Error");
        }
    }

    public void LoadDefaultNodeProperties()
    {
        int index = Node.FindValueInEnumNodeTypes(editNodeCaptionText.text);
        //int index = Node.FindValueInEnumNodeTypes(currentEntity.label);

        if (index == -1)
            return;

        Node.NodeTypes nodeType = (Node.NodeTypes)index;

        string defaultPros = DefaultProperties.ForNode(nodeType);

        //currentEntity.
        editNodeProperties.text = defaultPros;

        print("loading default properties for: " + nodeType.ToString());
    }

    public void LoadEdge(bool reloadText = true)
    {
        try
        {
            // Load text
            if (reloadText)
            {
                editEdgeCaptionText.text = currentEdge.label;
                editEdgeProperties.text = DataUtility.GetPropertiesAsString(currentEdge.propertiesDict, ref currentEdge.GetPropertiesREF());

                //editEdgeProperties.text = currentEdge.properties;
            }

            // Load connections for viz
            Node nodeFrom = currentEdge.from.GetComponent<Node>();
            Node nodeTo= currentEdge.to.GetComponent<Node>();

            string nodeFromCaption = nodeFrom.label == "" ? "Caption" : nodeFrom.label;
            editEdgeNodeUpperViz.transform.GetChild(0).GetComponent<Text>().text = nodeFromCaption;

            editEdgeArrowBottomViz.SetActive(true);

            string nodeToCaption = nodeTo.label == "" ? "Caption" : nodeTo.label;
            editEdgeNodeBottomViz.transform.GetChild(0).GetComponent<Text>().text = nodeToCaption;

            // Setting dropdown to be the same as the edge caption text
            if (currentEdge.label != "")
            {
                // Load dropdown value as well
                int index = Connect.FindValueInEnumEdgeTypes(currentEdge.label);
                selectEdgeTypeDD.value = index == -1 ? 0 : index;

                // Changing dropdown value will result in editing the edit caption text. So chaning back...
                editEdgeCaptionText.text = currentEdge.label;
            }
            else
            {
                selectEdgeTypeDD.value = 0;
            }
        }
        catch
        {

        }
    }

    public void ToggleAppBar()
    {
        appBarOpen = !appBarOpen;

        if (appBarOpen == true)
        {
            appBarBtn.GetComponent<Animator>().Play("OpenAppBar");
            appBarBtn.GetComponentInChildren<Text>().text = ">";
            ShowOrHideUIElements(false);
        }
        else
        {
            appBarBtn.GetComponent<Animator>().Play("CloseAppBar");
            appBarBtn.GetComponentInChildren<Text>().text = "<";
            ShowOrHideUIElements(true);
        }
    }

    public void UpdateGremlinCodeText(Node _node)
    {
        string addVertexQuery = "";

        Vertex v = BridgeToGraphson.NodeToVertex(_node);

        addVertexQuery = ObjectToGremlinConverter.VertexToQuery(v);
        UpdateGremlinCodeText(addVertexQuery);
    }

    public void UpdateGremlinCodeText(Connect _edge)
    {
        string addEdgeQuery = "";

        Edge e = BridgeToGraphson.ConnectToEdge(_edge);

        addEdgeQuery = ObjectToGremlinConverter.EdgeToQuery(e);

        UpdateGremlinCodeText(addEdgeQuery);
    }

    public void UpdateGremlinCodeText(string _text)
    {
        if (codeText.GetComponent<InputField>().text.Length > 20000)
            codeText.GetComponent<InputField>().text = "";
        codeText.GetComponent<InputField>().text += _text + '\n';
    }

    public void OpenEditNodeUIFromObj(GameObject obj)
    {
        if (obj.transform.parent != null && obj.transform.parent.GetComponent<Node>() != null)
            OpenEditNode(obj.transform.parent.GetComponent<Node>());
        else if (obj.transform.GetComponent<Node>() != null)
            OpenEditNode(obj.transform.GetComponent<Node>());
    }

    public void ReverseLine()
    {
        EntityManager em = Camera.main.GetComponent<EntityManager>();

        // Save old values
        int currentEdgeType = selectEdgeTypeDD.value;
        string label = editEdgeCaptionText.text;

        em.ReverseLine(currentEdge);
        
        LoadEdge(false);


        // Resetting the type/label/dropdown so it won't be equal to entity label.
        // We want it to be set to chosen value from dropdown in UI.
        selectEdgeTypeDD.value = currentEdgeType;
        editEdgeCaptionText.text = label;

        // Go out from menu
        //currentEdge = null;
        //Globals.inMenu = false;
        //editEdgePanel.SetActive(false);
    }

    public void CopyExportedData()
    {
        TextEditor te = new TextEditor();
        te.text = exportedData.text;
        te.SelectAll();
        te.Copy();
    }

    public void ShowExportedData(string data)
    {
        ToggleAppBar();
        ToggleVisibilityUIElements();

        exportedDataPanel.SetActive(true);
        exportedData.text = data;
        //exportedData.transform.position += Vector3.up * -data.Length * 10;
        Globals.inMenu = true;
    }

    public void HideExportedData()
    {
        ToggleVisibilityUIElements();

        exportedData.text = "";
        exportedDataPanel.SetActive(false);
        Globals.inMenu = false;
    }

    public void ShowLoadPanel(string typeOfLoad)
    {
        importedDataPanel.SetActive(true);
        ToggleAppBar();
        Globals.inMenu = true;

        if (typeOfLoad == "scene")
        {
            importedDataPanel.transform.Find("LoadSceneBtn").gameObject.SetActive(true);
        }
        else if(typeOfLoad == "graphson")
        {
            importedDataPanel.transform.Find("LoadGraphsonBtn").gameObject.SetActive(true);
            importedDataPanel.transform.Find("ConvertToGremlin").gameObject.SetActive(true);
        }
    }

    public void HideLoadScene()
    {
        importedDataPanel.transform.Find("LoadSceneBtn").gameObject.SetActive(false);
        importedDataPanel.transform.Find("LoadGraphsonBtn").gameObject.SetActive(false);
        importedDataPanel.transform.Find("ConvertToGremlin").gameObject.SetActive(false);

        importedDataPanel.SetActive(false);
        Globals.inMenu = false;
    }

    public void LoadScene()
    {
        bool status = arrowImporter.LoadScene(importedData.text);
        HideLoadScene();
        if (status )
            importedData.text = "";
    }

    public void LoadGraphson()
    {
        CosmosDbStructure data = BridgeToGraphson.jsonToCosmosDBStructure(importedData.text);

        HideLoadScene();

        // Create scene with graphson data
        sceneCreator.GenerateScene(data);
    }

    public void ConvertGraphsonToGremlin()
    {
        CosmosDbStructure data = BridgeToGraphson.jsonToCosmosDBStructure(importedData.text);
        string commands = GraphsonToGremlin.ObjectToGremlinConverter.StructureToQueryString(data);
        importedData.text = commands;
        //codeText.GetComponent<TMPro.TextMeshProUGUI>().text += commands;
    }

    public void UploadToDB()
    {
        Transform editObj = uploadToDBPanel.transform.GetChild(0);
        InputField hostname = editObj.Find("hostname Inputfield").GetComponent<InputField>();
        InputField port = editObj.Find("port Inputfield").GetComponent<InputField>();
        InputField authKey = editObj.Find("authKey Inputfield").GetComponent<InputField>();
        InputField database = editObj.Find("database Inputfield").GetComponent<InputField>();
        InputField collection = editObj.Find("collection Inputfield").GetComponent<InputField>();

        Toggle dropDataToggle = editObj.Find("Drop Data Toggle").GetComponent<Toggle>();

        CosmosDbStructure structure = arrowExporter.SceneDataToCosmosDBStructure();
        //CosmosDbStructure data = BridgeToGraphson.jsonToCosmosDBStructure(importedData.text);

        //List<string> queries = GraphsonToGremlin.ObjectToGremlinConverter.StructureToQueryList(structure);

        int _port;
        bool status = int.TryParse(port.text, out _port);
        if (!status)
            _port = 443;

        //bool uploaded = GraphToDB.UploadToCosmosDB(hostname.text, _port, authKey.text, database.text,
        //    collection.text, queries);
        GraphToDB.UploadToCosmosDB(hostname.text, _port, authKey.text, database.text,
            collection.text, structure, dropDataToggle.isOn);

        /*
        if (uploaded)
        {
            ShowMessage("Data has now been uploaded!", Color.green);
        }
        else
        {
            ShowMessage("Something went wrong during the upload!", Color.red);
        }
        */
        Globals.inMenu = false;
        uploadToDBPanel.SetActive(false);
    }

    public void ShowUploadToDB()
    {
        Globals.inMenu = true;
        uploadToDBPanel.SetActive(true);
        
        exportedDataPanel.SetActive(false);
    }

    public void HideUploadToDB()
    {
        Globals.inMenu = false;
        uploadToDBPanel.SetActive(false);
    }
}
