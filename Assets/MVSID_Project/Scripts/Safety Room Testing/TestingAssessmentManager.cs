
using System;
using System.Collections.Generic;
using UnityEngine;

public class TestingAssessmentManager : MonoBehaviour
{
    public static TestingAssessmentManager Instance
    {
        get;
        private set;
    }

    // =====================================
    // INDIVIDUAL ANSWER RECORD
    // =====================================

    [Serializable]
    public class AnswerRecord
    {
        public string question;
        public string traineeAnswer;
        public string correctAnswer;
        public bool isCorrect;
        public string improvement;
    }

    // =====================================
    // INDIVIDUAL TASK RECORD
    // =====================================

    [Serializable]
    public class TaskRecord
    {
        public string taskID;
        public string taskName;
        public string category;

        public bool completed;
        public bool contributesToScore = true;

        public int correctAnswers;
        public int totalAnswers;
        public int incorrectAttempts;

        public string improvement;

        public List<AnswerRecord> answers =
            new List<AnswerRecord>();

        public List<string> mistakeDetails =
            new List<string>();

        public float Accuracy
        {
            get
            {
                if (totalAnswers <= 0)
                    return 0f;

                return 100f * correctAnswers /
                       totalAnswers;
            }
        }
    }

    // =====================================
    // SESSION INFORMATION
    // =====================================

    [Header("Trainee Information")]
    [SerializeField] private string traineeName;

    [Header("Session Status")]
    [SerializeField] private bool sessionStarted;
    [SerializeField] private bool sessionCompleted;

    [SerializeField] private float startTime;
    [SerializeField] private float finalDuration;

    [Header("Assessment Settings")]
    [Range(0f, 100f)]
    public float passingScore = 80f;

    [Header("Recorded Assessment Tasks")]
    [SerializeField]
    private List<TaskRecord> tasks =
        new List<TaskRecord>();

    public string TraineeName => traineeName;

    public bool SessionStarted => sessionStarted;

    public bool SessionCompleted => sessionCompleted;

    public IReadOnlyList<TaskRecord> Tasks => tasks;

    // =====================================
    // PERSISTENT INSTANCE
    // =====================================

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static TestingAssessmentManager
        GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        GameObject obj = new GameObject(
            "TestingAssessmentManager"
        );

