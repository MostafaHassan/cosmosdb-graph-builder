using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityManager : MonoBehaviour
{
    public GameObject nodePrefab;
    public GameObject linePrefab;

    [SerializeField]
    public List<GameObject> entities;

    public float lineOffset = 10.0f;
    public float arrowToNodeOffset = 3;
    public float currentNodeCloserToCameraOffset = 1.0f;

    private LineRenderer currentLine;
    private bool drawing = false;

    private float timer = 0.2f;
    private bool clicked = false;

    private GameObject selectedNode = null;
    private Vector2 previousPoint = Vector2.zero;
    private Vector3 originalPosition;

    private bool hitEdge = false;
    private bool movingNode = false;

    private GameObject changedColorOnNodeEdge;
    private bool colChanger = false;

    private UserInterface ui;

    private GameObject hitObj;

    private GameObject mouseOverNode;
    private Transform currentEdgeHoverOver;

    // Start is called before the first frame update
    void Start()
    {
        ui = GameObject.Find("Canvas").GetComponent<UserInterface>();
    }

    void ResetLineColor()
    {
        if (currentEdgeHoverOver != null)
        {
            currentEdgeHoverOver.GetComponent<LineRenderer>().material.color = Color.black;
            currentEdgeHoverOver.GetChild(0).GetComponent<SpriteRenderer>().color = Color.black;
            currentEdgeHoverOver = null;
        }
    }

    // This function is used to find out if user is moving, selecting or drawing new nodes
    public bool isStopped()
    {
        if (!hitObj && drawing == false && hitEdge == false && movingNode == false)
            return true;
        return false;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            UpdateAllNodesAndEdgesInScene();
        }

        if (Globals.inMenu)
            return;

        if (UnityEngine.EventSystems.EventSystem.current != null &&
            !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            ControlNode();
            DrawLine();
        }

        RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
        // Change line color when over edge aka connection line
        if (hit && hit.transform.tag == "Edge")
        {
            ResetLineColor();

            currentEdgeHoverOver = hit.transform.parent.transform;
            currentEdgeHoverOver.GetComponent<LineRenderer>().material.color = Color.gray;
            currentEdgeHoverOver.GetChild(0).GetComponent<SpriteRenderer>().color = Color.gray;

            if(Input.GetMouseButton(0))
            {
                if (Input.GetKeyDown(KeyCode.Delete))
                {
                    DeleteEdge(currentEdgeHoverOver.GetComponent<Connect>());
                    return;
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    ReverseLine(currentEdgeHoverOver.GetComponent<Connect>());
                    return;
                }
            }
            // If user middle clicks the caption will be set to one from enum
            else if(Input.GetMouseButtonDown(2))
            {
                currentEdgeHoverOver.GetComponent<Connect>().SetNextEdgeType();
                DisplayEdgeType(currentEdgeHoverOver.gameObject);

                ui.SetCurrentEdge(currentEdgeHoverOver.GetComponent<Connect>());
                ui.SaveEdgeButton();
            }
        }
        else
        {
            ResetLineColor();
        }

        if (clicked)
        {
            TickTimer();
        }

        if (timer < 0.0f && timer > -0.2f)
        {
            ResetTimerAndClicked();
        }
    }

    void TickTimer()
    {
        timer -= Time.deltaTime;
    }

    void ResetTimerAndClicked()
    {
        timer = 0.3f;
        clicked = false;
    }

    void CanDoubleClickEdgeAndBlankSpace(RaycastHit2D hit)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (hit)
            {
                if (hit.transform.tag == "Edge")
                {
                    if (clicked && (timer > 0.05f && timer < 0.3f))
                    {
                        // Edge double clicked, now edit it
                        var ui = GameObject.Find("Canvas").transform.GetComponent<UserInterface>();
                        {
                            ui.OpenEditEdge(hit.transform.parent.GetComponent<Connect>());
                        }

                        ResetTimerAndClicked();
                        return;
                    }

                    clicked = true;
                }
            }

            else
            {
                Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
                //Debug.DrawRay(r.origin, r.direction * 20);

                // Create node
                if (clicked && (timer > 0.05f && timer < 0.3f))
                {
                    // Double clicked
                    Vector3 _pos = r.origin + r.direction * 20;
                    _pos.z = 10;

                    GameObject newObj = CreateNode(_pos);
                    newObj.transform.position = _pos;

                    ResetTimerAndClicked();
                    return;
                }

                clicked = true;
            }
        }
    }

    void ControlNode()
    {
        RaycastHit2D hit = (Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition)));
        hitObj = hit == true ? hit.transform.gameObject : null;

        if (!movingNode)
        {
            ControlNodeColor(hit);

            // Zoom to entity
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (hit)
                {
                    Camera.main.transform.position = new Vector3(
                        hit.transform.position.x, hit.transform.position.y,
                        Camera.main.transform.position.z
                        );
                }
            }
        }

        CanDoubleClickEdgeAndBlankSpace(hit);

        if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1) && !Input.GetKey(KeyCode.LeftAlt))
        {
            if (hit && hit.transform.tag == "Node")
            {
                // Update mouse and keep track of this object
                previousPoint = Input.mousePosition;
                originalPosition = hit.transform.position;

                selectedNode = hit.transform.gameObject;

                // Display that this object has been "selected"
                PlayClickAnim(hit.transform.gameObject);

                // Double clicked - open menu
                if (clicked && (timer > 0.05f && timer < 0.3f))
                {
                    ui.OpenEditNodeUIFromObj(hit.transform.gameObject);

                    selectedNode = null;

                    ResetTimerAndClicked();
                    return;
                }
                else
                {
                    timer = 0.3f;
                }

                clicked = true;
            }

            // Edge has been clicked, start drawing line to current mouse pos
            else if (hit && hit.transform.tag == "NodeEdge")
            {
                OnEdgeClicked(hit.transform.gameObject);
            }
        }

        // Draw line by right-clicking on node
        else if (Input.GetMouseButtonDown(1) && !Input.GetMouseButton(0))
        {
            if (hit && hit.transform.tag == "Node")
            {
                // The user right clicked the node, this counts as clicking on a node edge
                OnEdgeClicked(hit.transform.parent.gameObject);
            }
        }

        // Move entity and zoom to entity if F-clicked
        if (Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetKey(KeyCode.LeftAlt))
        {
            if (selectedNode != null)
            {
                // Moving node
                Vector2 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                //Vector3 dir = (Input.mousePosition - new Vector3(previousPoint.x, previousPoint.y, 0));
                Vector3 dir3D = new Vector3(currentPos.x, currentPos.y, lineOffset) - selectedNode.transform.parent.transform.position;
                
                if (dir3D.magnitude > 10.0f || movingNode == true)
                {
                    Vector3 newPos = new Vector3(currentPos.x, currentPos.y, lineOffset);
                    Vector3 newPosDir = newPos - selectedNode.transform.parent.transform.position;

                    selectedNode.transform.parent.transform.position += newPosDir * Time.deltaTime * 30;


                    // Moving nodes should be a little bit closer to the camera, this will prevent current node from being visually "behind" other nodes
                    //selectedNode.transform.localPosition = new Vector3(0, 0, -0.5f); 

                    selectedNode.transform.parent.GetComponent<Node>().UpdatePos(selectedNode.transform.parent.transform.position);

                    // Move all startlines connections (startpos)
                    MoveLineStartPositions(selectedNode.transform.parent.GetChild(1));

                    // Move all endlines connections (endpos)
                    if (selectedNode.transform.parent.tag == "NodeEdge")
                    {
                        Node e = selectedNode.GetComponent<Node>();
                        List<Connect> connections = selectedNode.transform.parent.GetComponent<Node>().connections;

                        //print(connections.Count);
                        for (int i = 0; i < connections.Count; i++)
                        {
                            if (connections[i] != null)
                            {
                                UpdateEdge(connections[i].transform.GetComponent<LineRenderer>(), connections[i].from,
                                   connections[i].to);
                            }
                            else
                            {
                                connections.RemoveAt(i);
                            }
                        }
                    }
                    movingNode = true;
                }

                if (Input.GetKey(KeyCode.Delete))
                {
                    DeleteNode(selectedNode.transform.parent.gameObject);
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            /* // Move back the node visually
            if(selectedNode != null)
            {
                selectedNode.transform.localPosition = new Vector3(0, 0, -0.05f);
            }
            */
            
            selectedNode = null;
            previousPoint = Vector3.zero;
            movingNode = false;
        }

        // If user middle clicks the caption will be set to one from enum
            else if(Input.GetMouseButtonDown(2))
            {
                if (hit && hit.transform.tag == "Node")
                {
                    Node _n = hit.transform.parent.GetComponent<Node>();
                    _n.SetNextNodeType();
                    ui.SetCurrentEntity(_n);

                    Color tempCol = _n.color;
                    ui.SaveNodeButton();
                    _n.color = tempCol;
                    _n.ShowNodeColor();
                }
            }
    }

    public void MoveLineStartPosition(Transform linesGO, int i)
    {
        Transform _to = linesGO.GetChild(i).transform.GetComponent<Connect>().to.transform;
        Transform _from = linesGO.GetChild(i).transform.GetComponent<Connect>().from.transform;

        Vector3 _toPos = _to.transform.position;
        Vector3 _fromPos = _from.transform.position;

        UpdateEdge(linesGO.GetChild(i).GetComponent<LineRenderer>(), _from.gameObject, _to.gameObject);
    }

    public void MoveLineStartPositions(Transform linesGO)
    {
        if (linesGO.childCount > 0)
        {
            for (int i = 0; i < linesGO.childCount; i++)
            {
                MoveLineStartPosition(linesGO, i);
            }
        }
    }

    void ControlNodeColor(RaycastHit2D hit)
    {
        if (hit && hit.transform.tag == "NodeEdge")
        {
            Color col;

            // Change back previous node edge
            if (changedColorOnNodeEdge != null)
            {
                col = hit.transform.GetComponent<SpriteRenderer>().color;
                changedColorOnNodeEdge.transform.GetComponent<SpriteRenderer>().color = new Color(col.r, col.g, col.b, 0);
            }

            // Hover over edge, color it blue by removing alpha
            changedColorOnNodeEdge = hit.transform.gameObject;
            colChanger = true;
            col = hit.transform.GetComponent<SpriteRenderer>().color;
            changedColorOnNodeEdge.transform.GetComponent<SpriteRenderer>().color = new Color(col.r, col.g, col.b, 1);
        }

        else
        {
            // Out of edge, reset color
            if (colChanger == true)
            {
                Color col = changedColorOnNodeEdge.GetComponent<SpriteRenderer>().color;
                changedColorOnNodeEdge.GetComponent<SpriteRenderer>().color = new Color(col.r, col.g, col.b, 0);
                changedColorOnNodeEdge = null;
                colChanger = false;
            }

            // If mose over node
            if (hit && hit.transform.tag == "Node")
            {
                if (mouseOverNode != null)
                {
                    mouseOverNode.transform.GetComponent<SpriteRenderer>().color = mouseOverNode.transform.parent.GetComponent<Node>().color;  //Color.white;
                    mouseOverNode = null;
                }
                mouseOverNode = hit.transform.gameObject;
                mouseOverNode.transform.GetComponent<SpriteRenderer>().color = Color.gray;
            }

            else
            {
                if (mouseOverNode != null)
                {
                    mouseOverNode.transform.GetComponent<SpriteRenderer>().color = mouseOverNode.transform.parent.GetComponent<Node>().color; //Color.white;
                    mouseOverNode = null;
                }
            }
        }
    }

    void MoveArrow(Transform arrow, LineRenderer line)
    {
        Vector3 arrowDirection = line.GetPosition(1) - line.GetPosition(0);
        arrowDirection.Normalize();

        arrow.position = line.GetPosition(1) - arrowDirection;
        arrow.right = arrowDirection;
        /*
        // Rotate Arrow
        arrow.right = rightDir;

        // Lock x-rot
        arrow.rotation = Quaternion.Euler(
            0, arrow.transform.localEulerAngles.y, arrow.transform.localEulerAngles.z);

        arrow.position = pos - arrow.right * lineOffset * 0.2f;
        */

        DisplayEdgeType(arrow.parent.gameObject);
    }
    
    public void DisplayEdgeType(GameObject connectionLine)
    {
        GameObject edgeText = connectionLine.transform.Find("Type").gameObject;
        TextMesh tm = edgeText.GetComponent<TextMesh>();

        // Show text if there is any
        Connect con = connectionLine.GetComponent<Connect>();
        tm.text = con.label;

        // Move around text only if there is text
        if (connectionLine.GetComponent<Connect>().label != "")
        {
            // Position the text in the middle of the two nodes with a bit of offset up

            // Middle
            //tm.transform.position = (con.from.transform.position + con.to.transform.position) * 0.5f;
            Vector3 pos1 = connectionLine.GetComponent<LineRenderer>().GetPosition(0);
            Vector3 pos2 = connectionLine.GetComponent<LineRenderer>().GetPosition(1);

            Vector3 dir = pos2 - pos1;
            dir *= 0.5f;

            tm.transform.position = (pos1 + pos2) * 0.5f;

            //float offsetUp = 3.0f;
            Vector3 offsetUp = Vector3.Cross(dir.normalized, Vector3.forward).normalized;

            float xOffset = 2;

            // Rotate the text to follow the direction of the line
            if (dir.normalized.x > 0.0f)
            {
                tm.transform.right = dir.normalized;
                offsetUp *= -1;
            }
            else
            {
                tm.transform.right = -dir.normalized;
                xOffset = 5;
            }

            // Offset text on x axis
            tm.transform.position += dir.normalized * xOffset;

            // Offset up
            //Vector3 _up = new Vector3(dir.y, -dir.x);
            //tm.transform.position += _up.normalized * offsetUp;
            tm.transform.position += offsetUp * 3;
        }
    }

    void OnEdgeClicked(GameObject hit)
    {
        if (hitEdge)
        {
            return; // run once
        }

        hitEdge = true;
        originalPosition = hit.transform.position;

        // Create Line
        GameObject line = Instantiate(linePrefab, hit.transform.position, Quaternion.identity,
                    hit.transform.GetChild(1));

        currentLine = line.GetComponent<LineRenderer>();
        currentLine.SetPosition(0, originalPosition + Vector3.forward);

        Connect lineConnection = line.GetComponent<Connect>();
        lineConnection.from = hit.transform.gameObject;

    }

    void DrawLine()
    {
        // Do not allow drawing during movement of node
        if (movingNode)
        {
            return;
        }

        // Drawing...
        if (hitEdge == true)
        {
            // Get last line and use it
            Vector3 _toPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector3 posToNode = _toPos - currentLine.GetComponent<Connect>().from.transform.position;
            _toPos.z = 10;
            posToNode = _toPos - posToNode.normalized * (lineOffset + arrowToNodeOffset);

            posToNode.z = 10;
            currentLine.SetPosition(1, posToNode);

            // Move Arrow
            Transform arrow = currentLine.transform.GetChild(0);
            //Vector3 dirRight = (posToNode - currentLine.GetComponent<Connect>().from.transform.position).normalized;
            MoveArrow(arrow, currentLine);

            drawing = true;
        }

        // Drawing complete, releasing line
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            hitEdge = false;

            if (drawing)
            {
                // create a relation between the 2 nodes (if on other Entity/node) 
                RaycastHit2D hit = (Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition)));
                if (hit && hit.transform.tag == "Node")
                {
                    // Do not create node if user clicked on same node
                    if (hit.transform.parent != null && hit.transform.parent.gameObject != currentLine.GetComponent<Connect>().from)
                    {
                        currentLine.transform.GetComponent<Connect>().to = hit.transform.parent.gameObject;
                        hit.transform.GetComponentInParent<Node>().connections.Add(currentLine.GetComponent<Connect>());

                        int edgesCount = GetAmountOfConnectionsBetweenTwoNodes(currentLine.GetComponent<Connect>().from.GetComponent<Node>(),
                            currentLine.GetComponent<Connect>().to.GetComponent<Node>());

                        currentLine.GetComponent<Connect>().lineIndex = edgesCount;
                        UpdateEdge(currentLine, currentLine.GetComponent<Connect>().from, currentLine.GetComponent<Connect>().to);
                    }
                    else
                    {
                        Destroy(currentLine.gameObject);
                    }
                }

                // if on clear space, create a new entity/node
                else
                {
                    Vector3 _nodePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    _nodePos.z = 10;
                    GameObject newNode = CreateNode(_nodePos);
                    newNode.transform.position = _nodePos;
                    currentLine.transform.GetComponent<Connect>().to = newNode;
                    newNode.GetComponent<Node>().connections.Add(currentLine.GetComponent<Connect>());

                    UpdateNewEdge(currentLine, currentLine.GetComponent<Connect>().from, newNode);
                }

                drawing = false;
            }
        }
    }

    private void AddColliderToLine(LineRenderer line)
    {
        var startPos = line.GetPosition(0);
        var endPos = line.GetPosition(line.positionCount - 1);

        if (line.transform.Find("Collider") == null)
        {
            BoxCollider2D col = new GameObject("Collider").AddComponent<BoxCollider2D>();
            col.transform.tag = "Edge";
            col.transform.parent = line.transform;
        }

        MoveAndResizeLineCollider(line);
    }

    private void MoveAndResizeLineCollider(LineRenderer line)
    {
        var startPos = line.GetPosition(0);
        var endPos = line.GetPosition(line.positionCount - 1);

        BoxCollider2D col = line.transform.Find("Collider").GetComponent<BoxCollider2D>();
        float lineLength = Vector3.Distance(startPos, endPos);
        float offsetForArrow = 3.0f;
        float colliderWidth = (line.startWidth + line.endWidth) * 0.5f + 0.8f;
        col.size = new Vector3(lineLength + offsetForArrow, colliderWidth, 0.25f);
        //col.size = new Vector3(lineLength, 0.175f, 0.25f);
        Vector3 midPoint = (startPos + endPos) / 2;
        col.transform.position = midPoint;
        float angle = (Mathf.Abs(startPos.y - endPos.y) / Mathf.Abs(startPos.x - endPos.x));
        if ((startPos.y < endPos.y && startPos.x > endPos.x) || (endPos.y < startPos.y && endPos.x > startPos.x))
        {
            angle *= -1;
        }
        angle = Mathf.Rad2Deg * Mathf.Atan(angle);
        col.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void PlayClickAnim(GameObject obj)
    {
        //print("Change color...");
        obj.GetComponent<Animator>().Play("btnClick");
    }

    public void ReverseLine(Connect edge)
    {
        // Swap to and from
        GameObject _temp = edge.to;
        edge.to = edge.from;
        edge.from = _temp;

        EntityManager em = Camera.main.GetComponent<EntityManager>();

        // Get Lines object from the other node
        // Note that from as now to and to is from
        GameObject linesObj = edge.from.gameObject.transform.GetChild(1).gameObject;
        edge.transform.SetParent(linesObj.transform);

        Node node = linesObj.transform.parent.GetComponent<Node>();
        for (int i = 0; i < node.connections.Count; i++)
        {
            // Find edge and remove it fron connections list
            if (node.connections[i] == edge)
            {
                Destroy(edge.to.GetComponent<Connect>());

                node.connections.RemoveAt(i);
                
                break;
            }
        }

        edge.to.GetComponent<Node>().connections.Add(edge);

        // Find edge in new line object and 
        // update line and arrow to look good for new transformation
        Transform newLineParent = edge.from.transform.GetChild(1);
        for (int i = 0; i < newLineParent.childCount; i++)
        {
            if (newLineParent.GetChild(i).GetComponent<Connect>() == edge)
            {
                MoveLineStartPosition(newLineParent, i);
            }
        }
    }

    public void DeleteEdge(Connect edge)
    {
        GameObject nodeToObj = edge.to;
        GameObject nodeFromObj = edge.from;

        Node nodeTo = nodeToObj.transform.GetComponent<Node>();
        int index = nodeTo.FindNodeInConnection(edge);
        if (index != -1)
        {
            nodeTo.connections.RemoveAt(index);
        }

        Destroy(edge.transform.gameObject);

        // Iterate through all existing lines between the nodes and give them new indices
        SetNewEdgeIndicies(nodeFromObj, nodeToObj);

        // Update all existing edges between the nodes
        List<Connect> resultingEdges = GetConnectionsBetweenTwoNodes(nodeFromObj.GetComponent<Node>(), nodeTo);
        foreach(Connect c in resultingEdges)
        {
            LineRenderer lr = c.GetComponent<LineRenderer>();
            UpdateEdge(lr, c.from, c.to );
        }
    }

    public void SetNewEdgeIndicies(GameObject from, GameObject to)
    {
        Node fromNode = from.GetComponent<Node>();
        Node toNode = to.GetComponent<Node>();
        List<Connect> myConnections = GetConnectionsBetweenTwoNodes(from.GetComponent<Node>(), to.GetComponent<Node>());

        for(int i = 0; i < myConnections.Count; i++)
        {
            myConnections[i].lineIndex = i + 1;
        }
    }

    public GameObject CreateNode(Vector2 _pos)
    {
        GameObject _node = Instantiate(nodePrefab, new Vector3 (_pos.x, _pos.y, lineOffset), Quaternion.identity);
        _node.transform.GetChild(0).GetComponent<SpriteRenderer>().color = _node.GetComponent<Node>().color;
        
        entities.Add(_node);

        return _node;
    }
    
    // Not supposed to be used every frame!
    // This function updates all edges for all nodes in scene
    public void UpdateAllNodesAndEdgesInScene()
    {
        foreach(GameObject e in entities)
        {
            Node _n = e.GetComponent<Node>();
            _n.UpdatePos(e.transform.position);
            
            foreach(Connect c in _n.connections)
            {
                UpdateEdge(c.GetComponent<LineRenderer>(), c.from, c.to);
            }
        }
    }

    public bool DeleteNode(GameObject node)
    {
        int i = 0;
        foreach (GameObject n in entities)
        {
            if (n == node)
            {
                // Delete node
                n.GetComponent<Node>().DeleteConnections();
                entities.RemoveAt(i);
                Destroy(n);
                return true;
            }
            i++;
        }
        return false;
    }

    public LineRenderer CreateEdge(GameObject _nodeFrom, GameObject _nodeTo)
    {
        GameObject lineObj = Instantiate(linePrefab, _nodeFrom.transform.GetChild(1));
        LineRenderer line = lineObj.GetComponent<LineRenderer>();

        return line;
    }

    public void UpdateEdge(LineRenderer line, GameObject _nodeFrom, GameObject _nodeTo)
    {
        Vector3 posFrom = _nodeFrom.transform.position;
        Vector3 posTo = _nodeTo.transform.position - (_nodeTo.transform.position - posFrom).normalized * lineOffset;

        Vector3 offset = GetLineOffsetFromOtherLines(line, _nodeFrom, _nodeTo);

        posFrom += offset;
        posTo += offset;
            //posFrom += line.GetComponent<Connect>().lineOffset;
            //posTo += line.GetComponent<Connect>().lineOffset;

        line.SetPosition(0, posFrom);
        line.SetPosition(1, posTo);
        
        //currentLine.GetComponent<BezierScript>().UpdateLine();

        Connect _line = line.GetComponent<Connect>();

        // Move Arrow
        Transform arrow = _line.transform.GetChild(0);
        Vector3 dirRight = (posTo - _line.from.transform.position).normalized;
        MoveArrow(arrow, line);

        // Create collider to line
        AddColliderToLine(line);
    }

    public Vector3 GetLineOffsetFromOtherLines(LineRenderer line, GameObject _nodeFrom, GameObject _nodeTo)
    {
        int lineCount = line.GetComponent<Connect>().lineIndex;
        // Get sibling index and offset it using that
        //int lineCount = line.transform.GetSiblingIndex() + 1;
        
        //lineCount = GetAmountOfConnectionsBetweenTwoNodes(_nodeFrom.GetComponent<Node>(), _nodeTo.GetComponent<Node>());
        
        if (lineCount >= 2)
        {
            // Toggle up and down / right and left
            int toggleSide = lineCount % 2;
            float halfCount = lineCount / 2;
           // if (lineCount > 3)
           //     halfCount += 1;
            
            //halfCount += toggleSide;
            float lineOffset = 4.0f;
            halfCount *= lineOffset;

            // Get middle position between the nodes
            Vector3 dirBetweenNodes = _nodeTo.transform.position - _nodeFrom.transform.position;
            Vector3 dirOffset = Vector3.Cross(dirBetweenNodes.normalized, Vector3.forward);

            if (dirBetweenNodes.x > 0.0f)
                dirOffset *= -1;

            if(toggleSide == 0)
                dirOffset *= -1; // Invert offset every other time

            Vector3 newPos = dirOffset * halfCount;
            return newPos;
        }
        return Vector3.zero;
    }

    public List<Connect> GetConnectionsBetweenTwoNodes(Node a, Node b)
    {
        List<Connect> cons = new List<Connect>();
        foreach (Connect c in a.connections)
        {
            if (c.from.GetComponent<Node>() == b || c.to.GetComponent<Node>() == b)
            {
                cons.Add(c);
            }
        }

        foreach (Connect c in b.connections)
        {
            if (c.from.GetComponent<Node>() == a || c.to.GetComponent<Node>() == a)
            {
                cons.Add(c);
            }
        }

        return cons;
    }

    public int GetAmountOfConnectionsBetweenTwoNodes(Node a, Node b)
    {
        int amountOfLines = GetConnectionsBetweenTwoNodes(a, b).Count;
        return amountOfLines;
    }

    public void UpdateNewEdge(LineRenderer line, GameObject _nodeFrom, GameObject _nodeTo)
    {
        Node _n = _nodeTo.transform.GetComponent<Node>();
        Connect _line = line.GetComponent<Connect>();

        // Add connect to node connections if does not already exist
        bool found = false;
        for (int i = 0; i < _n.connections.Count; i++)
        {
            if (_n.connections[i] == _line)
            {
                found = true;
                break;
            }

        }
        if (!found)
            _n.connections.Add(line.GetComponent<Connect>());

        // Set edge index
        int edgesCount = GetAmountOfConnectionsBetweenTwoNodes(_line.from.GetComponent<Node>(),
                            _line.to.GetComponent<Node>());

        _line.lineIndex = edgesCount;

        // Update line data and position it accordingly
        UpdateEdge(line, _nodeFrom, _nodeTo);
    }
}
