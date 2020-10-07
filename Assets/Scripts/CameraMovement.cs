using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float movementSpeed = 80;
    private float multiSpeed = 1;

    public float cameraPanSpeed = 0.5f;
    private float scrollSensitivity = -5000.0f;

    Vector3 lastClicked = Vector3.zero;
    Vector3 lastCamPosition = Vector3.zero;

    UserInterface _userInterface;

    private Vector3 originalCamPos;

    // Start is called before the first frame update
    void Start()
    {
        lastCamPosition = Camera.main.transform.position;
        _userInterface = GameObject.Find("Canvas").GetComponent<UserInterface>();

        originalCamPos = Camera.main.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Globals.inMenu)
            return;

        // Reset camera position and zoom
        if (Input.GetKeyDown(KeyCode.F))
        {
            EntityManager e = Camera.main.GetComponent<EntityManager>();

            if (e.isStopped())
            {
                Camera.main.transform.position = originalCamPos;
                Camera.main.orthographicSize = 34;
            }
        }

        // Zoom with buttons
        if (Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.Period))
        {
            ZoomIn();
        }
        else if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.Comma))
        {
            ZoomOut();
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            // Zoom with scroll
            Camera.main.orthographicSize += Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity * Time.deltaTime;
        }

        // Panning
        if (Input.GetMouseButtonDown(0))
        {
            // Save origin pos
            lastClicked = Input.mousePosition;
            lastCamPosition = Camera.main.transform.position;
        }
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(0))
        {
            Vector3 dir = Input.mousePosition - lastClicked;
            Camera.main.transform.position = lastCamPosition - dir * cameraPanSpeed;
        }

        // Move Faster
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            multiSpeed = 3;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            multiSpeed = 1;
        }

        // Move Camera with keys
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            transform.position += Vector3.up * movementSpeed * multiSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            transform.position += Vector3.left * movementSpeed * multiSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            transform.position += Vector3.down * movementSpeed * multiSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            transform.position += Vector3.right * movementSpeed * multiSpeed * Time.deltaTime;
        }


        // Clamp camera zooming
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 20, 400);
    }

    public void ZoomInButton()
    {
        Camera.main.orthographicSize -= 20f * multiSpeed;
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 20, 400);
    }
    public void ZoomOutButton()
    {
        Camera.main.orthographicSize += 20 * multiSpeed;
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 20, 400);
    }

    public void ZoomIn()
    {
        Camera.main.orthographicSize -= 2f * multiSpeed;
    }
    public void ZoomOut()
    {
        Camera.main.orthographicSize += 2 * multiSpeed;
    }
}
