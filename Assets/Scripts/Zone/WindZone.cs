using UnityEngine;

// ============================================================
//  WindZone.cs  (3D)
//  Attach to a GameObject with a 3D Collider (set Is Trigger = true).
//  Ball must have a Rigidbody and tag "Ball".
//
//  HOW TO SET UP:
//  1. Create empty GameObject, name it "WindZone"
//  2. Add BoxCollider -> check Is Trigger -> resize to cover wind area
//  3. Add this script
//  4. Set Wind Direction and Wind Strength in Inspector
//  5. Optionally add a Particle System child for visual arrows/dust
// ============================================================

namespace BasketRobbins
{
    public class WindZone : MonoBehaviour
    {
        private Rigidbody ballInside;

        [Space(20f)]
        [Header("assign Objects")]
        [SerializeField] private BoxCollider boxCollider;
        [SerializeField] private ParticleSystem windParticles;

        [Space(20f)]
        [Header("Wind Power")]
        public float minWindStrength = 1f;
        public float maxWindStrength = 10f;

        [HideInInspector] public float windPower = 0.1f;              

        [Tooltip("Force applied to ball per second (try 5-15)")]
        public float windStrength => Mathf.Lerp(minWindStrength, maxWindStrength, windPower);

        private void OnEnable()
        {
            GameManager.OnSimulationStart_Action += SimulationStart;
            GameManager.OnSimulationStop_Action += SimulationStop;
        }

        private void OnDisable()
        {
            GameManager.OnSimulationStart_Action -= SimulationStart;
            GameManager.OnSimulationStop_Action -= SimulationStop;
        }

        void SimulationStart()
        {
            boxCollider.isTrigger = true;
        }

        void SimulationStop()
        {
            boxCollider.isTrigger = false;
        }

        public void SetWindPower(float value)
        {
            ParticleSystem.EmissionModule emission = windParticles.emission;
            float emissionRate;
            float newPower = Mathf.Clamp01(value);
            if (!Mathf.Approximately(newPower, windPower))
            {
                windPower = newPower;

                emissionRate = Mathf.Lerp(10, 100, windPower);
                emission.rateOverTime = emissionRate;
            }
        }

        private void FixedUpdate()
        {
            if (ballInside == null) return;
            Vector3 currentWindDir = transform.TransformDirection(Vector3.forward);
            ballInside.AddForce(currentWindDir * windStrength, ForceMode.Force);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(GameConstants.Tag.Ball))
                ballInside = other.GetComponent<Rigidbody>();
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(GameConstants.Tag.Ball))
                ballInside = null;
        }

        /*private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.35f);
            Collider col = GetComponent<Collider>();
            if (col != null) Gizmos.DrawWireCube(transform.position, col.bounds.size);
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, windDirection.normalized * 2f);
        }*/
    }
}

