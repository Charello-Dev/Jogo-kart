using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KartControll : MonoBehaviour
{

    [Header("general references")]
    private Rigidbody rb;

    [Header("Inputs")]
    private float inputMove;
    private float inputTurn;
    private bool inputDrift;

    [Header("Forward speeds")]
    [SerializeField] private float forwardSpeedNormal;
    [SerializeField] private float forwardSpeedBrake;
    private float forwardSpeedCurrent;

    [Header("Reverse speeds")]
    [SerializeField] private float reverseSpeedNormal;
    [SerializeField] private float reverseSpeedBrake;
    private float reverseSpeedCurrent;

    [Header("Turn speed")]
    [SerializeField] private float turnSpeed;

    [Header("Drag")]
    [SerializeField] private float dragNormal;
    [SerializeField] private float dragBrake;

    [Header("Wheels")]
    [SerializeField] private float maxSteeringAngle;

    [Space(10)]
    
    [SerializeField] private Transform frontLeftWheel;
    [SerializeField] private Transform frontRightWheel;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    void Start() {
        forwardSpeedCurrent = forwardSpeedNormal;
        reverseSpeedCurrent = reverseSpeedNormal;

        rb.linearDamping = dragNormal;
    }


    void Update() {
        Inputs();
        TurnWheels();
        TurnKart();
    }

    private void FixedUpdate() {
        MoveKart();
    }

    private void Inputs() {
        inputMove = Input.GetAxisRaw("Vertical");
        inputTurn = Input.GetAxisRaw("Horizontal");

        inputDrift = Input.GetButton("Drift");

        if (inputDrift) {
            Brake();
        }
        else {
            UnBrake();
        }
    }

    private void TurnWheels() {
        float newSteeringAngle = inputTurn * maxSteeringAngle;

        // roda da esquerda
        frontLeftWheel.localRotation = Quaternion.Euler(frontLeftWheel.localRotation.eulerAngles.x, newSteeringAngle, frontLeftWheel.localRotation.eulerAngles.z);
        
        // roda da direita
        frontRightWheel.localRotation = Quaternion.Euler(frontRightWheel.localRotation.eulerAngles.x, newSteeringAngle, frontRightWheel.localRotation.eulerAngles.z);
    }

    private void MoveKart() {
        if (inputMove > 0f) {
            rb.AddForce(transform.forward * forwardSpeedCurrent * inputMove, ForceMode.Acceleration);
        }
        else if (inputMove < 0f) {
            rb.AddForce(transform.forward * reverseSpeedCurrent * inputMove, ForceMode.Acceleration);
        }
    }

    private void Brake() {
        forwardSpeedCurrent = forwardSpeedBrake;
        reverseSpeedCurrent = reverseSpeedBrake;

        rb.linearDamping = dragBrake;
    }

    private void UnBrake() {
        forwardSpeedCurrent = forwardSpeedNormal;
        reverseSpeedCurrent = reverseSpeedNormal;

        rb.linearDamping = dragNormal;
    }

    private void TurnKart() {
        float newTurn = inputTurn * turnSpeed * inputMove * Time.fixedDeltaTime;
        transform.Rotate(0f, newTurn, 0f, Space.World);
    }
}
