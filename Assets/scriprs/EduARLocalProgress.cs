using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class EduARTopicProgress
{
    public string subject;
    public string topic;
    public int arScans;
    public bool summaryRead;
    public int quizAttempts;
    public int totalQuestions;
    public int totalCorrect;
    public int bestScore;
    public float averageScore;
    public bool favorite;
    public string lastActivityUtc;

    public float ProgressPercent()
    {
        if (quizAttempts > 0 && bestScore >= 80 && summaryRead && arScans > 0)
            return 100f;

        if (quizAttempts > 0)
            return 75f;

        if (summaryRead)
            return 50f;

        if (arScans > 0)
            return 25f;

        return 0f;
    }

    public string MasteryLabel()
    {
        if (quizAttempts > 0 && bestScore >= 80 && summaryRead && arScans > 0)
            return "MASTERED";

        if (quizAttempts > 0)
            return "QUIZ COMPLETED";

        if (summaryRead)
            return "SUMMARY READ";

        if (arScans > 0)
            return "EXPLORING";

        return "NOT STARTED";
    }
}

[Serializable]
public class EduARQuizHistoryEntry
{
    public string subject;
    public string topic;
    public int score;
    public int total;
    public float percentage;
    public string timestampUtc;
}

[Serializable]
public class EduARMistakeEntry
{
    public string subject;
    public string topic;
    public string question;
    public string selectedAnswer;
    public string correctAnswer;
    public string timestampUtc;
}

[Serializable]
public class EduARStudentProgress
{
    public string userId;
    public string lastLearningDate;
    public int streak;
    public int totalArScans;
    public int totalQuizAttempts;
    public List<string> recentTopics = new List<string>();
    public List<EduARTopicProgress> topics = new List<EduARTopicProgress>();
    public List<EduARQuizHistoryEntry> quizHistory = new List<EduARQuizHistoryEntry>();
    public List<EduARMistakeEntry> mistakes = new List<EduARMistakeEntry>();

    public bool goalScanToday;
    public bool goalLearnToday;
    public bool goalQuizToday;
}

[Serializable]
public class EduARProgressDatabase
{
    public List<EduARStudentProgress> students = new List<EduARStudentProgress>();
}

public static class EduARLocalProgress
{
    private static EduARProgressDatabase database;
    private static EduARStudentProgress current;

    private static string FilePath =>
        Path.Combine(Application.persistentDataPath, "EduAR_ProgressData.json");

    public static bool IsReady => current != null;

    public static void Initialize(string userId)
    {
        LoadDatabase();

        if (string.IsNullOrEmpty(userId))
        {
            current = null;
            return;
        }

        current = null;

        for (int i = 0; i < database.students.Count; i++)
        {
            if (database.students[i].userId == userId)
            {
                current = database.students[i];
                break;
            }
        }

        if (current == null)
        {
            current = new EduARStudentProgress
            {
                userId = userId,
                streak = 0
            };

            database.students.Add(current);
            SaveDatabase();
        }
    }

    public static void EndSession()
    {
        SaveDatabase();
        current = null;
    }

    public static EduARTopicProgress GetTopic(
        string subject,
        string topic)
    {
        EnsureCurrent();

        for (int i = 0; i < current.topics.Count; i++)
        {
            if (current.topics[i].subject == subject &&
                current.topics[i].topic == topic)
            {
                return current.topics[i];
            }
        }

        EduARTopicProgress created = new EduARTopicProgress
        {
            subject = subject,
            topic = topic
        };

        current.topics.Add(created);
        SaveDatabase();
        return created;
    }

    public static void MarkARScan(string subject, string topic)
    {
        if (!IsReady) return;

        EduARTopicProgress data = GetTopic(subject, topic);
        data.arScans++;
        current.totalArScans++;
        data.lastActivityUtc = DateTime.UtcNow.ToString("O");

        TouchRecentTopic(subject, topic);
        MarkDailyAction("scan");
        SaveDatabase();
    }

    public static void MarkSummaryRead(string subject, string topic)
    {
        if (!IsReady) return;

        EduARTopicProgress data = GetTopic(subject, topic);
        data.summaryRead = true;
        data.lastActivityUtc = DateTime.UtcNow.ToString("O");

        TouchRecentTopic(subject, topic);
        MarkDailyAction("learn");
        SaveDatabase();
    }

