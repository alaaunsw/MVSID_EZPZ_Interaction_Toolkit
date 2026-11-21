
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
    public bool loopAnimation = false;
    public bool closeWhenPlayerWalksAway = false;

    private Renderer[] pointRenderers;
    private Collider[] pointColliders;
    private VideoPlayer videoPlayer;

    private bool playerIsClose;
    private bool animationIsOpen;
    private bool videoCompleted;

    private void Awake()
    {
        if (playerCamera == null &&
            Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }

        pointRenderers =
            GetComponentsInChildren<Renderer>();

        pointColliders =
            GetComponentsInChildren<Collider>();

        SetPointVisible(false);

        if (informationPanel == null)
            return;

        videoPlayer =
            informationPanel.GetComponent<VideoPlayer>();

        if (videoPlayer == null)
        {
            videoPlayer =
                informationPanel.GetComponentInChildren<
                    VideoPlayer>(true);
        }

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.Stop();

            // Detect when video playback reaches its end.
            videoPlayer.loopPointReached +=
                OnVideoFinished;
        }

        informationPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -=
                OnVideoFinished;
        }
    }

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

        bool isClose =
            distance <= activationDistance;

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

    // EZPZ On Primary Interact()
    public void OpenInformation()
    {
        if (!playerIsClose)
            return;

        if (informationPanel == null)
        {
            Debug.LogWarning(
                "PPE animation panel not assigned."
            );
            return;
        }

        if (animationIsOpen)
        {
            // Don't allow an extra click to interrupt
            // the video-learning sequence.
            return;
        }

        PlayInformationAnimation();
    }

    private void PlayInformationAnimation()
    {
        videoCompleted = false;

        if (SafetyPPEAnimationManager.Instance != null)
        {
            SafetyPPEAnimationManager.Instance.Open(this);
        }

        informationPanel.SetActive(true);
        animationIsOpen = true;

        if (videoPlayer != null)
        {
            videoPlayer.isLooping = false;
            videoPlayer.Stop();
            videoPlayer.Play();

            Debug.Log(
                "PPE video started: " + gameObject.name
            );
        }
        else
        {
            Debug.LogWarning(
                "VideoPlayer not found on " +
                informationPanel.name
            );
        }
    }

    // Called automatically when playback finishes.
    private void OnVideoFinished(VideoPlayer source)
    {
        if (!animationIsOpen ||
            videoCompleted ||
            source != videoPlayer)
            return;

        videoCompleted = true;

        Debug.Log(
            "PPE video finished: " + gameObject.name
        );

        PPETrainingController controller =
            FindFirstObjectByType<PPETrainingController>();

        if (controller != null)
        {
            controller.NotifyVideoFinished(this);
        }
    }

    public void CloseInformation()
    {
        if (videoPlayer != null)
            videoPlayer.Stop();

        if (informationPanel != null)
            informationPanel.SetActive(false);

        animationIsOpen = false;

        if (SafetyPPEAnimationManager.Instance != null)
        {
            SafetyPPEAnimationManager.Instance.NotifyClosed(
                this
            );
        }
    }
}
