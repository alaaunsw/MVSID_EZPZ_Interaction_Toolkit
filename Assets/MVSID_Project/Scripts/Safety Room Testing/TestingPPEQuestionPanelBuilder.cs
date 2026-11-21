
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
#endif

public class TestingPPEQuestionPanelBuilder : MonoBehaviour
{
    [Header("Existing Assessment Manager")]
    public TestingPPEAssessmentManager assessmentManager;

    [Header("Panel Appearance")]
    public Color backgroundColor =
        new Color32(110, 143, 161, 245);

    public Color buttonColor =
        new Color32(24, 73, 102, 255);

    public Color headingColor = Color.white;

    [Header("Generated Panel")]
    public GameObject generatedPanel;

    [ContextMenu("Build Testing PPE Question Panel")]
    public void BuildQuestionPanel()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Stop Play Mode before building the panel."
            );
            return;
        }

        Canvas canvas = GetComponent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError(
                "Attach this script to SafetyWelcomeCanvas."
            );
            return;
        }

        if (assessmentManager == null)
        {
            Debug.LogError(
                "Assign TestingPPEAssessmentManager first."
            );
            return;
        }

        Transform existing =
            transform.Find("TestingPPEQuestionPanel");

        if (existing != null)
        {
            Debug.LogWarning(
                "TestingPPEQuestionPanel already exists. " +
                "It has not been rebuilt."
            );
            return;
        }

        // =====================================
        // CREATE MAIN PANEL
        // =====================================

        GameObject panel = CreateUI(
            "TestingPPEQuestionPanel",
            transform,
            850,
            720,
            0,
            0
        );

        Image panelImage =
            panel.AddComponent<Image>();

        panelImage.color = backgroundColor;
        panelImage.raycastTarget = true;

        generatedPanel = panel;

        // =====================================
        // HEADING
        // =====================================

        CreateText(
            "Heading",
            panel.transform,
            "PPE KNOWLEDGE ASSESSMENT",
            34,
            780, 55,
            0, 300,
            headingColor,
            true
        );

        TextMeshProUGUI equipmentTitle =
            CreateText(
                "EquipmentTitle",
                panel.transform,
                "SAFETY HELMET",
                30,
                760, 50,
                0, 240,
                Color.white,
                true
            );

        TextMeshProUGUI questionText =
            CreateText(
                "QuestionText",
                panel.transform,
                "What is the main purpose " +
                "of this equipment?",
                25,
                760, 75,
                0, 170,
                Color.white,
                false
            );

        // =====================================
        // FOUR ANSWER BUTTONS
        // =====================================

        Button[] buttons = new Button[4];

        TextMeshProUGUI[] labels =
            new TextMeshProUGUI[4];

        int[] yPositions =
        {
            75, -10, -95, -180
        };

        string[] examples =
        {
            "A. Protect the head from impacts",
            "B. Protect the hands from cuts",
            "C. Reduce exposure to noise",
            "D. Improve visibility"
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject answer = CreateUI(
                "AnswerButton_" + (i + 1),
                panel.transform,
                745, 68,
                0, yPositions[i]
            );

            Image answerImage =
                answer.AddComponent<Image>();

            answerImage.color = buttonColor;

            Button button =
                answer.AddComponent<Button>();

            button.targetGraphic = answerImage;

            buttons[i] = button;

            BoxCollider collider =
                answer.AddComponent<BoxCollider>();

            collider.center = Vector3.zero;

            collider.size =
                new Vector3(745, 68, 10);

            // Same EZPZ interaction system used
            // by the rest of the Safety Room.
            answer.AddComponent<InteractableGeneral>();

            TestingPPEAnswerButton relay =
                answer.AddComponent<
                    TestingPPEAnswerButton
                >();

            relay.assessmentManager =
                assessmentManager;

            relay.answerIndex = i;

            labels[i] = CreateText(
                "AnswerText",
                answer.transform,
                examples[i],
                22,
                700, 60,
                0, 0,
                Color.white,
                false
            );

#if UNITY_EDITOR

            // Connect normal desktop Unity UI.
            UnityEventTools.AddPersistentListener(
                button.onClick,
                relay.Activate
            );

#endif
        }

        // =====================================
        // STATUS AND PROGRESS
        // =====================================

        TextMeshProUGUI statusText =
            CreateText(
                "StatusText",
                panel.transform,
                "Select one answer.",
                20,
                750, 45,
                0, -270,
                Color.white,
                false
            );

        TextMeshProUGUI progressText =
            CreateText(
                "ProgressText",
                panel.transform,
                "EQUIPMENT IDENTIFIED: 0 / 5",
                21,
                750, 40,
                0, -325,
                Color.white,
                true
            );

        // =====================================
        // CONNECT TO EXISTING MANAGER
        // =====================================

        assessmentManager.questionPanel = panel;

        assessmentManager.equipmentTitle =
            equipmentTitle;

        assessmentManager.questionText =
            questionText;

        assessmentManager.statusText =
            statusText;

        assessmentManager.progressText =
            progressText;

        assessmentManager.answerButtons =
            buttons;

        assessmentManager.answerLabels =
            labels;

        // Start hidden; equipment interactions
        // will open the panel during testing.
        panel.SetActive(false);

#if UNITY_EDITOR

        EditorUtility.SetDirty(assessmentManager);
        EditorUtility.SetDirty(this);

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene
        );

        Selection.activeGameObject = panel;

#endif

        Debug.Log(
            "Testing PPE Question Panel created. " +
            "Connect the four EZPZ events next."
        );
    }

    // ==========================================
    // CREATE UI OBJECT
    // ==========================================

    private GameObject CreateUI(
        string objectName,
        Transform parent,
        float width,
        float height,
        float x,
        float y)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform)
        );

        obj.transform.SetParent(parent, false);

        obj.layer = LayerMask.NameToLayer("UI");

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.sizeDelta =
            new Vector2(width, height);

        rect.anchoredPosition =
            new Vector2(x, y);

        rect.localScale = Vector3.one;

        rect.localRotation = Quaternion.identity;

        return obj;
    }

    // ==========================================
    // CREATE TEXT
    // ==========================================

    private TextMeshProUGUI CreateText(
        string objectName,
        Transform parent,
        string content,
        float fontSize,
        float width,
        float height,
        float x,
        float y,
        Color color,
        bool bold)
    {
        GameObject obj = CreateUI(
            objectName,
            parent,
            width, height,
            x, y
        );

        TextMeshProUGUI text =
            obj.AddComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = fontSize;
        text.color = color;

        text.fontStyle =
            bold ? FontStyles.Bold : FontStyles.Normal;

        text.alignment =
            TextAlignmentOptions.Center;

        text.enableWordWrapping = true;
        text.raycastTarget = false;

        return text;
    }
}

// ==========================================
// INDIVIDUAL ANSWER BUTTON
// ==========================================

public class TestingPPEAnswerButton : MonoBehaviour
{
    public TestingPPEAssessmentManager assessmentManager;

    [Range(0, 3)]
    public int answerIndex;

    public void Activate()
    {
        if (assessmentManager == null)
            return;

        assessmentManager.SelectAnswer(answerIndex);
    }
}
