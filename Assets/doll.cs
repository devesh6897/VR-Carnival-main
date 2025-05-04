using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class VRDollHeadRotator : MonoBehaviour
{
    [Header("Doll Settings")]
    [SerializeField] private Transform dollHead;
    [SerializeField] private float rotationSpeed = 90f; // Degrees per second
    [SerializeField] private bool isRotatingToFront = true;
    [SerializeField] private float frontAngle = 0;
    [SerializeField] private float backAngle = 180f;

    [Header("Game Timing")]
    [SerializeField] private float minGreenLightTime = 3f;
    [SerializeField] private float maxGreenLightTime = 8f;
    [SerializeField] private float minRedLightTime = 2f;
    [SerializeField] private float maxRedLightTime = 5f;

    [Header("Game Timer")]
    [SerializeField] private float gameTimeInSeconds = 120f; // 2 minutes
    [SerializeField] private TextMeshProUGUI timerText; // UI Text to display the timer
    [SerializeField] private AudioClip timerEndSound; // Sound to play when timer ends
    [SerializeField] private AudioClip timerTickSound; // Optional tick sound for last 10 seconds
    private float currentGameTime;
    private bool isTimerActive = false;

    [Header("Auto Start")]
    [SerializeField] private bool autoStartGame = true;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip greenLightSound;
    [SerializeField] private AudioClip redLightSound;
    [SerializeField] private AudioClip rotationSound;
    [SerializeField] private AudioClip elimination;
    [SerializeField] private AudioClip gunshot;

    [Header("Movement Detection")]
    [SerializeField] private float movementSensitivity = 0.01f; // How sensitive the movement detection is
    [SerializeField] private float rotationSensitivity = 0.5f; // How sensitive the rotation detection is
    [SerializeField] private float graceTime = 0.5f; // Short grace period when light changes to red

    [Header("VR Settings")]
    [SerializeField] private Transform vrPlayerHead; // Reference to VR headset/camera
    [SerializeField] private Transform vrLeftController; // Reference to left VR controller
    [SerializeField] private Transform vrRightController; // Reference to right VR controller

    [Header("References")]
    [SerializeField] public GameObject gameover_Panel;
    
    private Quaternion targetRotation;
    private bool isRotating = false;
    private float lightChangeTimer = 0f;
    private bool isGreenLight = false;
    private bool gameActive = false;

    // Movement tracking variables
    private Vector3 lastHeadPosition;
    private Quaternion lastHeadRotation;
    private Vector3 lastLeftControllerPosition;
    private Vector3 lastRightControllerPosition;
    private Quaternion lastLeftControllerRotation;
    private Quaternion lastRightControllerRotation;
    private float redLightStartTime;

    // Timer tick sound coroutine reference
    private Coroutine tickSoundCoroutine;

    void Start()
    {
        // Initialize tracking variables
        if (vrPlayerHead != null)
        {
            lastHeadPosition = vrPlayerHead.position;
            lastHeadRotation = vrPlayerHead.rotation;
        }
        else
        {
            Debug.LogError("VR Player Head transform not assigned!");
        }

        if (vrLeftController != null)
        {
            lastLeftControllerPosition = vrLeftController.position;
            lastLeftControllerRotation = vrLeftController.rotation;
        }

        if (vrRightController != null)
        {
            lastRightControllerPosition = vrRightController.position;
            lastRightControllerRotation = vrRightController.rotation;
        }

        // Initialize doll head to front position
        if (dollHead != null)
        {
            dollHead.localRotation = Quaternion.Euler(0, 0, frontAngle);
        }

        // Setup audio source if needed
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("Created new AudioSource component");
            }
        }

        // Initialize game timer
        currentGameTime = gameTimeInSeconds;
        UpdateTimerDisplay();

        // Auto-start the game if enabled
        if (autoStartGame)
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        gameActive = true;
        isTimerActive = true;
        currentGameTime = gameTimeInSeconds; // Reset timer to full time
        UpdateTimerDisplay();

        if (gameover_Panel != null)
        {
            gameover_Panel.SetActive(false);
        }

        // Start tick sound coroutine if timer sound is assigned
        if (timerTickSound != null)
        {
            if (tickSoundCoroutine != null)
            {
                StopCoroutine(tickSoundCoroutine);
            }
            tickSoundCoroutine = StartCoroutine(PlayTimerTickSounds());
        }

        // Start with green light
        SetGreenLight();

        // Begin rotation immediately
        RotateHead();
    }

    public void StopGame()
    {
        gameActive = false;
        isTimerActive = false;

        // Stop tick sound coroutine if it's running
        if (tickSoundCoroutine != null)
        {
            StopCoroutine(tickSoundCoroutine);
            tickSoundCoroutine = null;
        }
    }

    // Call this to manually trigger rotation
    public void RotateHead()
    {
        if (!isRotating && dollHead != null)
        {
            isRotatingToFront = !isRotatingToFront;
            float targetAngle = isRotatingToFront ? frontAngle : backAngle;
            targetRotation = Quaternion.Euler(0, 0, targetAngle);
            isRotating = true;

            // Only play rotation sound when turning from back to front
            // (which happens during red light)
            if (isRotatingToFront && !isGreenLight)
            {
                PlaySound(rotationSound);
            }

            Debug.Log("RotateHead called. Target Angle: " + targetAngle + ", RotatingToFront: " + isRotatingToFront);
        }
    }

    // Centralized sound playing function
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            // Stop any currently playing sound
            audioSource.Stop();

            // Set the new clip and play it
            audioSource.clip = clip;
            audioSource.Play();

            Debug.Log("Playing sound: " + clip.name);
        }
        else
        {
            if (clip == null)
                Debug.LogWarning("Attempted to play null audio clip");
            if (audioSource == null)
                Debug.LogError("AudioSource component is missing");
        }
    }

    // For one-shot sounds that shouldn't interrupt the current sound
    private void PlaySoundOneShot(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void SetGreenLight()
    {
        isGreenLight = true;
        isRotatingToFront = false;
        float targetAngle = backAngle;
        targetRotation = Quaternion.Euler(0, 0, targetAngle);
        isRotating = true;

        // Set next light change timer
        lightChangeTimer = Random.Range(minGreenLightTime, maxGreenLightTime);

        // Play green light sound
        PlaySound(greenLightSound);

        // Reset tracking variables for VR
        UpdateLastPositions();
    }

    private void SetRedLight()
    {
        isGreenLight = false;
        isRotatingToFront = true;
        float targetAngle = frontAngle;
        targetRotation = Quaternion.Euler(0, 0, targetAngle);
        isRotating = true;

        // Set next light change timer
        lightChangeTimer = Random.Range(minRedLightTime, maxRedLightTime);

        // Play red light sound
        PlaySound(redLightSound);

        // Record time when red light starts
        redLightStartTime = Time.time;

        // Update tracking position and rotation for VR
        UpdateLastPositions();
    }

    // Update all tracking positions for VR movement detection
    private void UpdateLastPositions()
    {
        if (vrPlayerHead != null)
        {
            lastHeadPosition = vrPlayerHead.position;
            lastHeadRotation = vrPlayerHead.rotation;
        }

        if (vrLeftController != null)
        {
            lastLeftControllerPosition = vrLeftController.position;
            lastLeftControllerRotation = vrLeftController.rotation;
        }

        if (vrRightController != null)
        {
            lastRightControllerPosition = vrRightController.position;
            lastRightControllerRotation = vrRightController.rotation;
        }
    }

    public void GameOver()
    {
        if (gameActive)
        {
            Debug.Log("Game Over!");
            gameActive = false;
            isTimerActive = false;

            // Stop tick sound coroutine if it's running
            if (tickSoundCoroutine != null)
            {
                StopCoroutine(tickSoundCoroutine);
                tickSoundCoroutine = null;
            }

            StartCoroutine(PlayGameOverSounds());
        }
    }

    private IEnumerator PlayGameOverSounds()
    {
        PlaySound(elimination);
        yield return new WaitForSeconds(3f); // Adjust delay as needed
        PlaySound(gunshot);
        yield return new WaitForSeconds(1f); // Adjust delay as needed
        gameover_Panel.SetActive(true);
    }

    // Coroutine to play tick sounds in the last 10 seconds
    private IEnumerator PlayTimerTickSounds()
    {
        while (currentGameTime > 0)
        {
            // Only play tick sounds in the last 10 seconds
            if (currentGameTime <= 10f)
            {
                PlaySoundOneShot(timerTickSound);
            }

            // Wait until next second
            yield return new WaitForSeconds(1f);
        }
    }

    // Format and display the time
    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentGameTime / 60);
            int seconds = Mathf.FloorToInt(currentGameTime % 60);

            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // Check if timer has expired
    private void CheckTimer()
    {
        if (isTimerActive)
        {
            currentGameTime -= Time.deltaTime;

            if (currentGameTime <= 0)
            {
                currentGameTime = 0;
                isTimerActive = false;

                // Player wins if they survived the full time
                HandleTimerComplete();
            }

            UpdateTimerDisplay();
        }
    }

    // Method called when timer completes
    private void HandleTimerComplete()
    {
        // Play timer end sound if assigned
        if (timerEndSound != null)
        {
            PlaySound(timerEndSound);
        }

        // Player wins - implement win condition behavior here
        Debug.Log("Time's up! Player survived the full time!");

        // Stop the game
        StopGame();

        // Show win screen or other UI feedback
        if (gameover_Panel != null)
        {
            // You can customize this to show a different win screen
            gameover_Panel.SetActive(true);
        }
    }

    private bool CheckVRMovement()
    {
        bool hasMoved = false;
        
        // Check head movement
        if (vrPlayerHead != null)
        {
            float headPositionDiff = Vector3.Distance(vrPlayerHead.position, lastHeadPosition);
            float headRotationDiff = Quaternion.Angle(vrPlayerHead.rotation, lastHeadRotation);
            
            if (headPositionDiff > movementSensitivity || headRotationDiff > rotationSensitivity)
            {
                hasMoved = true;
                Debug.Log($"Head moved: Position diff={headPositionDiff}, Rotation diff={headRotationDiff}");
            }
            
            // Update head tracking
            lastHeadPosition = vrPlayerHead.position;
            lastHeadRotation = vrPlayerHead.rotation;
        }
        
        // Check left controller movement
        if (vrLeftController != null && !hasMoved)
        {
            float leftHandPositionDiff = Vector3.Distance(vrLeftController.position, lastLeftControllerPosition);
            float leftHandRotationDiff = Quaternion.Angle(vrLeftController.rotation, lastLeftControllerRotation);
            
            if (leftHandPositionDiff > movementSensitivity || leftHandRotationDiff > rotationSensitivity)
            {
                hasMoved = true;
                Debug.Log($"Left controller moved: Position diff={leftHandPositionDiff}, Rotation diff={leftHandRotationDiff}");
            }
            
            // Update left controller tracking
            lastLeftControllerPosition = vrLeftController.position;
            lastLeftControllerRotation = vrLeftController.rotation;
        }
        
        // Check right controller movement
        if (vrRightController != null && !hasMoved)
        {
            float rightHandPositionDiff = Vector3.Distance(vrRightController.position, lastRightControllerPosition);
            float rightHandRotationDiff = Quaternion.Angle(vrRightController.rotation, lastRightControllerRotation);
            
            if (rightHandPositionDiff > movementSensitivity || rightHandRotationDiff > rotationSensitivity)
            {
                hasMoved = true;
                Debug.Log($"Right controller moved: Position diff={rightHandPositionDiff}, Rotation diff={rightHandRotationDiff}");
            }
            
            // Update right controller tracking
            lastRightControllerPosition = vrRightController.position;
            lastRightControllerRotation = vrRightController.rotation;
        }
        
        return hasMoved;
    }

    void Update()
    {
        if (!gameActive) return;

        // Update timer
        CheckTimer();

        // Handle automatic light changes
        lightChangeTimer -= Time.deltaTime;
        if (lightChangeTimer <= 0)
        {
            if (isGreenLight)
            {
                SetRedLight();
            }
            else
            {
                SetGreenLight();
            }
        }

        // Check VR movement during red light
        if (!isGreenLight && !isRotating)
        {
            // Only check for movement after the grace period
            if (Time.time > redLightStartTime + graceTime)
            {
                if (CheckVRMovement())
                {
                    GameOver();
                }
            }
        }

        // Handle rotation
        if (isRotating && dollHead != null)
        {
            dollHead.localRotation = Quaternion.RotateTowards(
                dollHead.localRotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            // Check if rotation is nearly complete
            if (Quaternion.Angle(dollHead.localRotation, targetRotation) < 0.1f)
            {
                dollHead.localRotation = targetRotation; // Snap to exact target
                isRotating = false;
            }
        }
    }

    // Public methods to pause/resume timer
    public void PauseTimer()
    {
        isTimerActive = false;
    }

    public void ResumeTimer()
    {
        isTimerActive = true;
    }

    // Method to add time to the current timer
    public void AddTime(float secondsToAdd)
    {
        currentGameTime += secondsToAdd;
        UpdateTimerDisplay();
    }

    // Method to adjust the sensitivity at runtime
    public void SetMovementSensitivity(float sensitivity)
    {
        movementSensitivity = Mathf.Max(0.001f, sensitivity);
    }

    // Method to adjust the rotation sensitivity at runtime
    public void SetRotationSensitivity(float sensitivity)
    {
        rotationSensitivity = Mathf.Max(0.1f, sensitivity);
    }
}