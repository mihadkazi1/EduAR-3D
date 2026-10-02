using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

[Serializable]
public class EduARLocalUser
{
    public string userId;
    public string fullName;
    public string studentId;
    public string email;
    public string passwordSalt;
    public string passwordHash;
    public string registeredAtUtc;
    public string avatarPath;
}

[Serializable]
public class EduARLocalDatabase
{
    public List<EduARLocalUser> users = new List<EduARLocalUser>();
}

public static class EduARLocalAuth
{
    private const int SaltSize = 16;
    private const string SessionKey = "EduAR_SessionUserId";

    private static EduARLocalDatabase database;

    private static string FilePath =>
        Path.Combine(Application.persistentDataPath, "EduAR_LocalData.json");

    public static EduARLocalUser CurrentUser
    {
        get
        {
            EnsureLoaded();

            string sessionId = PlayerPrefs.GetString(SessionKey, "");
            if (string.IsNullOrEmpty(sessionId))
                return null;

            for (int i = 0; i < database.users.Count; i++)
            {
                if (database.users[i].userId == sessionId)
                    return database.users[i];
            }

            PlayerPrefs.DeleteKey(SessionKey);
            PlayerPrefs.Save();
            return null;
        }
    }

    public static void Initialize()
    {
        LoadDatabase();
    }

    public static bool TryGetCurrentUser(out EduARLocalUser user)
    {
        user = CurrentUser;
        return user != null;
    }

    public static bool TryRegister(
        string fullName,
        string studentId,
        string email,
        string password,
        out EduARLocalUser user,
        out string error)
    {
        EnsureLoaded();

        user = null;
        error = "";

        string normalizedEmail = NormalizeEmail(email);

        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(studentId) ||
            string.IsNullOrWhiteSpace(normalizedEmail) ||
            string.IsNullOrEmpty(password))
        {
            error = "All fields are required.";
            return false;
        }

        for (int i = 0; i < database.users.Count; i++)
        {
            if (string.Equals(
                    database.users[i].email,
                    normalizedEmail,
                    StringComparison.OrdinalIgnoreCase))
            {
                error = "An account with this email already exists.";
                return false;
            }
        }

        byte[] salt = new byte[SaltSize];

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        user = new EduARLocalUser
        {
            userId = Guid.NewGuid().ToString("N"),
            fullName = fullName.Trim(),
            studentId = studentId.Trim(),
            email = normalizedEmail,
            passwordSalt = Convert.ToBase64String(salt),
            passwordHash = ComputeHash(password, salt),
            registeredAtUtc = DateTime.UtcNow.ToString("O")
        };

        database.users.Add(user);
        SaveDatabase();

        return true;
    }

    public static bool TryLogin(
        string email,
        string password,
        out EduARLocalUser user,
        out string error)
    {
        EnsureLoaded();

        user = null;
        error = "";

        string normalizedEmail = NormalizeEmail(email);

        for (int i = 0; i < database.users.Count; i++)
        {
            EduARLocalUser candidate = database.users[i];

            if (!string.Equals(
                    candidate.email,
                    normalizedEmail,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (VerifyPassword(
                    password,
                    candidate.passwordSalt,
                    candidate.passwordHash))
            {
                user = candidate;
                SetSession(candidate.userId);
                return true;
            }

            error = "Incorrect password.";
            return false;
        }

        error = "No local account was found for this email.";
        return false;
    }

    public static void Logout()
    {
        PlayerPrefs.DeleteKey(SessionKey);
        PlayerPrefs.Save();
    }

    public static bool SetAvatarPath(string userId, string avatarPath)
    {
        EnsureLoaded();

        if (string.IsNullOrWhiteSpace(userId))
            return false;

        for (int i = 0; i < database.users.Count; i++)
        {
            if (database.users[i].userId != userId)
                continue;

            database.users[i].avatarPath = avatarPath ?? string.Empty;
            SaveDatabase();
            return true;
        }

        return false;
    }

    public static bool ClearAvatarPath(string userId)
    {
        return SetAvatarPath(userId, string.Empty);
    }

    public static bool HasAnyAccount()
    {
        EnsureLoaded();
        return database.users.Count > 0;
    }

    public static List<EduARLocalUser> GetUsers()
    {
        EnsureLoaded();
        return new List<EduARLocalUser>(database.users);
    }

    public static void DeleteAllLocalData()
    {
        database = new EduARLocalDatabase();
        Logout();

        try
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(
                "[EduAR] Could not delete local auth data: " +
                ex.Message
            );
        }
    }

    private static void SetSession(string userId)
    {
        PlayerPrefs.SetString(SessionKey, userId);
        PlayerPrefs.Save();
    }

    private static void EnsureLoaded()
    {
        if (database == null)
            LoadDatabase();
    }

    private static void LoadDatabase()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                database = new EduARLocalDatabase();
                SaveDatabase();
                return;
            }

            string json = File.ReadAllText(FilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                database = new EduARLocalDatabase();
                return;
            }

            database = JsonUtility.FromJson<EduARLocalDatabase>(json);

            if (database == null)
                database = new EduARLocalDatabase();

            if (database.users == null)
                database.users = new List<EduARLocalUser>();
        }
        catch (Exception ex)
        {
            Debug.LogWarning(
                "[EduAR] Local auth database load failed: " +
                ex.Message
            );

            database = new EduARLocalDatabase();
        }
    }

    private static void SaveDatabase()
    {
        try
        {
            string json = JsonUtility.ToJson(database, true);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "[EduAR] Local auth database save failed: " +
                ex.Message
            );
        }
    }

    private static string NormalizeEmail(string email)
    {
        return string.IsNullOrWhiteSpace(email)
            ? ""
            : email.Trim().ToLowerInvariant();
    }

    private static string ComputeHash(
        string password,
        byte[] salt)
    {
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] combined = new byte[salt.Length + passwordBytes.Length];

        Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
        Buffer.BlockCopy(
            passwordBytes,
            0,
            combined,
            salt.Length,
            passwordBytes.Length
        );

        using (SHA256 sha = SHA256.Create())
        {
            byte[] hash = sha.ComputeHash(combined);
            return Convert.ToBase64String(hash);
        }
    }

    private static bool VerifyPassword(
        string password,
        string saltBase64,
        string expectedHash)
    {
        try
        {
            byte[] salt = Convert.FromBase64String(saltBase64);
            string actualHash = ComputeHash(password, salt);

            byte[] actual = Convert.FromBase64String(actualHash);
            byte[] expected = Convert.FromBase64String(expectedHash);

            if (actual.Length != expected.Length)
                return false;

            int difference = 0;

            for (int i = 0; i < actual.Length; i++)
                difference |= actual[i] ^ expected[i];

            return difference == 0;
        }
        catch
        {
            return false;
        }
    }
}