        return obj.AddComponent<
            TestingAssessmentManager
        >();
    }

    // =====================================
    // START TESTING SESSION
    // =====================================

    public void BeginSession(string username)
    {
        if (sessionStarted &&
            !sessionCompleted)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Session already active."
            );

            return;
        }

        traineeName = username;

        tasks.Clear();

        startTime = Time.realtimeSinceStartup;
        finalDuration = 0f;

        sessionStarted = true;
        sessionCompleted = false;

        RegisterDefaultTasks();

        Debug.Log(
            "[ASSESSMENT] Testing session started: " +
            traineeName
        );
    }

    // =====================================
    // REGISTER EXPECTED TASKS
    // =====================================

    private void RegisterDefaultTasks()
    {
        RegisterTask(
            "SAFETY_01",
            "Equipment Knowledge",
            "Safety",
            13
        );

        RegisterTask(
            "SAFETY_02",
            "PPE Matching",
            "Safety",
            10
        );

        RegisterTask(
            "SAFETY_03",
            "Mannequin PPE Assessment",
            "Safety",
            13
        );

        RegisterTask(
            "INSPECTION_01",
            "Pipeline Identification",
            "Inspection",
            1
        );

        RegisterTask(
            "INSPECTION_02",
            "Visual Corrosion Inspection",
            "Inspection",
            1
        );

        RegisterTask(
            "INSPECTION_03",
            "Material Loss Assessment",
            "Inspection",
            0
        );

        RegisterTask(
            "INSPECTION_04",
            "Initial Engineer Report",
            "Inspection",
            0
        );

        RegisterTask(
            "INSPECTION_05",
            "Instrument Readings",
            "Inspection",
            2
        );

        RegisterTask(
            "INSPECTION_06",
            "Pressure Reduction Operation",
            "Inspection",
            1
        );

        RegisterTask(
            "INSPECTION_07",
            "Updated Pressure Report",
            "Inspection",
            1
        );

        RegisterTask(
            "INSPECTION_08",
            "Corrosion Risk Visualisation",
            "Inspection",
            0,
            false
        );

        RegisterTask(
            "INSPECTION_09",
            "Final Inspection Report",
            "Inspection",
            0
        );
    }

    public void RegisterTask(
        string taskID,
        string taskName,
        string category,
        int totalAnswers,
        bool contributesToScore = true)
    {
        if (FindTask(taskID) != null)
            return;

        TaskRecord task = new TaskRecord();

        task.taskID = taskID;
        task.taskName = taskName;
        task.category = category;

        task.totalAnswers =
            Mathf.Max(0, totalAnswers);

        task.contributesToScore =
            contributesToScore;

        tasks.Add(task);
    }

    // =====================================
    // FIND TASK
    // =====================================

    public TaskRecord FindTask(string taskID)
    {
        return tasks.Find(
            task => task.taskID == taskID
        );
    }

    // =====================================
    // RECORD INDIVIDUAL ANSWER
    // =====================================

    public void RecordAnswer(
        string taskID,
        string question,
        string traineeAnswer,
        string correctAnswer,
        bool isCorrect,
        string improvement = "")
    {
        if (!sessionStarted ||
            sessionCompleted)
            return;

        TaskRecord task = FindTask(taskID);

        if (task == null)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Unknown task: " +
                taskID
            );

            return;
        }

        if (task.completed)
            return;

        // Prevent duplicate answer records.

        if (task.answers.Exists(
            answer => answer.question == question))
        {
            return;
        }

        AnswerRecord record = new AnswerRecord();

        record.question = question;
        record.traineeAnswer = traineeAnswer;
        record.correctAnswer = correctAnswer;
        record.isCorrect = isCorrect;
        record.improvement = improvement;

        task.answers.Add(record);

        task.totalAnswers = Mathf.Max(
            task.totalAnswers,
            task.answers.Count
        );

        if (isCorrect)
        {
            task.correctAnswers++;
        }
        else
        {
            task.incorrectAttempts++;

            task.mistakeDetails.Add(
                question +
                " | Submitted: " +
                traineeAnswer +
                " | Expected: " +
                correctAnswer
            );
        }

        Debug.Log(
            "[ASSESSMENT] Answer recorded: " +
            taskID
        );
    }

    // =====================================
    // RECORD COMPLETE TASK RESULTS
    // =====================================

    public void RecordTaskResult(
        string taskID,
        int correctAnswers,
        int totalAnswers,
        int incorrectAttempts,
        bool completed,
        string improvement = "",
        IEnumerable<string> mistakeDetails = null)
    {
        if (!sessionStarted ||
            sessionCompleted)
            return;

        TaskRecord task = FindTask(taskID);

        if (task == null)
        {
            Debug.LogWarning(
                "[ASSESSMENT] Unknown task: " +
                taskID
            );

            return;
        }

        task.totalAnswers =
            Mathf.Max(0, totalAnswers);

        task.correctAnswers = Mathf.Clamp(
            correctAnswers,
            0,
            task.totalAnswers
        );

        task.incorrectAttempts =
            Mathf.Max(0, incorrectAttempts);

        task.completed = completed;
        task.improvement = improvement;

        if (mistakeDetails != null)
        {
            List<string> details =
                new List<string>(mistakeDetails);

            task.mistakeDetails.Clear();
            task.mistakeDetails.AddRange(details);
        }

        Debug.Log(
            "[ASSESSMENT] Task recorded: " +
            task.taskName
        );
    }

    // =====================================
    // MARK TASK COMPLETE
    // =====================================

    public void MarkTaskCompleted(string taskID)
    {
        if (!sessionStarted ||
            sessionCompleted)
            return;

        TaskRecord task = FindTask(taskID);

        if (task == null)
            return;

        task.completed = true;

        Debug.Log(
            "[ASSESSMENT] Task completed: " +
            task.taskName
        );
    }

    // =====================================
    // TASK COMPLETION STATISTICS
    // =====================================

    public int GetCompletedTaskCount()
    {
        int count = 0;

        foreach (TaskRecord task in tasks)
        {
            if (task.completed)
                count++;
        }

        return count;
    }

    public int GetTotalTaskCount()
    {
        return tasks.Count;
    }

    public bool AllTasksCompleted()
    {
        return tasks.Count > 0 &&
               GetCompletedTaskCount() ==
               GetTotalTaskCount();
    }

    // =====================================
    // ACCURACY CALCULATIONS
    // =====================================

    public float GetOverallScore()
    {
        return CalculateAccuracy("");
    }

    public float GetSafetyScore()
    {
        return CalculateAccuracy("Safety");
    }

    public float GetInspectionScore()
    {
        return CalculateAccuracy("Inspection");
    }

    private float CalculateAccuracy(string category)
    {
        int correct = 0;
        int total = 0;

        foreach (TaskRecord task in tasks)
        {
            if (!task.contributesToScore)
                continue;

            if (!string.IsNullOrEmpty(category) &&
                task.category != category)
                continue;

            correct += task.correctAnswers;
            total += task.totalAnswers;
        }

        if (total <= 0)
            return 0f;

        return 100f * correct / total;
    }

    public int GetTotalMistakes()
    {
        int total = 0;

        foreach (TaskRecord task in tasks)
        {
            total += task.incorrectAttempts;
        }

        return total;
    }

    // =====================================
    // SESSION TIMER
    // =====================================

    public float GetElapsedSeconds()
    {
        if (!sessionStarted)
            return 0f;

        if (sessionCompleted)
            return finalDuration;

        return Mathf.Max(
            0f,
            Time.realtimeSinceStartup - startTime
        );
    }

    public string GetFormattedDuration()
    {
        TimeSpan time = TimeSpan.FromSeconds(
            GetElapsedSeconds()
        );

        return string.Format(
            "{0:00}:{1:00}:{2:00}",
            (int)time.TotalHours,
            time.Minutes,
            time.Seconds
        );
    }

    // =====================================
    // COMPLETE TESTING SESSION
    // =====================================

    public void FinishSession()
    {
        if (!sessionStarted ||
            sessionCompleted)
            return;

        if (!AllTasksCompleted())
        {
            Debug.LogWarning(
                "[ASSESSMENT] Some tasks are incomplete."
            );

            return;
        }

        finalDuration = GetElapsedSeconds();

        sessionCompleted = true;

        Debug.Log(
            "[ASSESSMENT] Testing session completed."
        );

        Debug.Log(
            "[ASSESSMENT] Overall score: " +
            GetOverallScore().ToString("F1") +
            "%"
        );

        Debug.Log(
            "[ASSESSMENT] Completion time: " +
            GetFormattedDuration()
        );
    }

    public bool HasPassed()
    {
        return sessionCompleted &&
               AllTasksCompleted() &&
               GetOverallScore() >= passingScore;
    }

    // =====================================
    // RESET SESSION
    // =====================================

    public void ResetSession()
    {
        tasks.Clear();

        traineeName = "";

        sessionStarted = false;
        sessionCompleted = false;

        startTime = 0f;
        finalDuration = 0f;

        Debug.Log(
            "[ASSESSMENT] Session reset."
        );
    }
}