    public static void SaveQuizResult(
        string subject,
        string topic,
        int score,
        int total,
        List<EduARQuizAnswerData> answers)
    {
        if (!IsReady || total <= 0)
            return;

        EduARTopicProgress data = GetTopic(subject, topic);
        data.quizAttempts++;
        data.totalQuestions += total;
        data.totalCorrect += score;
        data.bestScore = Mathf.Max(
            data.bestScore,
            Mathf.RoundToInt((float)score / total * 100f)
        );
        data.averageScore =
            (float)data.totalCorrect / Mathf.Max(1, data.totalQuestions) * 100f;
        data.lastActivityUtc = DateTime.UtcNow.ToString("O");

        current.totalQuizAttempts++;

        EduARQuizHistoryEntry history = new EduARQuizHistoryEntry
        {
            subject = subject,
            topic = topic,
            score = score,
            total = total,
            percentage = (float)score / total * 100f,
            timestampUtc = DateTime.UtcNow.ToString("O")
        };

        current.quizHistory.Insert(0, history);
        TrimHistory();

        if (answers != null)
        {
            for (int i = 0; i < answers.Count; i++)
            {
                if (answers[i].isCorrect)
                    continue;

                current.mistakes.Insert(0, new EduARMistakeEntry
                {
                    subject = subject,
                    topic = topic,
                    question = answers[i].question,
                    selectedAnswer = answers[i].selectedAnswer,
                    correctAnswer = answers[i].correctAnswer,
                    timestampUtc = DateTime.UtcNow.ToString("O")
                });
            }
        }

        TrimMistakes();
        TouchRecentTopic(subject, topic);
        MarkDailyAction("quiz");
        SaveDatabase();
    }

    public static void ToggleFavorite(string subject, string topic)
    {
        if (!IsReady) return;

        EduARTopicProgress data = GetTopic(subject, topic);
        data.favorite = !data.favorite;
        SaveDatabase();
    }

    public static int GetDailyGoalCompletedCount()
    {
        if (!IsReady) return 0;

        RefreshDailyFlags();

        int count = 0;
        if (current.goalScanToday) count++;
        if (current.goalLearnToday) count++;
        if (current.goalQuizToday) count++;
        return count;
    }

    public static int GetStreak()
    {
        if (!IsReady) return 0;
        RefreshDailyFlags();
        return current.streak;
    }

    public static float GetOverallProgress()
    {
        float total = 0f;
        int count = 0;

        string[] subjects = { "Biology", "Physics", "Chemistry", "Mathematics" };

        for (int i = 0; i < subjects.Length; i++)
        {
            string[] topics = GetTopicsForSubject(subjects[i]);

            for (int j = 0; j < topics.Length; j++)
            {
                total += GetTopic(subjects[i], topics[j]).ProgressPercent();
                count++;
            }
        }

        return count == 0 ? 0f : total / count;
    }

    public static float GetSubjectProgress(string subject)
    {
        string[] topics = GetTopicsForSubject(subject);
        if (topics.Length == 0) return 0f;

        float total = 0f;

        for (int i = 0; i < topics.Length; i++)
            total += GetTopic(subject, topics[i]).ProgressPercent();

        return total / topics.Length;
    }

    public static int GetMasteredCount()
    {
        int count = 0;
        string[] subjects = { "Biology", "Physics", "Chemistry", "Mathematics" };

        for (int i = 0; i < subjects.Length; i++)
        {
            string[] topics = GetTopicsForSubject(subjects[i]);
            for (int j = 0; j < topics.Length; j++)
            {
                if (GetTopic(subjects[i], topics[j]).ProgressPercent() >= 100f)
                    count++;
            }
        }

        return count;
    }

    public static int GetAverageQuizScore()
    {
        if (!IsReady || current.quizHistory.Count == 0)
            return 0;

        float total = 0f;
        for (int i = 0; i < current.quizHistory.Count; i++)
            total += current.quizHistory[i].percentage;

        return Mathf.RoundToInt(total / current.quizHistory.Count);
    }

    public static int GetBestQuizScore()
    {
        if (!IsReady) return 0;

        int best = 0;
        for (int i = 0; i < current.quizHistory.Count; i++)
            best = Mathf.Max(best, Mathf.RoundToInt(current.quizHistory[i].percentage));

        return best;
    }

    public static List<EduARQuizHistoryEntry> GetQuizHistory(int maxItems = 5)
    {
        if (!IsReady) return new List<EduARQuizHistoryEntry>();

        List<EduARQuizHistoryEntry> copy =
            new List<EduARQuizHistoryEntry>();

        int count = Mathf.Min(maxItems, current.quizHistory.Count);
        for (int i = 0; i < count; i++)
            copy.Add(current.quizHistory[i]);

        return copy;
    }

    public static List<EduARMistakeEntry> GetMistakes(int maxItems = 20)
    {
        if (!IsReady) return new List<EduARMistakeEntry>();

        List<EduARMistakeEntry> copy =
            new List<EduARMistakeEntry>();

        int count = Mathf.Min(maxItems, current.mistakes.Count);
        for (int i = 0; i < count; i++)
            copy.Add(current.mistakes[i]);

        return copy;
    }

    public static List<string> GetRecentTopics(int maxItems = 4)
    {
        if (!IsReady) return new List<string>();

        List<string> copy = new List<string>();
        int count = Mathf.Min(maxItems, current.recentTopics.Count);

        for (int i = 0; i < count; i++)
            copy.Add(current.recentTopics[i]);

        return copy;
    }

    public static bool IsFavorite(string subject, string topic)
    {
        if (!IsReady) return false;
        return GetTopic(subject, topic).favorite;
    }

    public static int GetTotalArScans()
    {
        return IsReady ? current.totalArScans : 0;
    }

