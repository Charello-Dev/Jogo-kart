using UnityEngine;

public class KartController : MonoBehaviour
{
    [Header("Movimento")]
    public float acceleration = 10f;
    public float moveSpeed; 
    public float maxSpeed = 15f;

    [Header("Drift")]
    private int driftDirection = 0;  // -1 esquerda, 1 direita, 0 nenhum
    public float driftSideForce = 8f;      
    public float driftCorrection = 0.5f;
    public float driftCorrectionForce = 4f; 

    private Vector3 driftVelocity = Vector3.zero; 

    public float driftLockForce = 3f; 

    private bool isDrifting = false;

    [Header("Nitro")]
    public float[] nitroSpeeds = { 18f, 22f, 28f }; // 3 níveis de nitro
    public int nitroLevel = 0;                     // 0 - 2
    private float nitroCharge = 0f;                 // 0 - 100%
    public float driftChargeRate = 0.1f;              
    public float nitroDrainRate = 0.7f;            
    private bool nitroActive = false;              

    // inputs
    private float accelerationInput;
    private float steeringInput;

    void Update() 
    {
        #region move
        // pega inputs
        accelerationInput = Input.GetAxis("Vertical");
        steeringInput = Input.GetAxis("Horizontal");
        
        // atualiza velocidade
        moveSpeed += accelerationInput * acceleration * Time.deltaTime;
        
        // freio e desaceleração natural
        if (accelerationInput < 0) moveSpeed *= 0.95f; // freia mais rápido
        else if (accelerationInput == 0) moveSpeed *= 0.98f; // desacelera devagar


        moveSpeed = Mathf.Clamp(moveSpeed, 0, maxSpeed);
        #endregion

        #region nitro management
        
        // input drift
        bool wantsToDrift = Input.GetKey(KeyCode.Space);
        
        // inicia drift
        if (wantsToDrift && Mathf.Abs(steeringInput) > 0.5f && driftDirection == 0) {
            driftDirection = (int)Mathf.Sign(steeringInput); // TRVA a direção!
            isDrifting = true;
        }
        
        // sai do drift
        if (!wantsToDrift) {
            driftDirection = 0;
            isDrifting = false;
        }
        
        // inicia fricção reduzida durante o drift
        if (isDrifting) moveSpeed *= 0.98f;

        // carrega nitro com drift
        if (isDrifting && driftDirection != 0) {
            nitroCharge += driftChargeRate * Time.deltaTime;
            nitroCharge = Mathf.Clamp(nitroCharge, 0, 100); // 0 - 100%
        }
        #endregion

        #region nitro system
        if (isDrifting) {
            // carrega nitro durante o drift
            nitroCharge += driftChargeRate * Time.deltaTime;
            nitroCharge = Mathf.Clamp01(nitroCharge);

            // atualiza nível de nitro
            if (nitroCharge >= 0.66f) nitroLevel = 3;
            else if (nitroCharge >= 0.33f) nitroLevel = 2;
            else nitroLevel = 1;
        }

        // ativa nitro
        if (!Input.GetKey(KeyCode.Space) && nitroCharge > 0.1f && !nitroActive) {
            nitroActive = true;
        }

        // gasta nitro quando ativo
        if (nitroActive) {
            nitroCharge -= nitroDrainRate * Time.deltaTime;
            if (nitroCharge <= 0f) {
                nitroLevel = 0;
                nitroActive = false;
                nitroCharge = 0f;
            }
        }
        #endregion
    }

    void FixedUpdate() {

        //velocidade final (com nitro)
        float finalSpeed = moveSpeed;
        if (nitroActive && nitroLevel > 0) {
            finalSpeed = nitroSpeeds[nitroLevel - 1]; // nível 1 - 3
        }

        // movimento normal (frente)
        transform.Translate(Vector3.forward * finalSpeed * Time.deltaTime);

        #region drift mechanics
        if (moveSpeed > 1f && isDrifting && driftDirection != 0) {
            
            // aplica força lateral do drift
            Vector3 driftForce = transform.right * (driftDirection * driftSideForce);
            driftVelocity += driftForce * Time.deltaTime;
            
            // correção com input oposto
            Vector3 correctionForce = transform.right * (steeringInput * driftCorrectionForce * 0.5f);
            driftVelocity += correctionForce * Time.deltaTime;
            
            // velocidade lateral do drift
            transform.Translate(driftVelocity * Time.deltaTime, Space.World);
            
            // fricção do drift
            driftVelocity *= 0.95f;
            
            // rotação durante o drift
            float steerAmount = (driftDirection * driftLockForce) + (steeringInput * driftCorrection);
            transform.Rotate(Vector3.up, steerAmount * Time.deltaTime * 50f);
            
        } else {
            // normal (sem drift)
            driftVelocity *= 0.9f; // limpa driftVelocity rápido
            
            float steerAmount = steeringInput * (moveSpeed / maxSpeed) * 2f;
            transform.Rotate(Vector3.up, steerAmount * Time.deltaTime * 50f);
        }
        #endregion
    }
}
