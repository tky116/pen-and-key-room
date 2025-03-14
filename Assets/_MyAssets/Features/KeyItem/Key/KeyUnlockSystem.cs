using UnityEngine;

public class KeyUnlockSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MetaQuest3Input inputManager;
    [SerializeField] private KeyLockGuideSystem guideSystem;
    [SerializeField] private TriggerLoadScene sceneLoadTrigger;

    [Header("Settings")]
    [SerializeField] private float unlockRotationThreshold = 15f;  // 解錠に必要な回転角度
    [SerializeField] private bool showDebugLogs = true;

    [Header("Audio")]
    [SerializeField] private AudioClip insertSound;
    [SerializeField] private AudioClip unlockSound;

    private AudioSource audioSource;
    private bool isInserted = false;
    private bool isUnlocked = false;
    private Transform keyTransform;
    private float lastYAngle;
    private float totalRotation = 0f;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isUnlocked) return;

        if (other.CompareTag("KeyBlade"))
        {
            keyTransform = other.transform.parent;
            if (keyTransform == null)
            {
                Debug.LogError("KeyBlade must have a parent object!");
                return;
            }

            isInserted = true;
            lastYAngle = keyTransform.localEulerAngles.y;

            if (showDebugLogs)
            {
                Debug.Log($"Key inserted. Initial Y angle: {lastYAngle:F1}°");
            }

            if (insertSound && audioSource)
            {
                audioSource.PlayOneShot(insertSound);
            }

            guideSystem?.OnKeyInserted();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("KeyBlade"))
        {
            isInserted = false;
            keyTransform = null;
            totalRotation = 0f;

            if (!isUnlocked)
            {
                guideSystem?.OnKeyGrabbed();
            }
        }
    }

    private void Update()
    {
        if (!isInserted || isUnlocked || keyTransform == null) return;

        float currentYAngle = keyTransform.localEulerAngles.y;
        float deltaAngle = Mathf.DeltaAngle(lastYAngle, currentYAngle);

        if (Mathf.Abs(deltaAngle) > 1.0f)  // 小さすぎる回転は無視
        {
            totalRotation += Mathf.Abs(deltaAngle);
            ProvideRotationHapticFeedback();  // 回転時の振動

            if (showDebugLogs)
            {
                Debug.Log($"Rotation: {deltaAngle:F1}°, Total: {totalRotation:F1}°");
            }

            if (totalRotation >= unlockRotationThreshold)
            {
                Unlock();
            }
        }

        lastYAngle = currentYAngle;
    }

    private void Unlock()
    {
        if (isUnlocked) return;

        isUnlocked = true;
        ProvideUnlockHapticFeedback();

        if (unlockSound && audioSource)
        {
            audioSource.PlayOneShot(unlockSound);
        }

        guideSystem?.OnKeyUnlocked();

        if (sceneLoadTrigger != null)
        {
            sceneLoadTrigger.LoadScene();
        }
        else
        {
            Debug.LogWarning("シーン遷移用の TriggerLoadScene がアタッチされていません。");
        }
    }

    private void ProvideUnlockHapticFeedback()
    {
        // 鍵を開けた時の強めの振動
        if (inputManager != null)
        {
            inputManager.LeftQuestController?.SendHapticImpulse(0.3f, 0.3f);
            inputManager.RightQuestController?.SendHapticImpulse(0.3f, 0.3f);
        }
    }

    private void ProvideRotationHapticFeedback()
    {
        // 回転中の軽い振動
        if (inputManager != null)
        {
            float intensity = 0.1f;
            float duration = 0.1f;
            inputManager.LeftQuestController?.SendHapticImpulse(intensity, duration);
            inputManager.RightQuestController?.SendHapticImpulse(intensity, duration);
        }
    }
}
