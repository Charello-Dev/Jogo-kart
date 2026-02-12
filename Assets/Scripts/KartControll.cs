using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KartControll : MonoBehaviour
{

    [Header("general references")]
    private Rigidbody rb;
    [SerializeField] private float gravityForce;

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

    [Header("Tilt")]
    [SerializeField] private float tiltSpeed;
    [SerializeField] private float groundCheckRaius;
    [SerializeField] private Transform GroundCheck01;
    [SerializeField] private Transform GroundCheck02;
    [SerializeField] private LayerMask groundLayer;
    private RaycastHit goundHitPoint;
    private bool isGrounded;

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
        TiltRamp();
        TurnKart();
    }

    private void FixedUpdate() {
        ApplyGravity();
        MoveKart();
    }

    private void ApplyGravity() {
        if (!isGrounded) {
            rb.AddForce(-Vector3.up * gravityForce);
        }
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

    private void TiltRamp() {
        Vector3 normalGround = Vector3.zero;

        if (Physics.Raycast(GroundCheck01.position, -transform.up, out goundHitPoint, groundCheckRaius, groundLayer)) {
            isGrounded = true;
            normalGround = goundHitPoint.normal;
        }
        else {
            isGrounded = false;
            normalGround = Vector3.zero;
        }

        if (Physics.Raycast(GroundCheck02.position, -transform.up, out goundHitPoint, groundCheckRaius, groundLayer)) {
            isGrounded = true;
            normalGround = (normalGround + goundHitPoint.normal) / 2f;
        }

        if (isGrounded) {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(transform.up, normalGround) * transform.rotation, tiltSpeed * Time.deltaTime);
        }
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
