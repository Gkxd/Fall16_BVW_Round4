﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Select (Map), Aggregate (Reduce), Where (Filter)

public class PlayerController : MonoBehaviour
{

    public CubemanController controller;
    public float maxSpeed;
    public float jumpStrength;
    public float jumpDelay;

    private new Rigidbody rigidbody;

    private Transform head;
    private Transform hip;
    private Transform footL;
    private Transform footR;

    private float speed;
    private float angle;
    private float buttonSpin;
    private float lastJumpTime;

    public Transform tiltAnchor;
    public Transform buttonAnchor;

    private Queue<float> prevHeights;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        head = controller.Head.transform;
        hip = controller.Hip_Center.transform;
        footL = controller.Foot_Left.transform;
        footR = controller.Foot_Right.transform;
        prevHeights = new Queue<float>();
    }

    void Update()
    {
        Debug.DrawLine(hip.position, head.position, Color.blue);
        Vector3 lean = head.position - hip.position;

        Vector3 leanDirection = Vector3.ProjectOnPlane(lean, Vector3.up);
        Debug.DrawRay(hip.position, leanDirection, Color.red);

        float xAxis = leanDirection.x;
        float zAxis = leanDirection.z;

        float height = hip.position.y;
        if (prevHeights.Count < 60)
        {
            prevHeights.Enqueue(height);
        }
        else
        {
            float avgPrevHeight = prevHeights.Sum() / prevHeights.Count();
            
            if (height - avgPrevHeight > 0.1f)
            {
                if (Time.time - lastJumpTime > jumpDelay)
                {
                    rigidbody.velocity += jumpStrength * Vector3.up;
                    lastJumpTime = Time.time;
                }
            }

            prevHeights.Enqueue(height);
            prevHeights.Dequeue();
        }
        
        // Tilt the button when turning
        float leanAngle = (xAxis < 0 ? 1 : -1) * Vector3.Angle(Vector3.up, Vector3.ProjectOnPlane(lean, Vector3.forward));
        tiltAnchor.localEulerAngles = new Vector3(0, 0, leanAngle);

        if (xAxis < -0.2f)
        {
            angle = (angle - 30 * Time.deltaTime) % 360;
        }
        else if (xAxis > 0.2f)
        {
            angle = (angle + 30 * Time.deltaTime) % 360;
        }
        else if (zAxis < -0.1f)
        {
            speed = Mathf.Min(maxSpeed, speed + 0.5f * Time.deltaTime);
        }
        else if (zAxis > 0.2f)
        {
            speed = Mathf.Max(-maxSpeed, speed - 0.5f * Time.deltaTime);
        }
        else
        {
            speed = Mathf.Lerp(speed, 0, Time.deltaTime);
        }

        // Spin the button based on the speed
        buttonSpin = (buttonSpin + 360 * speed * Time.deltaTime) % 360;
        buttonAnchor.localEulerAngles = Vector3.right * buttonSpin;

        // Set the speed of the button
        Vector3 gravity = Vector3.Project(rigidbody.velocity, Vector3.down);
        transform.eulerAngles = Vector3.up * angle;
        rigidbody.velocity = Quaternion.Euler(0, angle, 0) * Vector3.forward * speed + gravity;
    }
}
