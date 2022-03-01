using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class main_camera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

        // Update is called once per frame
        void Update()
    {
        moveForScreenEdge();
    }

    public float edgeMovingSpeed = 0.1f;
    public float zoneLeft = 0f;
    public float zoneRight = 100f;
    public float zoneBottom = -30f;
    public float zoneTop = 40f;
    void moveForScreenEdge()
    {
        float mouseX = Input.mousePosition.x;
        float mouseY = Input.mousePosition.y;
        float xSpeed = 0.0f;
        float zSpeed = 0.0f;

        if (mouseX <= 0 && transform.position.x >= zoneLeft)
        {
            xSpeed = -edgeMovingSpeed;
        } else if (mouseX >= Screen.width && transform.position.x <= zoneRight)
        {
            xSpeed = edgeMovingSpeed;
        }

        if (mouseY <= 0 && transform.position.z >= zoneBottom)
        {
            zSpeed = -edgeMovingSpeed;
        } else if (mouseY >= Screen.height && transform.position[2] <= zoneTop)
        {
            zSpeed = edgeMovingSpeed;
        }

        Vector3 v = new Vector3(xSpeed, 0.0f, zSpeed);
        transform.position += v;
    }

    public void moveTowards(Vector3 v)
    {
        transform.position = new Vector3(v.x, transform.position[1], v[2] - 6);
    }
}
