using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float minHeight = 10;
    public float maxHeight = 200;
    public float ySpeed = 200;
    public float xSpeed = 20;
    public float rotationSpeed = 50;
    public float borderRatio = 0.3f;
    public bool mouseLock = true;

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            this.transform.Rotate(Input.GetAxis("Mouse ScrollWheel") * rotationSpeed * Vector3.up, Space.World);
        }
        else
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0 && this.transform.position.y > minHeight)
            {
                this.transform.position += Time.deltaTime * ySpeed * transform.forward;
            }
            if (Input.GetAxis("Mouse ScrollWheel") < 0 && this.transform.position.y < maxHeight)
            {
                this.transform.position += Time.deltaTime * ySpeed * -transform.forward;
            }
        }
        Vector3 mousePos = Input.mousePosition;
        float borderY = borderRatio * Screen.height;
        float borderX = borderRatio * Screen.width;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            mouseLock = !mouseLock;
        }
        if (Input.GetKey(KeyCode.Mouse0))
        {
            mouseLock = true;
        }
        if (mouseLock)
        {
            if (mousePos.y > Screen.height - borderY)
            {
                this.transform.position += Time.deltaTime * (xSpeed + transform.position.y/2) * new Vector3(transform.up.x, 0, transform.up.z).normalized;
            }
            if (mousePos.y < borderY)
            {
                this.transform.position -= Time.deltaTime * (xSpeed + transform.position.y/2) * new Vector3(transform.up.x, 0, transform.up.z).normalized;
            }
            if (mousePos.x > Screen.width - borderX)
            {
                this.transform.position += Time.deltaTime * (xSpeed + transform.position.y / 2) * new Vector3(transform.right.x, 0, transform.right.z).normalized;
            }
            if (mousePos.x < borderX)
            {
                this.transform.position -= Time.deltaTime * (xSpeed + transform.position.y / 2) * new Vector3(transform.right.x, 0, transform.right.z).normalized;
            }
        }
    }
}