    public static int GetTotalQuizAttempts()
    {
        return IsReady ? current.totalQuizAttempts : 0;
    }

    public static bool IsAchievementUnlocked(string achievementId)
    {
        if (!IsReady) return false;

        switch (achievementId)
        {
            case "first_quiz":
                return current.totalQuizAttempts >= 1;

            case "ar_explorer":
                return current.totalArScans >= 10;

            case "quiz_champion":
                {
                    int highScores = 0;
                    for (int i = 0; i < current.quizHistory.Count; i++)
                    {
                        if (current.quizHistory[i].percentage >= 100f)
                            highScores++;
                    }
                    return highScores >= 3;
                }

            case "anatomy_master":
                return GetTopic("Biology", "Heart").ProgressPercent() >= 100f &&
                       GetTopic("Biology", "Brain").ProgressPercent() >= 100f &&
                       GetTopic("Biology", "Lungs").ProgressPercent() >= 100f;

            case "science_explorer":
                {
                    int subjects = 0;
                    string[] names = { "Biology", "Physics", "Chemistry", "Mathematics" };
                    for (int i = 0; i < names.Length; i++)
                    {
                        if (GetSubjectProgress(names[i]) > 0f)
                            subjects++;
                    }
                    return subjects >= 3;
                }

            case "streak_7":
                return current.streak >= 7;

            default:
                return false;
        }
    }

    private static void MarkDailyAction(string type)
    {
        if (!IsReady) return;

        string today = DateTime.Now.ToString("yyyy-MM-dd");

        if (current.lastLearningDate != today)
        {
            if (DateTime.TryParse(
                    current.lastLearningDate,
                    out DateTime previous))
            {
                DateTime todayDate = DateTime.Now.Date;
                if (previous.Date == todayDate.AddDays(-1))
                    current.streak++;
                else
                    current.streak = 1;
            }
            else
            {
                current.streak = 1;
            }

            current.lastLearningDate = today;
            current.goalScanToday = false;
            current.goalLearnToday = false;
            current.goalQuizToday = false;
        }

        switch (type)
        {
            case "scan":
                current.goalScanToday = true;
                break;
            case "learn":
                current.goalLearnToday = true;
                break;
            case "quiz":
                current.goalQuizToday = true;
                break;
        }
    }

    private static void RefreshDailyFlags()
    {
        if (!IsReady) return;

        string today = DateTime.Now.ToString("yyyy-MM-dd");
        if (current.lastLearningDate != today)
        {
            current.goalScanToday = false;
            current.goalLearnToday = false;
            current.goalQuizToday = false;
        }
    }

    private static void TouchRecentTopic(string subject, string topic)
    {
        if (!IsReady) return;

        string key = subject + "|" + topic;

        current.recentTopics.Remove(key);
        current.recentTopics.Insert(0, key);

        while (current.recentTopics.Count > 8)
            current.recentTopics.RemoveAt(current.recentTopics.Count - 1);
    }

    private static void TrimHistory()
    {
        while (current.quizHistory.Count > 50)
            current.quizHistory.RemoveAt(current.quizHistory.Count - 1);
    }

    private static void TrimMistakes()
    {
        while (current.mistakes.Count > 100)
            current.mistakes.RemoveAt(current.mistakes.Count - 1);
    }

    private static string[] GetTopicsForSubject(string subject)
    {
        switch (subject)
        {
            case "Biology":
                return new[] { "Heart", "Brain", "Lungs" };
            case "Physics":
                return new[] { "Pendulum" };
            case "Chemistry":
                return new[] { "Water" };
            case "Mathematics":
                return new[] { "Geometry" };
            default:
                return new string[0];
        }
    }

    private static void EnsureCurrent()
    {
        if (!IsReady)
            throw new InvalidOperationException("EduARLocalProgress is not initialized.");
    }

    private static void LoadDatabase()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                database = new EduARProgressDatabase();
                SaveDatabase();
                return;
            }

            string json = File.ReadAllText(FilePath);
            database = string.IsNullOrWhiteSpace(json)
                ? new EduARProgressDatabase()
                : JsonUtility.FromJson<EduARProgressDatabase>(json);

            if (database == null)
                database = new EduARProgressDatabase();

            if (database.students == null)
                database.students = new List<EduARStudentProgress>();
        }
        catch (Exception ex)
        {
            Debug.LogWarning(
                "[EduAR] Progress database load failed: " +
                ex.Message
            );

            database = new EduARProgressDatabase();
        }
    }

    private static void SaveDatabase()
    {
        if (database == null) return;

        try
        {
            File.WriteAllText(
                FilePath,
                JsonUtility.ToJson(database, true)
            );
        }
        catch (Exception ex)
        {
            Debug.LogWarning(
                "[EduAR] Progress database save failed: " +
                ex.Message
            );
        }
    }
}

[Serializable]
public class EduARQuizAnswerData
{
    public string question;
    public string selectedAnswer;
    public string correctAnswer;
    public bool isCorrect;
}
