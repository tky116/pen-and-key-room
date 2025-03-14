using UnityEngine;
using System;
using System.Linq;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(CharacterController))]
public class FootstepController : MonoBehaviour
{
    [Serializable]
    public class FootstepSoundSet
    {
        public PhysicsMaterial material;
        public AudioClip[] sounds;

        [Range(0.1f, 2.0f)]
        public float volumeMultiplier = 1.0f;
    }

    [Header("Sound Settings")]
    [SerializeField] private AudioClip[] defaultFootstepSounds;

    [SerializeField] private AudioClip[] slidingFootstepSounds;
    [SerializeField] private FootstepSoundSet[] footstepSoundSets;
    [SerializeField] private float baseFootstepVolume = 1f;
    [SerializeField] private float minTimeBetweenSteps = 0.3f;

    [Header("Movement Settings")]
    [SerializeField] private float movementThreshold = 0.1f;

    [SerializeField] private float maxSpeed = 3.0f;
    [SerializeField] private float raycastDistance = 1.5f;
    [SerializeField] private LayerMask groundLayer = -1;

    [Header("Impact Settings")]
    [SerializeField] private float minImpactThreshold = 0.1f;

    [SerializeField] private float maxImpactForce = 2.0f;
    [SerializeField] private float impactVolumeMultiplier = 1.5f;

    [Header("Slope Settings")]
    [SerializeField] private float minSlopeAngle = 20f;

    [SerializeField] private float maxSlopeAngle = 50f;
    [SerializeField] private float slidingSlopeAngle = 30f;
    [SerializeField] private float maxSlopeVolumeMultiplier = 1.3f;
    [SerializeField] private float slidingSoundInterval = 0.5f;

    [Header("Stereo Settings")]
    [SerializeField] private float footstepPanStrength = 0.2f;

    [Header("Dynamic Volume Settings")]
    [SerializeField] private bool useDynamicVolume = true;

    [SerializeField] private float minVolumeMultiplier = 0.5f;
    [SerializeField] private float maxVolumeMultiplier = 1.5f;

    private AudioSource audioSource;
    private OVRPlayerController playerController;
    private CharacterController characterController;
    private float timeSinceLastStep;
    private bool isSliding;
    private bool isLeftFoot = true;
    private float previousYVelocity;
    private float currentImpactStrength;
    private Vector3 lastPosition;

    private void Start()
    {
        InitializeComponents();
        SetupAudioSource();
        lastPosition = transform.position;
    }

    private void InitializeComponents()
    {
        audioSource = GetComponent<AudioSource>();
        playerController = GetComponent<OVRPlayerController>();
        characterController = GetComponent<CharacterController>();

        if (playerController == null)
        {
            Debug.LogError("OVRPlayerController not found on the same GameObject!");
            enabled = false;
            return;
        }

        previousYVelocity = 0f;
        currentImpactStrength = 0f;
    }

    private void SetupAudioSource()
    {
        audioSource.spatialize = true;
        audioSource.spatialBlend = 1.0f;
        audioSource.volume = baseFootstepVolume;
        audioSource.playOnAwake = false;
        audioSource.panStereo = 0f;
    }

    private void OnDisable()
    {
        if (isSliding)
        {
            isSliding = false;
            CancelInvoke(nameof(PlaySlidingSound));
        }
    }

    private void Update()
    {
        // フレームレート非依存の移動速度計算
        Vector3 currentPosition = transform.position;
        Vector3 movement = currentPosition - lastPosition;
        float movementSpeed = movement.magnitude / Time.deltaTime;
        lastPosition = currentPosition;

        timeSinceLastStep += Time.deltaTime;

        // 衝撃力の計算
        float yVelocity = characterController.velocity.y;
        float velocityDelta = yVelocity - previousYVelocity;
        currentImpactStrength = Mathf.Abs(velocityDelta);
        previousYVelocity = yVelocity;

        CheckSliding();

        if (movementSpeed > movementThreshold && timeSinceLastStep >= GetDynamicStepInterval(movementSpeed))
        {
            PlayFootstepSound(movementSpeed, currentImpactStrength);
            timeSinceLastStep = 0f;
        }
    }

