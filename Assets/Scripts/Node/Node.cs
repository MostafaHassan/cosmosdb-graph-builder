using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;
using System.Linq;

[System.Serializable]
public class Node : MonoBehaviour
{
    public enum NodeTypes
    {
        Select,
        Person,
        Employee,
        Company,
        Application,
        Case,
        RealEstate,
        Permit,
        Record,
        Registration
    }

    [SerializeField]
    public string type;

    [SerializeField]
    public int instanceID;

    [SerializeField]
    public float x;

    [SerializeField]
    public float y;

    [SerializeField]
    public Color color;

    [SerializeField]
    public string label = "";

    public Dictionary<string, string> propertiesDict;

    [SerializeField]
    public List<Connect> connections;

    private string properties;

    private Vector3 _originalBubbleScale;
    private Transform dialogBubble_Text;

    private void Awake()
    {
        propertiesDict = new Dictionary<string, string>();
        //propertiesDict.ToArray();
        connections = new List<Connect>();

        dialogBubble_Text = transform.Find("VisualizeData_Pivot").Find("VisualizeData").transform.Find("propertiesText");

        _originalBubbleScale = dialogBubble_Text.parent.localScale;
    }

    void Start()
    {
        type = "vertex";
        instanceID = transform.GetInstanceID();
        x = transform.position.x;
        y = transform.position.y;

        SetNodeIcon();
    }

    public void ShowNodeColor()
    {
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = GetComponent<Node>().color;
    }

    public void Update()
    {
        
    }

    public void UpdatePos(Vector3 _pos)
    {
        x = _pos.x;
        y = _pos.y;
    }

    public void SetNodeIcon()
    {
        int index = FindValueInEnumNodeTypes(label);
        if(index == -1)
        {
            transform.GetChild(0).Find("icon").GetComponent<SpriteRenderer>().sprite = null;
            transform.GetChild(0).Find("icon").gameObject.SetActive(false);
            return;
        }

        NodeTypes nt = (NodeTypes)index;

        Sprite toUse = null;
        toUse = Resources.Load<Sprite>("Icons/"+nt.ToString());
        transform.GetChild(0).Find("icon").GetComponent<SpriteRenderer>().sprite = toUse;
        transform.GetChild(0).Find("icon").gameObject.SetActive(true);
    }

    public void SetVisualLabel()
    {
        string visualLabel = label;
        if (visualLabel.Length > 12)
            visualLabel = visualLabel.Insert((int)(visualLabel.Length * 0.5f), ""+'\n');
        if (visualLabel == "")
            visualLabel = "Caption";
        transform.GetChild(0).GetChild(0).GetComponent<TextMesh>().text = visualLabel;
    }

    // Show dialog bubbles if it's not turned off globally
    public void ShowDialogBubble()
    {
        if (Globals.showDialogBubbles)
        {
            // Show dialog bubble
            if (propertiesDict != null && propertiesDict.Count != 0)
            {
                dialogBubble_Text.GetComponent<TextMesh>().text = DataUtility.GetPropertiesAsString(propertiesDict, ref GetPropertiesREF());

                ResizeDialogBubbleToFitText(dialogBubble_Text);
                dialogBubble_Text.parent.parent.gameObject.SetActive(true);
            }
            // Hide dialog bubble
            else
            {
                dialogBubble_Text.parent.parent.gameObject.SetActive(false);
            }
        }
    }

