
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public class SafetyTraineeAccountManager : MonoBehaviour
{
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

    private AccountDatabase database;
    private string filePath;

    private const int HashIterations = 150000;
    private const int SaltLength = 16;
    private const int HashLength = 32;

    public string CurrentUsername => currentUsername;
    public string CurrentTraineeId => currentTraineeId;

    public bool IsLoggedIn =>
        !string.IsNullOrEmpty(currentTraineeId);

    private void Awake()
    {
        filePath = Path.Combine(
            Application.persistentDataPath,
            "trainee_accounts.json"
        );

        LoadAccounts();
    }

    // ---------------------------------------
    // SIGN UP
    // ---------------------------------------

    public bool Register(
        string username,
        string password,
        out string message)
    {
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

        if (FindAccount(username) != null)
        {
            message = "Username already exists.";
            return false;
        }

        byte[] salt = new byte[SaltLength];

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

        TraineeAccount account = new TraineeAccount
        {
            id = Guid.NewGuid().ToString("N"),
            username = username,
            salt = Convert.ToBase64String(salt),
            passwordHash = Convert.ToBase64String(hash),
            iterations = HashIterations
        };

        database.accounts.Add(account);

        if (!SaveAccounts())
        {
            database.accounts.Remove(account);

            message =
                "Could not save account. Please try again.";
            return false;
        }

        SetCurrentTrainee(account);

        message = "Account created successfully.";
        return true;
    }

    // ---------------------------------------
    // LOG IN
    // ---------------------------------------

    public bool Login(
        string username,
        string password,
        out string message)
    {
        username = username?.Trim() ?? "";

        TraineeAccount account =
            FindAccount(username);

        if (account == null ||
            string.IsNullOrEmpty(password))
        {
            message = "Invalid username or password.";
            return false;
        }

        try
        {
            byte[] salt =
                Convert.FromBase64String(account.salt);

            byte[] storedHash =
                Convert.FromBase64String(
                    account.passwordHash
                );

            byte[] enteredHash = DerivePassword(
                password,
                salt,
                account.iterations
            );

            if (!HashesEqual(storedHash, enteredHash))
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

        SetCurrentTrainee(account);

        message = "Login successful.";
        return true;
    }

    // ---------------------------------------
    // PASSWORD HASHING
    // ---------------------------------------

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
            return pbkdf2.GetBytes(HashLength);
        }
    }

    private bool HashesEqual(
        byte[] a,
        byte[] b)
    {
        if (a == null || b == null ||
            a.Length != b.Length)
            return false;

        int difference = 0;

        for (int i = 0; i < a.Length; i++)
            difference |= a[i] ^ b[i];

        return difference == 0;
    }

    // ---------------------------------------
    // ACCOUNT LOOKUP
    // ---------------------------------------

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

    private void SetCurrentTrainee(
        TraineeAccount account)
    {
        currentUsername = account.username;
        currentTraineeId = account.id;

        Debug.Log(
            "Trainee session started: " +
            currentUsername
        );
    }

    public void Logout()
    {
        currentUsername = "";
        currentTraineeId = "";
    }

    // ---------------------------------------
    // SAVE ACCOUNT DATABASE
    // ---------------------------------------

    private bool SaveAccounts()
    {
        try
        {
            string json =
                JsonUtility.ToJson(database, true);

            string temporaryPath =
                filePath + ".tmp";

            File.WriteAllText(
                temporaryPath,
                json
            );

            if (File.Exists(filePath))
                File.Delete(filePath);

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

    // ---------------------------------------
    // LOAD ACCOUNT DATABASE
    // ---------------------------------------

    private void LoadAccounts()
    {
        database = new AccountDatabase();

        if (!File.Exists(filePath))
            return;

        try
        {
            string json =
                File.ReadAllText(filePath);

            AccountDatabase loaded =
                JsonUtility.FromJson<AccountDatabase>(
                    json
                );

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
}
