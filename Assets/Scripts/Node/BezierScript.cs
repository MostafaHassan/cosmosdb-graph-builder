using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierScript : MonoBehaviour
{
    public LineRenderer lineRenderer;

    public Connect connection;
    public Transform p0;
    public Transform p1;
    public Transform p2;

    void Start()
    {
        connection = GetComponent<Connect>();
        lineRenderer = GetComponent<LineRenderer>();

        p1 = transform.Find("CurveLine");
    }

    public void UpdateLine()
    {
        connection = GetComponent<Connect>();
        Vector3 _from = connection.from.transform.position;
        Vector3 _to = connection.to.transform.position;

        p1.transform.position = (_from + _to) * 0.5f;

        // Get up vector between the two nodes
        float upFactor = 10;
        Vector3 dir = _to - _from;
        dir = dir.normalized * upFactor;

        Vector3 newDir = Vector3.Cross(Vector3.forward, dir);
        Debug.DrawLine(p1.transform.position, p1.transform.position + newDir);

        p1.transform.position += newDir;


        p0 = connection.from.transform;
        p2 = connection.to.transform;

        DrawQuadraticBezierCurve(p0.position, p1.position, p2.position);
    }

    void Update()
    {
        
    }

    void DrawQuadraticBezierCurve(Vector3 point0, Vector3 point1, Vector3 point2)
    {
        lineRenderer.positionCount = 15;
        float t = 0f;
        Vector3 B = new Vector3(0, 0, 0);
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            B = (1 - t) * (1 - t) * point0 + 2 * (1 - t) * t * point1 + t * t * point2;
            lineRenderer.SetPosition(i, B);
            t += (1 / (float)lineRenderer.positionCount);
        }
    }
}