    public void ResizeDialogBubbleToFitText(Transform dialogbubbleText)
    {
        float scaleFactor = 1f;
        float textSizeCol = 0.1f;

        string text = dialogbubbleText.GetComponent<TextMesh>().text;
        string[] lines = text.Split('\n');

        // Analyze text and put new lines in middle of text that are long
        string newText = "";
        foreach (string lineText in lines)
        {
            string t = lineText;
            if (t.Length > 25 && t.Length <= 50)
            {
                t = lineText.Insert((int)(lineText.Length * 0.5f), "" + '\n' + '\t');
            }
            else if (t.Length > 50)
            {
                t = t.Insert((int)(t.Length * 0.25f), "" + '\n' + '\t');

                t = t.Insert((int)(t.Length * 0.5f), "" + '\n' + '\t');

                t = t.Insert((int)(t.Length * 0.8f), "" + '\n' + '\t');
            }
            newText += t + '\n';
        }
        text = newText;
        dialogbubbleText.GetComponent<TextMesh>().text = newText;

        // Update lines
        lines = text.Split('\n');

        int amountOfRows = lines.Length - 1;

        textSizeCol += amountOfRows * 0.01f;
        scaleFactor = 1 + (amountOfRows * textSizeCol);

        
        // Unparent text from actual bubble and resize the bubble. Re-parent the text to bubble and re-position text to start
        Transform _parent = dialogbubbleText.parent;
        dialogbubbleText.parent = null;

        //Vector3 _scale = _parent.transform.localScale;
        _parent.transform.localScale = _originalBubbleScale  * scaleFactor;

        // Scale bubble on x axis to fit long words
        Vector3 _scale = _parent.transform.localScale;
        int longestTextOnALine = 1;
        foreach(string lineText in lines)
        {
            longestTextOnALine = lineText.Length > longestTextOnALine ? lineText.Length : longestTextOnALine;
        }

        float scaleFactorX = 0.002f;

        if (longestTextOnALine > 10)
            scaleFactorX = 0.003f;

        if (longestTextOnALine > 15)
            scaleFactorX = 0.005f;

        if (longestTextOnALine > 20)
            scaleFactorX = 0.014f;

        float scaleX = 1 + longestTextOnALine * scaleFactorX - scaleFactor * 0.15f;

        float startScalingDownX = 0;
        if (_scale.y > 1.0f)
        {
            startScalingDownX = _scale.y * 0.05f;
        }
        
        float finalScaleX = _scale.x * scaleX - startScalingDownX;
        finalScaleX = Mathf.Clamp(finalScaleX, 0.4f, 1.056101f);

        float finalScaleY = Mathf.Clamp(_scale.y, 0.5f, 2.2f);

        _parent.transform.localScale = new Vector3(finalScaleX, finalScaleY, _scale.z);

        dialogbubbleText.parent = _parent;
        dialogbubbleText.localPosition = _parent.Find("textStart").transform.localPosition;

        // Offset a bit closer to camera, in order to layer it infront of bubble
        dialogbubbleText.localPosition -= Vector3.forward * 0.1f; 
    }

    public void HideDialogBubble()
    {
        Transform dialogBubble = transform.Find("VisualizeData_Pivot").Find("VisualizeData").transform.Find("propertiesText");
        dialogBubble.parent.parent.gameObject.SetActive(false);
    }

    public static int FindValueInEnumNodeTypes(string val)
    {
        var values = Node.NodeTypes.GetValues(typeof(Node.NodeTypes));
        for (int x = 0; x < values.Length; x++)
        {
            if (values.GetValue(x).ToString().ToLower() == val.ToLower())
            {
                return x;
            }
        }
        return -1;
    }

    public void SetNextNodeType()
    {
        // Set caption to next edge type. 
        // Cirulates at last element and jumps over index 0
        int index = FindValueInEnumNodeTypes(label);
        if (index != -1)
        {
            index++;
            index %= NodeTypes.GetValues(typeof(NodeTypes)).Length;
            index = index == 0 ? index + 1 : index;

            NodeTypes newNodeType = (NodeTypes)index;
            label = newNodeType.ToString().ToLower();
        }
        else
        {
            // If caption could not be found in types set to first
            int firstInNodeTypes = 1;
            label = ((NodeTypes)firstInNodeTypes).ToString().ToLower();
        }
    }

    public void DeleteConnections()
    {
        // Delete connections here
        int i = 0;
        while (i < connections.Count)
        {
            connections[i].to = null;
            connections[i].from = null;
            Destroy(connections[i]);
            Destroy(connections[i].transform.gameObject);
            connections.RemoveAt(i);
        }
        connections.Clear();
    }

    public int FindNodeInConnection(Connect c)
    {
        int index = -1;
        for(int i = 0; i < connections.Count; i++)
        {
            if(connections[i] == c)
            {
                index = i;
            }
            else if(connections[i] == c)
            {
                index = i;
            }
        }
        return index;
    }

    public string GetProperties()
    {
        return properties;
    }

    public void SetProperties(string p)
    {
        properties = p;
    }

    public ref string GetPropertiesREF()
    {
        return ref properties;
    }
}
