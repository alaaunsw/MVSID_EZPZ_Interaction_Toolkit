
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public class SafetyTraineeAccountManager : MonoBehaviour
{
    public static SafetyTraineeAccountManager Instance
    {
        get;
        private set;
    }

    [Serializable]
    public class TraineeAccount
    {
        public string id;
        public string username;
        public string salt;
        public string passwordHash;
        public int iterations;
    }

    [Serializable]
    private class AccountDatabase
    {
        public List<TraineeAccount> accounts =
            new List<TraineeAccount>();
    }

    [Header("Current Session")]

    [SerializeField]
    private string currentUsername = "";

    [SerializeField]
    private string currentTraineeId = "";

    // NEW: Display current safety verification.

    [Header("Safety Session Verification")]

    [SerializeField]
    private bool safetyTrainingVerified;

    private AccountDatabase database;
    private string filePath;

    private const int HashIterations = 150000;
    private const int SaltLength = 16;
    private const int HashLength = 32;

    // ==========================================
    // PUBLIC ACCOUNT PROPERTIES
    // ==========================================

    public string CurrentUsername =>
        Instance != null && Instance != this
        ? Instance.CurrentUsername
        : currentUsername;

    public string CurrentTraineeId =>
        Instance != null && Instance != this
        ? Instance.CurrentTraineeId
        : currentTraineeId;

    public bool IsLoggedIn =>
        !string.IsNullOrEmpty(CurrentTraineeId);

    // NEW: Current trainee safety verification.

    public bool HasVerifiedSafetyTraining =>
        IsLoggedIn &&
        TraineeSafetySession.IsCompletedFor(
            CurrentTraineeId
        );

    // ==========================================
    // UNITY
    // ==========================================

    private void Awake()
    {
        // Keep one authoritative account manager.

        if (Instance != null &&
            Instance != this)
        {
            Debug.Log(
                "An existing trainee account manager " +
                "is already active."
            );

            return;
        }

        Instance = this;

        // Preserve the manager across scenes.

        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogError(
                "SafetyTraineeAccountManager must be " +
                "on a dedicated root GameObject " +
                "to persist across scenes."
            );
        }

        // Start without an authenticated trainee.

        currentUsername = "";
        currentTraineeId = "";

        // NEW: Clear previous runtime safety status.

        TraineeSafetySession.Clear();

        safetyTrainingVerified = false;

        filePath = Path.Combine(
            Application.persistentDataPath,
            "trainee_accounts.json"
        );

        LoadAccounts();

        Debug.Log(
            "Persistent trainee account system ready."
        );
    }

    private void Update()
    {
        // NEW: Automatically display safety status
        // in the Unity Inspector.

        safetyTrainingVerified =
            HasVerifiedSafetyTraining;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            TraineeSafetySession.Clear();

            Instance = null;
        }
    }

    // ==========================================
    // SIGN UP
    // ==========================================

    public bool Register(
        string username,
        string password,
        out string message)
    {
        // Forward requests to the persistent manager.

        if (Instance != null &&
            Instance != this)
        {
            return Instance.Register(
                username,
                password,
                out message
            );
        }

        username = username?.Trim() ?? "";

        if (username.Length < 3 ||
            username.Length > 32)
        {
            message =
                "Username must contain 3-32 characters.";

            return false;
        }

        if (password == null ||
            password.Length < 8)
        {
            message =
                "Password must contain at least 8 characters.";

            return false;
        }

        if (database == null)
        {
            message =
                "Account database is not available.";

            return false;
        }

        if (FindAccount(username) != null)
        {
            message =
                "Username already exists.";

            return false;
        }

        byte[] salt =
            new byte[SaltLength];

        using (RandomNumberGenerator rng =
            RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        byte[] hash = DerivePassword(
            password,
            salt,
            HashIterations
        );

        TraineeAccount account =
            new TraineeAccount
        {
            id =
                Guid.NewGuid().ToString("N"),

            username =
                username,

            salt =
                Convert.ToBase64String(salt),

            passwordHash =
                Convert.ToBase64String(hash),

            iterations =
                HashIterations
        };

        database.accounts.Add(account);

        if (!SaveAccounts())
        {
            database.accounts.Remove(account);

            message =
                "Could not save account. Please try again.";

            return false;
        }

        // Start a new authenticated session.

        SetCurrentTrainee(account);

        message =
            "Account created successfully.";

        return true;
    }

    // ==========================================
    // LOG IN
    // ==========================================

    public bool Login(
        string username,
        string password,
        out string message)
    {
        if (Instance != null &&
            Instance != this)
        {
            return Instance.Login(
                username,
                password,
                out message
            );
        }

        username = username?.Trim() ?? "";

        TraineeAccount account =
            FindAccount(username);

        if (account == null ||
            string.IsNullOrEmpty(password))
        {
            message =
                "Invalid username or password.";

            return false;
        }

        try
        {
            byte[] salt =
                Convert.FromBase64String(
                    account.salt
                );

            byte[] storedHash =
                Convert.FromBase64String(
                    account.passwordHash
                );

            byte[] enteredHash =
                DerivePassword(
                    password,
                    salt,
                    account.iterations
                );

            if (!HashesEqual(
                storedHash,
                enteredHash))
            {
                message =
                    "Invalid username or password.";

                return false;
            }
        }
        catch (Exception)
        {
            message =
                "Account verification failed.";

            return false;
        }

        // Start a new authenticated session.

        SetCurrentTrainee(account);

        message =
            "Login successful.";

        return true;
    }

    // ==========================================
    // PASSWORD HASHING
    // ==========================================

    private byte[] DerivePassword(
        string password,
        byte[] salt,
        int iterations)
    {
        using (var pbkdf2 =
            new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256))
        {
            return pbkdf2.GetBytes(
                HashLength
            );
        }
    }

    private bool HashesEqual(
        byte[] a,
        byte[] b)
    {
        if (a == null ||
            b == null ||
            a.Length != b.Length)
        {
            return false;
        }

        int difference = 0;

        for (int i = 0;
             i < a.Length;
             i++)
        {
            difference |= a[i] ^ b[i];
        }

        return difference == 0;
    }

    // ==========================================
    // ACCOUNT LOOKUP
    // ==========================================

    private TraineeAccount FindAccount(
        string username)
    {
        if (database == null)
            return null;

        foreach (TraineeAccount account
                 in database.accounts)
        {
            if (string.Equals(
                account.username,
                username,
                StringComparison.OrdinalIgnoreCase))
            {
                return account;
            }
        }

        return null;
    }

    // ==========================================
    // START TRAINEE SESSION
    // ==========================================

    private void SetCurrentTrainee(
        TraineeAccount account)
    {
        // NEW: Every successful login or registration
        // begins a fresh safety verification session.

        TraineeSafetySession.Clear();

        safetyTrainingVerified = false;

        currentUsername =
            account.username;

        currentTraineeId =
            account.id;

        Debug.Log(
            "Trainee session started: " +
            currentUsername
        );

        Debug.Log(
            "New safety verification session started."
        );

        // Reload previously saved individual
        // inspection training progress.

        RefreshTraineeProgress();
    }

    // ==========================================
    // LOG OUT
    // ==========================================

    public void Logout()
    {
        if (Instance != null &&
            Instance != this)
        {
            Instance.Logout();

            return;
        }

        // NEW: Clear safety verification.

        TraineeSafetySession.Clear();

        safetyTrainingVerified = false;

        currentUsername = "";
        currentTraineeId = "";

        // Clear the loaded runtime progress.
        // Saved trainee progress is not deleted.

        RefreshTraineeProgress();

        Debug.Log(
            "Trainee logged out successfully."
        );

        Debug.Log(
            "Safety session verification cleared."
        );
    }

    // ==========================================
    // REFRESH INDIVIDUAL TRAINEE PROGRESS
    // ==========================================

    private void RefreshTraineeProgress()
    {
        // Prefer the progress manager attached
        // to this persistent account manager.

        SafetyTraineeProgressManager manager =
            GetComponent<
                SafetyTraineeProgressManager
            >();

        if (manager == null)
        {
            manager =
                FindFirstObjectByType<
                    SafetyTraineeProgressManager
                >();
        }

        if (manager != null)
        {
            manager.accountManager = this;

            manager.RefreshProgress();
        }
    }

    // ==========================================
    // SAVE ACCOUNT DATABASE
    // ==========================================

    private bool SaveAccounts()
    {
        try
        {
            string json =
                JsonUtility.ToJson(
                    database,
                    true
                );

            string temporaryPath =
                filePath + ".tmp";

            File.WriteAllText(
                temporaryPath,
                json
            );

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.Move(
                temporaryPath,
                filePath
            );

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Account save failed: " +
                e.Message
            );

            return false;
        }
    }

    // ==========================================
    // LOAD ACCOUNT DATABASE
    // ==========================================

    private void LoadAccounts()
    {
        database =
            new AccountDatabase();

        if (!File.Exists(filePath))
            return;

        try
        {
            string json =
                File.ReadAllText(filePath);

            AccountDatabase loaded =
                JsonUtility.FromJson<
                    AccountDatabase
                >(json);

            if (loaded != null &&
                loaded.accounts != null)
            {
                database = loaded;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Account loading failed: " +
                e.Message
            );
        }
    }

    // ==========================================
    // CHECK CURRENT LOGIN SESSION
    // ==========================================

    [ContextMenu("Check Current Trainee Session")]
    public void CheckCurrentTraineeSession()
    {
        if (!IsLoggedIn)
        {
            Debug.Log(
                "No trainee is currently logged in."
            );

            return;
        }

        Debug.Log(
            "Current trainee: " +
            CurrentUsername
        );

        Debug.Log(
            "Current trainee ID: " +
            CurrentTraineeId
        );

        Debug.Log(
            "Active scene: " +
            UnityEngine.SceneManagement
                .SceneManager
                .GetActiveScene().name
        );

        Debug.Log(
            "Safety training verified: " +
            HasVerifiedSafetyTraining
        );

        SafetyTraineeAccountManager activeManager =
            Instance != null ? Instance : this;

        SafetyTraineeProgressManager manager =
            activeManager.GetComponent<
                SafetyTraineeProgressManager
            >();

        if (manager != null)
        {
            Debug.Log(
                "Successful training sessions: " +
                manager.SuccessfulTrainingSessions +
                " / " +
                manager.RequiredTrainingSessions
            );

            Debug.Log(
                "Testing access: " +
                manager.CanAccessTesting
            );
        }

        Debug.Log(
            "Trainee session is active."
        );
    }
}
