using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class main_character : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Collider _collider;
    private Animator _animator;
    public GameObject mainStatus;
    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        bool isStopMoving = mainStatus.GetComponent<main_status>().isStopMoving;
        if (isStopMoving)
        {
            _animator.speed = 0;
            return;
        } else
        {
            _animator.speed = 1;
        }
        getMovingCommand();
        directlyMoving();
    }

    public Terrain terrain;
    private bool isMoving = false;
    private Vector3 destination;
    private float movingMargin = .5f;
    private const float rotatingSpeed = 3f;
    public float speed = 1.0f;

    public float movingHeight = 10f;
    private void getMovingCommand()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                Debug.Log(hitInfo.point.y);
                if (hitInfo.point.y - movingHeight <= 0.1f && hitInfo.point.y - movingHeight >= -0.1f)
                {
                    destination = hitInfo.point;
                    isMoving = true;
                    _animator.SetBool("Walk", true);
                    movingMargin = .5f;
                    //_animator.SetFloat("Speed", 2.0f);
                    Debug.Log(_animator.GetBool("Walk"));
                    //Debug.Log(_animator.GetFloat("Speed"));

                    isJustStopMoving = () => { };
                }
            }
        }
    }

    Action isJustStopMoving;
    private void directlyMoving() {
        if (!isMoving)
        {
            return;
        }
        if ((destination - transform.position).magnitude < movingMargin)
        {
            isMoving = false;
            _animator.SetBool("Walk", false);
            Debug.Log("stop Running");
            isJustStopMoving();
        }
        if (isMoving && destination.y - movingHeight <= 0.1f && destination.y - movingHeight >= -0.1f)
        {
            //Debug.Log("isMoving");
            // Determine which direction to rotate towards
            Vector3 distance = destination - transform.position;

            // The step size is equal to speed times frame time.
            float singleStep = rotatingSpeed * Time.deltaTime;

            // Rotate the forward vector towards the target direction by one step
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, distance, singleStep, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDirection);

            Vector3 deltaV = distance.normalized *
                speed * Time.deltaTime;
            // 如果在这一帧已经可以到达

            if (deltaV.magnitude > (destination - transform.position).magnitude)
            {
                Debug.Log("Thats bug");
                transform.position = destination;
                isMoving = false;
                _animator.SetBool("Walk", false);
            }
            Vector3 v = deltaV + transform.position;
            //Debug.Log(terrain.terrainData.GetHeight((int)v[0], (int)v[1]));
            //Debug.Log(deltaV * 1000);
            //Debug.Log("position:");
            //Debug.Log(transform.position);
            //Debug.Log(v);
            //Debug.Log(deltaV);
            float terrainHeight = terrain.terrainData.GetHeight((int)v[0], (int)v[2]);
            //Debug.Log("terrainHeight:");
            //Debug.Log(terrainHeight);
            //Debug.Log(deltaV.magnitude * 1000);
            if (terrainHeight - movingHeight <= 0.1f && terrainHeight - movingHeight >= -0.1f)
            {
                //Debug.Log("isTransforming");
                v.y = terrain.terrainData.GetHeight((int)v[0], (int)v[2]);
                transform.position = v;
            } else
            {
                isMoving = false;
            }
        }
    }

    /**
     * @brief: let main character move towards target destination, 如果到达 margin 内则无需继续前进
     * 
     * @param margin: 
     */
    public void movingWithMargin(float margin, Vector3 __destination, Action __isJustStopMoving)
    {
        destination = __destination;
        movingMargin = margin;
        isMoving = true;
        _animator.SetBool("Walk", true);
        isJustStopMoving = __isJustStopMoving;
    }
}
