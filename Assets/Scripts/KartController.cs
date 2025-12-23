using UnityEngine;

public class KartController: MonoBehaviour
{
    [Header("Movimento")]
    public float moveSpeed = 10f;      // aceleração
    public float maxSpeed = 15f;       // velocidade máxima
    public float steerAngle = 120f;    // quanto gira por segundo

    [Header("Tração")]
    public float tractionNormal = 6f;   // aderência normal (alto = gruda na pista)
    public float tractionDrift = 1.5f;  // aderência em drift (baixo = escorrega)

    [Header("Rodas da frente (visuais)")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public float wheelSteerAngle = 30f; // quanto a roda pode girar para os lados

    Vector3 moveForce;   // vetor de movimento / velocidade atual

    bool drifting;       // está em modo drift?
    int driftSide = 0;   // -1 = esquerda, 1 = direita, 0 = nenhum

    void Update()
    {
        // entrada de drift
        bool driftKey = Input.GetKey(KeyCode.Space);

        if (driftKey && !drifting)
        {
            drifting = true;

            float hEnter = Input.GetAxis("Horizontal");

            if (Mathf.Abs(hEnter) > 0.1f)
                driftSide = hEnter > 0 ? 1 : -1;
            else
                driftSide = 1; // padrão: direita
        }

        if (!driftKey && drifting)
        {
            drifting = false;
            driftSide = 0;
            // aqui depois dá pra aplicar nitro
        }

        // aceleração e freio
        float v = Input.GetAxis("Vertical");   // W/S ou setas

        bool hasThrottle = Mathf.Abs(v) > 0.01f;

        if (hasThrottle)
        {
            // só gera velocidade se estiver acelerando
            moveForce += transform.forward * v * moveSpeed * Time.deltaTime;
        }
        else
        {
            // freio natural quando não está acelerando
            moveForce = Vector3.Lerp(
                moveForce,
                Vector3.zero,
                2f * Time.deltaTime
            );
        }

        moveForce = Vector3.ClampMagnitude(moveForce, maxSpeed);

        // steering (virar)
        float h = Input.GetAxis("Horizontal");
        bool hasSteer = Mathf.Abs(h) > 0.01f;

        float speedFactor = moveForce.magnitude / maxSpeed; // 0–1

        // só gira se: está com velocidade, tem input horizontal e está acelerando
        if (speedFactor > 0.01f && hasSteer && hasThrottle)
        {
            float steerInput;
            float steerMult = 1f;

            if (drifting && driftSide != 0)
            {
                steerInput = driftSide;

                float sideAmount = Mathf.Clamp01(driftSide * h);
                steerMult = 0.5f + sideAmount;
            }
            else
            {
                steerInput = h;
            }

            float steer = steerInput * steerAngle * steerMult * speedFactor * Time.deltaTime;
            transform.Rotate(0f, steer, 0f);
        }

        // giro das rodas (é mais de bonito msm)
        float wheelAngle = hasThrottle ? h * wheelSteerAngle : 0f;

        if (frontLeftWheel != null)
        {
            Vector3 e = frontLeftWheel.localEulerAngles;
            e.y = wheelAngle;
            frontLeftWheel.localEulerAngles = e;
        }

        if (frontRightWheel != null)
        {
            Vector3 e = frontRightWheel.localEulerAngles;
            e.y = wheelAngle;
            frontRightWheel.localEulerAngles = e;
        }

        // tração
        float currentTraction = drifting ? tractionDrift : tractionNormal;

        Vector3 desired = transform.forward * moveForce.magnitude;
        moveForce = Vector3.Lerp(
            moveForce,
            desired,
            currentTraction * Time.deltaTime
        );

        // aplicar movimento
        transform.position += moveForce * Time.deltaTime;
    }
}
