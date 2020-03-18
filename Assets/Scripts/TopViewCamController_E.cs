using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopViewCamController_E : MonoBehaviour
{
    public Transform camTransform;

    public float regularSpeed;
    public float quickSpeed;

    public float cameraSpeed;
    public float cameraTime;
    public float amountRot;
    public Vector3 zoomValue;
    public Vector3 dragCurrentPosition;
    public Vector3 dragStartPosition;

    public Vector3 newPos;
    public Quaternion newRot;
    public Vector3 newZoom;
    public float rotateStartPosition;
    public float rotateCurrentPosition;
    public Vector3 rotStartPos;
    public Vector3 rotCurPos;

    void Start()
    {
        newPos = transform.position;
        newRot = transform.rotation;
        newZoom = camTransform.localPosition;
    }

    void Update()
    {

        mouseMovementInput();
        MovementCam();
    }

    void mouseMovementInput()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            newZoom += Input.mouseScrollDelta.y * zoomValue;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragStartPosition = ray.GetPoint(entry);
            }
        }
        if (Input.GetMouseButton(0))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragCurrentPosition = ray.GetPoint(entry);

                newPos = transform.position + dragStartPosition - dragCurrentPosition;
            }
        }


        //rotate world by right clicking
        if (Input.GetMouseButtonDown(1))
        {
            rotStartPos = Input.mousePosition;
        }
        if (Input.GetMouseButton(1))
        {
            rotCurPos = Input.mousePosition;

            Vector3 difference = rotStartPos - rotCurPos;

            rotStartPos = rotCurPos;

            newRot *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));
        }

    }


    void MovementCam()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            cameraSpeed = quickSpeed;
        }
        else
        {
            cameraSpeed = regularSpeed;
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            newPos += (transform.forward * cameraSpeed);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            newPos += (transform.forward * -cameraSpeed);
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            newPos += (transform.right * cameraSpeed);
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            newPos += (transform.right * -cameraSpeed);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            newRot *= Quaternion.Euler(Vector3.up * amountRot);
        }
        if (Input.GetKey(KeyCode.E))
        {
            newRot *= Quaternion.Euler(Vector3.up * -amountRot);
        }
        if (Input.GetKey(KeyCode.R))
        {
            newZoom += zoomValue;
        }
        if (Input.GetKey(KeyCode.F))
        {
            newZoom -= zoomValue;
        }
        transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * cameraTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRot, Time.deltaTime * cameraTime);
        camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, newZoom, Time.deltaTime * cameraTime);
    }

}