    private void CheckSliding()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.2f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, raycastDistance, groundLayer))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            if (slopeAngle > slidingSlopeAngle && !isSliding)
            {
                isSliding = true;
                InvokeRepeating(nameof(PlaySlidingSound), 0f, slidingSoundInterval);
            }
            else if (slopeAngle <= slidingSlopeAngle && isSliding)
            {
                isSliding = false;
                CancelInvoke(nameof(PlaySlidingSound));
            }
        }
        else if (isSliding)
        {
            isSliding = false;
            CancelInvoke(nameof(PlaySlidingSound));
        }
    }

    private float GetDynamicStepInterval(float speed)
    {
        float speedFactor = Mathf.Clamp01(speed / maxSpeed);
        float interval = minTimeBetweenSteps / (speedFactor + 0.5f);

        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.2f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, raycastDistance, groundLayer))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (slopeAngle > minSlopeAngle)
            {
                float slopeFactor = Mathf.Clamp01((slopeAngle - minSlopeAngle) / (maxSlopeAngle - minSlopeAngle));
                interval *= Mathf.Lerp(1f, 0.7f, slopeFactor);
            }
        }

        return interval;
    }

    private void PlaySlidingSound()
    {
        if (slidingFootstepSounds.Length > 0)
        {
            AudioClip slideClip = slidingFootstepSounds[UnityEngine.Random.Range(0, slidingFootstepSounds.Length)];
            float currentVolume = baseFootstepVolume * 0.7f;
            audioSource.PlayOneShot(slideClip, currentVolume);
        }
    }

    private void PlayFootstepSound(float currentSpeed, float impactStrength)
    {
        AudioClip clip = GetSurfaceFootstepSound(out float materialVolumeMultiplier);
        if (clip == null) return;

        float speedFactor = useDynamicVolume
            ? Mathf.Clamp01(currentSpeed / maxSpeed)
            : 1f;

        float normalizedImpact = Mathf.Clamp01((impactStrength - minImpactThreshold) / maxImpactForce);
        float impactMultiplier = Mathf.Lerp(1.0f, impactVolumeMultiplier, normalizedImpact);

        float dynamicMultiplier = Mathf.Lerp(minVolumeMultiplier, maxVolumeMultiplier, speedFactor) * impactMultiplier;
        float finalVolume = baseFootstepVolume * dynamicMultiplier * materialVolumeMultiplier;

        audioSource.panStereo = isLeftFoot ? -footstepPanStrength : footstepPanStrength;
        isLeftFoot = !isLeftFoot;

        audioSource.PlayOneShot(clip, finalVolume);
    }

    private AudioClip GetSurfaceFootstepSound(out float volumeMultiplier)
    {
        volumeMultiplier = 1f;
        RaycastHit hit;

        Vector3 rayOrigin = transform.position + Vector3.up * 0.2f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, raycastDistance, groundLayer))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            if (slopeAngle > minSlopeAngle)
            {
                float slopeFactor = Mathf.Clamp01((slopeAngle - minSlopeAngle) / (maxSlopeAngle - minSlopeAngle));
                float slopeVolumeMultiplier = Mathf.Lerp(1.0f, maxSlopeVolumeMultiplier, slopeFactor);
                volumeMultiplier *= slopeVolumeMultiplier;
            }

            var physicMaterial = hit.collider.sharedMaterial;
            if (physicMaterial != null)
            {
                var soundSet = footstepSoundSets.FirstOrDefault(set => set.material == physicMaterial);
                if (soundSet != null && soundSet.sounds.Length > 0)
                {
                    volumeMultiplier *= soundSet.volumeMultiplier;
                    return soundSet.sounds[UnityEngine.Random.Range(0, soundSet.sounds.Length)];
                }
            }
        }

        return defaultFootstepSounds.Length > 0
            ? defaultFootstepSounds[UnityEngine.Random.Range(0, defaultFootstepSounds.Length)]
            : null;
    }
}
