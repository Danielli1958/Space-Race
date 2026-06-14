using UnityEngine;

public class RocketExhaust : MonoBehaviour
{
    [Header("Particle Systems")]
    public ParticleSystem mainThruster;     // The main continuous exhaust trail
    public ParticleSystem boostThruster;    // Extra burst effect when boosting

    [Header("Emission Rates")]
    public float idleEmissionRate = 15f;    // Particles per second while just moving
    public float boostEmissionRate = 60f;   // Particles per second while boosting

    private RocketController rocket;

    void Start()
    {
        rocket = GetComponentInParent<RocketController>();

        // Start main thruster playing immediately — rocket is always moving
        if (mainThruster != null)
            mainThruster.Play();

        if (boostThruster != null)
            boostThruster.Stop();
    }

    void Update()
    {
        if (mainThruster == null) return;

        // Adjust emission rate based on boost state
        var emission = mainThruster.emission;
        emission.rateOverTime = rocket.IsBoosting ? boostEmissionRate : idleEmissionRate;

        // Fire the boost burst particle system
        if (boostThruster != null)
        {
            if (rocket.IsBoosting && !boostThruster.isPlaying)
                boostThruster.Play();
            else if (!rocket.IsBoosting && boostThruster.isPlaying)
                boostThruster.Stop();
        }
    }
}