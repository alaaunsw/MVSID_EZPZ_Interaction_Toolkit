
using UnityEngine;
using UnityEngine.Video;

public class SafetyPPEInfoPoint : MonoBehaviour
{
    [Header("Player")]
    public Transform playerCamera;

    [Header("Interaction Distance")]
    public float activationDistance = 1.5f;

    [Header("Animation Display")]
    public GameObject informationPanel;

    [Header("Animation Settings")]
    public bool loopAnimation = true;
    public bool closeWhenPlayerWalksAway = false;

    private Renderer[] pointRenderers;
    private Collider[] pointColliders;
    private VideoPlayer videoPlayer;

    private bool playerIsClose = false;
    private bool animationIsOpen = false;

    // ==========================================
    // INITIALISATION
    // ==========================================

    private void Awake()
    {
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }

        pointRenderers =
            GetComponentsInChildren<Renderer>();

        pointColliders =
            GetComponentsInChildren<Collider>();

        SetPointVisible(false);

        if (informationPanel != null)
        {
            videoPlayer =
                informationPanel.GetComponent<VideoPlayer>();

            if (videoPlayer == null)
            {
                videoPlayer =
                    informationPanel.GetComponentInChildren<VideoPlayer>(
                        true
                    );
            }

            if (videoPlayer != null)
            {
                videoPlayer.playOnAwake = false;
                videoPlayer.isLooping = loopAnimation;
                videoPlayer.Stop();
            }

            informationPanel.SetActive(false);
        }
    }

    // ==========================================
    // PLAYER DISTANCE
    // ==========================================

    private void Update()
    {
        if (playerCamera == null)
        {
            if (Camera.main != null)
                playerCamera = Camera.main.transform;

            if (playerCamera == null)
                return;
        }

        float distance = Vector3.Distance(
            playerCamera.position,
            transform.position
        );

        bool isClose = distance <= activationDistance;

        if (isClose != playerIsClose)
        {
            playerIsClose = isClose;

            SetPointVisible(playerIsClose);

            if (!playerIsClose &&
                closeWhenPlayerWalksAway &&
                animationIsOpen)
            {
                CloseInformation();
            }
        }
    }

    // ==========================================
    // INTERACTION POINT VISIBILITY
    // ==========================================

    private void SetPointVisible(bool visible)
    {
        foreach (Renderer renderer in pointRenderers)
        {
            if (renderer != null)
                renderer.enabled = visible;
        }

        foreach (Collider collider in pointColliders)
        {
            if (collider != null)
                collider.enabled = visible;
        }
    }

    // ==========================================
    // EZPZ PRIMARY INTERACTION
    // ==========================================

    public void OpenInformation()
    {
        if (!playerIsClose)
        {
            Debug.Log("Move closer to the PPE equipment.");
            return;
        }

        if (informationPanel == null)
        {
            Debug.LogWarning(
                "PPE animation display is not assigned."
            );
            return;
        }

        if (animationIsOpen)
        {
            CloseInformation();
        }
        else
        {
            PlayInformationAnimation();
        }
    }

    // ==========================================
    // PLAY ANIMATION
    // ==========================================

    private void PlayInformationAnimation()
    {
        // Automatically close the previous PPE animation.
        if (SafetyPPEAnimationManager.Instance != null)
        {
            SafetyPPEAnimationManager.Instance.Open(this);
        }

        informationPanel.SetActive(true);
        animationIsOpen = true;

        if (videoPlayer == null)
        {
            videoPlayer =
                informationPanel.GetComponent<VideoPlayer>();

            if (videoPlayer == null)
            {
                videoPlayer =
                    informationPanel.GetComponentInChildren<VideoPlayer>(
                        true
                    );
            }
        }

        if (videoPlayer != null)
        {
            videoPlayer.isLooping = loopAnimation;

            videoPlayer.Stop();
            videoPlayer.Play();

            Debug.Log(
                "PPE animation started: " + gameObject.name
            );
        }
        else
        {
            Debug.LogWarning(
                "Video Player component not found."
            );
        }
    }

    // ==========================================
    // CLOSE ANIMATION
    // ==========================================

    public void CloseInformation()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        if (informationPanel != null)
        {
            informationPanel.SetActive(false);
        }

        animationIsOpen = false;

        // Inform the manager that this animation closed.
        if (SafetyPPEAnimationManager.Instance != null)
        {
            SafetyPPEAnimationManager.Instance.NotifyClosed(this);
        }

        Debug.Log(
            "PPE animation closed: " + gameObject.name
        );
    }
}
