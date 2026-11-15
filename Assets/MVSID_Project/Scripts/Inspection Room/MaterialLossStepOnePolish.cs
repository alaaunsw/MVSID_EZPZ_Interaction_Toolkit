
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MaterialLossStepOnePolish : MonoBehaviour
{
    [Header("Existing Training")]
    public GuidedMaterialLossTraining training;

    private Transform content;
    private TMP_Text headingText;
    private TMP_Text pipeIDText;
    private TMP_Text helpText;
    private Transform helpBox;

    private readonly Color navy =
        new Color32(26, 73, 135, 255);

    private readonly Color dark =
        new Color32(43, 55, 72, 255);

    private readonly Color grey =
        new Color32(244, 245, 247, 255);

    [ContextMenu("Build Clean Step 1 Layout")]
    public void Build()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Stop Play Mode first.");
            return;
        }

        if (!FindReferences())
            return;

        headingText = MakeText(
            "LearningHeading",
            30,
            navy,
            FontStyles.Bold
        );

        pipeIDText = MakeText(
            "LearningPipeID",
            18,
            dark,
            FontStyles.Normal
        );

        Transform existing = content.Find("LearningHelpBox");

        if (existing == null)
        {
            GameObject obj = new GameObject(
                "LearningHelpBox",
                typeof(RectTransform),
                typeof(Image)
            );

            obj.transform.SetParent(content, false);
            existing = obj.transform;
        }

        helpBox = existing;

        Image background =
            helpBox.GetComponent<Image>();

        background.color = grey;
        background.raycastTarget = false;

        Transform oldText =
            helpBox.Find("LearningHelpText");

        if (oldText == null)
        {
            GameObject obj = new GameObject(
                "LearningHelpText",
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

            obj.transform.SetParent(helpBox, false);
            oldText = obj.transform;
        }

        helpText = oldText.GetComponent<TMP_Text>();

        helpText.fontSize = 22;
        helpText.color = dark;
        helpText.enableWordWrapping = true;
        helpText.alignment = TextAlignmentOptions.Left;
        helpText.verticalAlignment =
            VerticalAlignmentOptions.Middle;
        helpText.raycastTarget = false;

        Apply(1);

        Debug.Log("Clean training layout created.");
    }

    private bool FindReferences()
    {
        if (training == null)
            training =
                GetComponent<GuidedMaterialLossTraining>();

        content = transform.Find("TrainingContent");

        return training != null && content != null;
    }

    private TMP_Text MakeText(
        string name,
        float fontSize,
        Color color,
        FontStyles style)
    {
        Transform existing = content.Find(name);

        if (existing == null)
        {
            GameObject obj = new GameObject(
                name,
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

            obj.transform.SetParent(content, false);
            existing = obj.transform;
        }

        TMP_Text text =
            existing.GetComponent<TMP_Text>();

        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAlignmentOptions.Left;
        text.verticalAlignment =
            VerticalAlignmentOptions.Middle;
        text.enableWordWrapping = true;
        text.raycastTarget = false;

        return text;
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying)
            return;

        if (!FindReferences() || training.stepText == null)
            return;

        string step = training.stepText.text;

        if (step.Contains("STEP 1"))
            Apply(1);
        else if (step.Contains("STEP 2"))
            Apply(2);
        else
            HideExtraObjects();
    }

    private void Apply(int step)
    {
        if (content == null)
            return;

        RectTransform panel =
            GetComponent<RectTransform>();

        // Compact panel.
        panel.sizeDelta = new Vector2(820, 680);

        // Hide the old duplicated help section.
        Transform oldHelp =
            content.Find("StepHintBackground");

        if (oldHelp != null)
            oldHelp.gameObject.SetActive(false);

        Transform oldVisuals =
            content.Find("WorkspaceVisuals");

        if (oldVisuals != null)
            oldVisuals.gameObject.SetActive(false);

        // Hide calculator while reading measurements.
        if (training.calculatorPanel != null)
            training.calculatorPanel.SetActive(false);

        // Main title.
        Place("TitleText", -105, 281, 560, 45, 26);
        SetAlignment("TitleText", TextAlignmentOptions.Left);

        // Pipe ID at the top right.
        if (pipeIDText != null)
        {
            pipeIDText.gameObject.SetActive(true);
            SetRect(pipeIDText.transform,
                310, 281, 155, 37);

            PipeInspectionData data =
                training.pipeTarget != null
                ? training.pipeTarget.GetInspectionData()
                : null;

            pipeIDText.text =
                data != null ? data.pipeID : "PIPE";
        }

        Place("GoldDivider", 0, 235, 740, 3);

        // Small step indicator on the right.
        Place("StepText", 300, 191, 160, 32, 19);
        SetAlignment("StepText",
            TextAlignmentOptions.Right);

        // Prominent learning heading.
        if (headingText != null)
        {
            headingText.gameObject.SetActive(true);
            headingText.text = step == 1
                ? "Find the Original Wall Thickness"
                : "Find the Remaining Wall Thickness";

            SetRect(headingText.transform,
                0, 145, 740, 49);
        }

        // Short, readable instruction.
        Transform oldBackground =
            content.Find("InstructionBackground");

        if (oldBackground != null)
            oldBackground.gameObject.SetActive(false);

        Place("InstructionText",
            0, 91, 740, 60, 22);

        SetAlignment(
            "InstructionText",
            TextAlignmentOptions.Left
        );

        if (training.instructionText != null)
        {
            training.instructionText.text = step == 1
                ? "Open the pipe information card and locate the original wall thickness."
                : "Return to the pipe information card and locate the remaining wall thickness.";
        }

        // Light-grey help box.
        if (helpBox != null)
        {
            helpBox.gameObject.SetActive(true);
            SetRect(helpBox, 0, 9, 740, 75);
        }

        if (helpText != null)
        {
            helpText.text = step == 1
                ? "WHERE TO LOOK\nWall Thickness (Original)"
                : "WHERE TO LOOK\nRemaining Thickness";

            SetRect(helpText.transform,
                0, 0, 685, 68);
        }

        Place("InputLabel",
            0, -83, 740, 35, 22);

        SetAlignment(
            "InputLabel",
            TextAlignmentOptions.Left
        );

        Place("AnswerInput",
            0, -133, 740, 56);

        Place("FeedbackText",
            0, -196, 740, 53, 20);

        // Action buttons use the same location.
        Place("CheckAnswerButton",
            0, -270, 740, 56);

        Transform next = content.Find(
            "CheckAnswNextStepButton"
        );

        if (next == null)
            next = content.Find("NextStepButton");

        SetRect(next, 0, -270, 740, 56);

        ResizeCollider(
            content.Find("AnswerInput"), 740, 56
        );

        ResizeCollider(
            content.Find("CheckAnswerButton"), 740, 56
        );

        ResizeCollider(next, 740, 56);
    }

    private void HideExtraObjects()
    {
        if (headingText != null)
            headingText.gameObject.SetActive(false);

        if (pipeIDText != null)
            pipeIDText.gameObject.SetActive(false);

        if (helpBox != null)
            helpBox.gameObject.SetActive(false);
    }

    private void Place(
        string name,
        float x,
        float y,
        float width,
        float height,
        float fontSize = 0)
    {
        Transform target = content.Find(name);

        if (target == null)
            return;

        SetRect(target, x, y, width, height);

        TMP_Text text =
            target.GetComponent<TMP_Text>();

        if (text != null && fontSize > 0)
        {
            text.fontSize = fontSize;
            text.enableAutoSizing = false;
        }
    }

    private void SetRect(
        Transform target,
        float x,
        float y,
        float width,
        float height)
    {
        if (target == null)
            return;

        RectTransform rect =
            target as RectTransform;

        if (rect == null)
            return;

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.anchoredPosition =
            new Vector2(x, y);

        rect.sizeDelta =
            new Vector2(width, height);

        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private void SetAlignment(
        string name,
        TextAlignmentOptions alignment)
    {
        Transform target = content.Find(name);

        if (target == null)
            return;

        TMP_Text text =
            target.GetComponent<TMP_Text>();

        if (text != null)
            text.alignment = alignment;
    }

    private void ResizeCollider(
        Transform target,
        float width,
        float height)
    {
        if (target == null)
            return;

        BoxCollider box =
            target.GetComponent<BoxCollider>();

        if (box == null)
            return;

        box.center = Vector3.zero;
        box.size = new Vector3(
            width, height, 2f
        );
    }
}
