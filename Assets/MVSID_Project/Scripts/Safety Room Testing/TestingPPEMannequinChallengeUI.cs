
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestingPPEMannequinChallengeUI : MonoBehaviour
{
    [Header("Existing Systems")]
    public TestingPPEMatchingController matchingController;
    public TestingPPETaskPuzzle taskPuzzle;
    public TestingPPEPuzzlePanelBuilder imageSource;

    [Header("Generated UI")]
    public GameObject matchingCanvas;
    public GameObject mannequinCanvas;
    public Button nextChallengeButton;
    public Button nextScenarioButton;
    public TextMeshProUGUI scenarioText;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI feedbackText;

    public Button[] equipmentButtons = new Button[10];

    private int scenario = 0;
    private int routineMistakes = 0;
    private bool dressingStarted = false;
    private bool finished = false;

    private readonly bool[] selected = new bool[10];

    private readonly string[] equipmentNames =
    {
        "Helmet", "Glasses", "Gloves", "Vest",
        "Boots", "Coveralls", "Earmuffs",
        "Gas Mask", "Face Shield", "Ear Plugs"
    };

    private void Start()
    {
        if (matchingController == null)
            matchingController =
                GetComponent<TestingPPEMatchingController>();

        if (taskPuzzle == null)
            taskPuzzle = GetComponent<TestingPPETaskPuzzle>();

        if (imageSource == null)
            imageSource =
                GetComponent<TestingPPEPuzzlePanelBuilder>();

        if (matchingCanvas == null)
        {
            Transform found =
                transform.Find("TestingPPEPuzzleCanvas");

            if (found != null)
                matchingCanvas = found.gameObject;
        }

        if (nextChallengeButton != null)
        {
            nextChallengeButton.onClick.AddListener(
                StartMannequinChallenge);
            nextChallengeButton.gameObject.SetActive(false);
        }

        if (nextScenarioButton != null)
        {
            nextScenarioButton.onClick.AddListener(
                AdvanceScenario);
            nextScenarioButton.gameObject.SetActive(false);
        }

        for (int i = 0; i < equipmentButtons.Length; i++)
        {
            if (equipmentButtons[i] == null)
                continue;

            int id = i;

            equipmentButtons[i].onClick.AddListener(
                () => ChooseEquipment(id));
        }

        if (mannequinCanvas != null)
            mannequinCanvas.SetActive(false);
    }

    private void Update()
    {
        if (!dressingStarted)
        {
            if (matchingController != null &&
                matchingController.IsComplete &&
                nextChallengeButton != null)
            {
                nextChallengeButton.gameObject.SetActive(true);
            }

            return;
        }

        if (finished || taskPuzzle == null)
            return;

        progressText.text =
            "EQUIPPED: " + taskPuzzle.CorrectSelections +
            " / " + (scenario == 0 ? 6 : 7) +
            "  |  MISTAKES: " + taskPuzzle.Mistakes;

        if (taskPuzzle.IsScenarioComplete &&
            nextScenarioButton != null)
        {
            nextScenarioButton.gameObject.SetActive(true);
        }
    }

    public void StartMannequinChallenge()
    {
        if (matchingController == null ||
            !matchingController.IsComplete ||
            taskPuzzle == null)
            return;

        dressingStarted = true;
        finished = false;

        if (matchingCanvas != null)
            matchingCanvas.SetActive(false);

        if (mannequinCanvas != null)
            mannequinCanvas.SetActive(true);

        StartScenario(0);
    }

    private void StartScenario(int index)
    {
        scenario = index;

        for (int i = 0; i < selected.Length; i++)
            selected[i] = false;

        taskPuzzle.StartScenario(index);

        if (nextScenarioButton != null)
            nextScenarioButton.gameObject.SetActive(false);

        if (index == 0)
        {
            scenarioText.text =
                "SCENARIO 1: ROUTINE PIPELINE INSPECTION";

            instructionText.text =
                "Prepare the worker for a routine pipeline " +
                "inspection in the designated work area. " +
                "Select the required protective equipment.";

        }
        else
        {
            scenarioText.text =
                "SCENARIO 2: HIGH-NOISE PIPELINE INSPECTION";

            instructionText.text =
                "The inspection is near operating compressors. " +
                "The site requires hearing protection in addition " +
                "to the baseline inspection PPE.";
        }

        feedbackText.text =
            "Select PPE to dress the mannequin.";

        RefreshButtons();
    }

    public void ChooseEquipment(int id)
    {
        if (!dressingStarted || finished ||
            taskPuzzle == null ||
            taskPuzzle.IsScenarioComplete)
            return;

        if (id < 0 || id >= selected.Length)
            return;

        if (selected[id])
            return;

        int previousCorrect =
            taskPuzzle.CorrectSelections;

        taskPuzzle.SelectEquipment(id);

        bool correct =
            taskPuzzle.CorrectSelections > previousCorrect;

        if (correct)
        {
            selected[id] = true;

            feedbackText.text =
                "CORRECT! " + equipmentNames[id] +
                " added to the mannequin.";

            feedbackText.color =
                new Color32(95, 255, 145, 255);
        }
        else
        {
            feedbackText.text =
                "INCORRECT! This PPE is not required " +
                "for the current scenario.";

            feedbackText.color =
                new Color32(255, 135, 115, 255);
        }

        RefreshButtons();
    }

    private void RefreshButtons()
    {
        for (int i = 0; i < equipmentButtons.Length; i++)
        {
            Button b = equipmentButtons[i];

            if (b == null)
                continue;

            b.interactable = !selected[i];

            Image image = b.GetComponent<Image>();

            if (image != null)
            {
                image.color = selected[i]
                    ? new Color32(44, 125, 94, 255)
                    : new Color32(36, 80, 107, 255);
            }
        }
    }

    public void AdvanceScenario()
    {
        if (taskPuzzle == null ||
            !taskPuzzle.IsScenarioComplete)
            return;

        if (scenario == 0)
        {
            routineMistakes = taskPuzzle.Mistakes;
            StartScenario(1);
        }
        else
        {
            finished = true;
            GetComponent<TestingPPEFinalFeedback>()?.ShowResults();
            int totalMistakes =
                matchingController.Mistakes +
                routineMistakes +
                taskPuzzle.Mistakes;

            scenarioText.text =
                "PPE PRACTICAL ASSESSMENT COMPLETE";

            instructionText.text =
                "Both mannequin scenarios have been completed.";

            feedbackText.text =
                "Total mistakes across both puzzle " +
                "challenges: " + totalMistakes;

            feedbackText.color = Color.white;

            progressText.text =
                "MATCHING: 10/10 | SCENARIOS: 2/2";

            nextScenarioButton.gameObject.SetActive(false);

            foreach (Button b in equipmentButtons)
            {
                if (b != null)
                    b.interactable = false;
            }

            Debug.Log(
                "[PPE Puzzle] Practical assessment complete. " +
                "Total mistakes: " + totalMistakes);
        }
    }

    [ContextMenu("Build Mannequin Challenge UI")]
    public void BuildMannequinUI()
    {
        if (transform.Find("TestingPPEMannequinCanvas") != null)
        {
            Debug.LogWarning(
                "Mannequin Canvas already exists.");
            return;
        }

        GameObject canvasObject = new GameObject(
            "TestingPPEMannequinCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        canvasObject.transform.SetParent(transform, false);

        RectTransform canvasRect =
            canvasObject.GetComponent<RectTransform>();

        canvasRect.sizeDelta = new Vector2(1100, 760);
        canvasRect.localScale = Vector3.one * 0.0025f;

        canvasObject.transform.position =
            new Vector3(0, 1.5f, 2.8f);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        Camera cam = Camera.main;
        if (cam != null)
            canvas.worldCamera = cam;

        mannequinCanvas = canvasObject;

        GameObject panel = MakeUI(
            "MannequinPanel", canvasObject.transform,
            1100, 760, 0, 0);

        panel.AddComponent<Image>().color =
            new Color32(120, 150, 166, 255);

        MakeText("Title", panel.transform,
            "PPE SELECTION PUZZLE",
            37, 1000, 65, 0, 330, Color.white);

        GameObject header = MakeUI(
            "ScenarioHeader", panel.transform,
            1030, 70, 0, 250);

        header.AddComponent<Image>().color =
            new Color32(255, 175, 32, 255);

        scenarioText = MakeText(
            "ScenarioText", header.transform,
            "SCENARIO 1: ROUTINE PIPELINE INSPECTION",
            28, 1000, 65, 0, 0,
            new Color32(25, 35, 45, 255));

        instructionText = MakeText(
            "InstructionText", panel.transform,
            "Select the PPE required for the task.",
            23, 1000, 70, 0, 175, Color.white);

        for (int i = 0; i < 10; i++)
        {
            int col = i % 5;
            int row = i / 5;

            float x = -410 + col * 205;
            float y = 65 - row * 165;

            GameObject buttonObject = MakeUI(
                "MannequinPPE_" + i,
                panel.transform,
                180, 140, x, y);

            Image background =
                buttonObject.AddComponent<Image>();

            background.color =
                new Color32(36, 80, 107, 255);

            Button b = buttonObject.AddComponent<Button>();
            equipmentButtons[i] = b;

            BoxCollider collider =
                buttonObject.AddComponent<BoxCollider>();

            collider.size = new Vector3(180, 140, 5);

            GameObject picture = MakeUI(
                "PPEImage", buttonObject.transform,
                100, 90, 0, 15);

            Image pictureImage =
                picture.AddComponent<Image>();

            pictureImage.preserveAspect = true;
            pictureImage.raycastTarget = false;

            if (imageSource == null)
                imageSource =
                    GetComponent<TestingPPEPuzzlePanelBuilder>();

            if (imageSource != null &&
                imageSource.ppeImages != null &&
                i < imageSource.ppeImages.Length)
            {
                pictureImage.sprite =
                    imageSource.ppeImages[i];
            }

            MakeText(
                "Letter", buttonObject.transform,
                ((char)('A' + i)).ToString(),
                26, 160, 38, 0, -51,
                new Color32(255, 196, 50, 255));
        }

        feedbackText = MakeText(
            "FeedbackText", panel.transform,
            "Select PPE to dress the mannequin.",
            22, 1000, 45, 0, -230, Color.white);

        progressText = MakeText(
            "ProgressText", panel.transform,
            "EQUIPPED: 0 / 6",
            25, 850, 48, 0, -280, Color.white);

        nextScenarioButton = MakeActionButton(
            "NextScenarioButton",
            panel.transform,
            "NEXT SCENARIO",
            300, -335);

        nextScenarioButton.gameObject.SetActive(false);

        // Add a Next Challenge button to the
        // already existing matching puzzle.
        Transform oldPanel = transform.Find(
            "TestingPPEPuzzleCanvas/PuzzlePanel");

        if (oldPanel != null)
        {
            nextChallengeButton = MakeActionButton(
                "NextChallengeButton",
                oldPanel,
                "NEXT CHALLENGE",
                290, -390);

            nextChallengeButton.gameObject.SetActive(false);
        }

        Debug.Log("Mannequin puzzle UI created.");
    }

    private GameObject MakeUI(
        string name, Transform parent,
        float w, float h, float x, float y)
    {
        GameObject obj = new GameObject(
            name, typeof(RectTransform));

        obj.transform.SetParent(parent, false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.sizeDelta = new Vector2(w, h);
        rect.anchoredPosition = new Vector2(x, y);

        return obj;
    }

    private TextMeshProUGUI MakeText(
        string name, Transform parent,
        string content, float size,
        float w, float h, float x, float y,
        Color color)
    {
        GameObject obj =
            MakeUI(name, parent, w, h, x, y);

        TextMeshProUGUI text =
            obj.AddComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = size;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.raycastTarget = false;

        return text;
    }

    private Button MakeActionButton(
        string name, Transform parent,
        string title, float width, float y)
    {
        GameObject obj =
            MakeUI(name, parent, width, 55, 0, y);

        obj.AddComponent<Image>().color =
            new Color32(36, 80, 107, 255);

        Button button = obj.AddComponent<Button>();

        BoxCollider collider = obj.AddComponent<BoxCollider>();
        collider.size = new Vector3(width, 55, 5);

        MakeText(
            "ButtonText", obj.transform,
            title, 22, width, 50,
            0, 0, Color.white);

        return button;
    }
}
