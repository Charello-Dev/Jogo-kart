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
    [SerializeField] private float forwardSpeedNormal;      // velocidade para frente normal
    [SerializeField] private float forwardSpeedBrake;       // velocidade para frente freiando
    private float forwardSpeedCurrent;                      // velocidade atual para frente

    [Header("Reverse speeds")]
    [SerializeField] private float reverseSpeedNormal;      // velocidade para trás normal
    [SerializeField] private float reverseSpeedBrake;       // velocidade para trás freiando
    private float reverseSpeedCurrent;                      // velocidade atual para trás

    [Header("Turn speed")]
    [SerializeField] private float turnSpeed;

    [Header("Drag")]
    [SerializeField] private float dragNormal; 
    [SerializeField] private float dragBrake; 

    [Header("Wheels")]
    [SerializeField] private float maxSteeringAngle;        // angulo máximo para virar as rodas dianteiras

    [Space(10)]

    [SerializeField] private Transform frontLeftWheel;      // roda dianteira esquerda
    [SerializeField] private Transform frontRightWheel;     // roda dianteira direita

    [Header("Wheels Effects")]
    [SerializeField] private TrailRenderer wheelEffectLeft;     // trail render da roda traseira esquerda
    [SerializeField] private TrailRenderer wheelEffectRight;    // trail render da roda traseira direita

    [Header("Tilt")]
    [SerializeField] private float tiltSpeed;           // velocidade para inclinar o kart
    [SerializeField] private float groundCheckRaius;    // tamanho do raycast de verificar o chão
    [SerializeField] private Transform GroundCheck01;   // raycast dianteiro, para verificar o chão
    [SerializeField] private Transform GroundCheck02;   // raycast traseiro, para verificar o chão
    [SerializeField] private LayerMask groundLayer;     // layer do chão
    private RaycastHit goundHitPoint;                   // ponto do chão q tá pegando o raycast
    private bool isGrounded;                            // booleana pra verificar se está no chão

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    void Start() {
        forwardSpeedCurrent = forwardSpeedNormal;
        reverseSpeedCurrent = reverseSpeedNormal;

        rb.linearDamping = dragNormal;

        WheelsEffects_Deactivated();
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

    // aplica a gravidade, para n ter q mudar no projeto
    private void ApplyGravity() {
        if (!isGrounded) {
            rb.AddForce(-Vector3.up * gravityForce);
        }
    }

    // recebe inputs do jogador
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

    // vira as rodas frontais
    private void TurnWheels() {
        float newSteeringAngle = inputTurn * maxSteeringAngle;

        // roda da esquerda
        frontLeftWheel.localRotation = Quaternion.Euler(frontLeftWheel.localRotation.eulerAngles.x, newSteeringAngle, frontLeftWheel.localRotation.eulerAngles.z);
        
        // roda da direita
        frontRightWheel.localRotation = Quaternion.Euler(frontRightWheel.localRotation.eulerAngles.x, newSteeringAngle, frontRightWheel.localRotation.eulerAngles.z);
    }

    // inclina o kart baseado no chão
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

    // movimentação do kart (frente e ré)
    private void MoveKart() {
        if (inputMove > 0f) {
            rb.AddForce(transform.forward * forwardSpeedCurrent * inputMove, ForceMode.Acceleration);
        }
        else if (inputMove < 0f) {
            rb.AddForce(transform.forward * reverseSpeedCurrent * inputMove, ForceMode.Acceleration);
        }
    }

    // ativa o modo freio/drift 
    private void Brake() {
        forwardSpeedCurrent = forwardSpeedBrake;
        reverseSpeedCurrent = reverseSpeedBrake;

        rb.linearDamping = dragBrake;

        if (isGrounded) {
            WheelsEffects_Activated();
        }
        else {
            WheelsEffects_Deactivated();
        }
    }

    // desativa o modo freio/drift
    private void UnBrake() {
        forwardSpeedCurrent = forwardSpeedNormal;
        reverseSpeedCurrent = reverseSpeedNormal;

        rb.linearDamping = dragNormal;

        WheelsEffects_Deactivated();
    }

    // ativa os efeitos de pneus durante o drift
    private void WheelsEffects_Activated() {
        wheelEffectLeft.emitting = true;
        wheelEffectRight.emitting = true;
    }

    // desativa os efeitos de pneus durante o drift
    private void WheelsEffects_Deactivated() {
        wheelEffectLeft.emitting = false;
        wheelEffectRight.emitting = false;
    }

    // vira o lart (esquerda/direita)
    private void TurnKart() {
        float newTurn = inputTurn * turnSpeed * inputMove * Time.fixedDeltaTime;
        transform.Rotate(0f, newTurn, 0f, Space.World);
    }
}
