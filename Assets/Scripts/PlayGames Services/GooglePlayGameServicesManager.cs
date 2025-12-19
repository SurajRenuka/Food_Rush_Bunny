using FancyScrollView.HeliosScrollView;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10)]

public class GooglePlayGameServicesManager : MonoBehaviour
{
    public static GooglePlayGameServicesManager Instance;

    public bool isAuthenticate = false;

    private const string LeaderboardID = "CgkI9YOPxNwNEAIQAQ";


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Initialize Play Games platform
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        SignInToGooglePlay();
    }

    private void SignInToGooglePlay()
    {

        PlayGamesPlatform.Instance.localUser.Authenticate(success =>
        {
            if (success)
            {
                isAuthenticate = true;
                Debug.Log("Signed into Google Play Services successfully!");
            }
            else
            {
                isAuthenticate = false;
                Debug.LogError("Failed to sign in to Google Play Services.");
            }
        });
    }


    public void SubmitScoreToLeaderboard(long score)
    {
        PlayGamesPlatform.Instance.ReportScore(score, LeaderboardID, success =>
        {
            if (success)
            {
                Debug.Log("Score submitted successfully to leaderboard!");
            }
            else
            {
                Debug.LogError("Failed to submit score to leaderboard.");
            }
        });

    }



    /*public void GetLeaderboardData(Action<List<Leaderboard_ItemData>> callback)
    {

        PlayGamesPlatform.Instance.LoadScores(
            LeaderboardID,
            LeaderboardStart.PlayerCentered,
            10,
            LeaderboardCollection.Public,
            LeaderboardTimeSpan.AllTime,
            (data) =>
            {
                if (data.Valid)
                {
                    Debug.Log($"Leaderboard data retrieved successfully. -> {data.Scores.Length} scores");
                    var userIds = new List<string>();
                    foreach (var score in data.Scores)
                    {
                        userIds.Add(score.userID);
                    }

                    Social.Active.LoadUsers(userIds.ToArray(), (users) =>
                    {
                        var leaderboardData = new List<Leaderboard_ItemData>();
                        foreach (var score in data.Scores)
                        {
                            string playerName = score.userID; // Default to userID
                            foreach (var user in users)
                            {
                                if (user.id == score.userID)
                                {
                                    playerName = user.userName;
                                    break;
                                }
                            }
                            Debug.Log($"<color=yellow>{playerName} => {score.rank} - HS: {score.value}</color>");
                            leaderboardData.Add(new Leaderboard_ItemData(score.rank, playerName, score.value));
                        }

                        callback?.Invoke(leaderboardData);
                    });
                }
                else
                {
                    Debug.LogError("Failed to retrieve leaderboard data.");
                    callback?.Invoke(null);
                }
            });

    }*/


    public void GetLeaderboardData(Action<List<Leaderboard_ItemData>> callback)
    {
        // 1. Safety Check: Ensure we are actually signed in before asking for data
        if (!Social.localUser.authenticated)
        {
            Debug.LogError("GPGS: Cannot get leaderboard. User is not authenticated.");
            callback?.Invoke(null);
            return;
        }

        PlayGamesPlatform.Instance.LoadScores(
            LeaderboardID,
            // 2. CHANGE: Use TopScores. 'PlayerCentered' fails if the player has no score.
            LeaderboardStart.TopScores,
            10,
            LeaderboardCollection.Public,
            LeaderboardTimeSpan.AllTime,
            (data) =>
            {
                if (data.Valid)
                {
                    Debug.Log($"GPGS: Leaderboard data valid. Found {data.Scores.Length} scores.");

                    // If no scores exist yet, return empty list immediately
                    if (data.Scores.Length == 0)
                    {
                        callback?.Invoke(new List<Leaderboard_ItemData>());
                        return;
                    }

                    var userIds = new List<string>();
                    foreach (var score in data.Scores)
                    {
                        userIds.Add(score.userID);
                    }

                    // 3. Load User Profiles (Names)
                    Social.Active.LoadUsers(userIds.ToArray(), (users) =>
                    {
                        var leaderboardData = new List<Leaderboard_ItemData>();

                        // 4. FIX: Handle case where LoadUsers fails (users is null)
                        // This prevents the code from crashing and ensures you still see scores (even without names)
                        if (users == null)
                        {
                            Debug.LogWarning("GPGS: LoadUsers returned null. Showing scores with UserIDs only.");
                            users = new UnityEngine.SocialPlatforms.IUserProfile[0]; // Empty array to prevent crash
                        }

                        foreach (var score in data.Scores)
                        {
                            string playerName = "Unknown"; // Default name

                            // Try to find the name in the loaded users
                            foreach (var user in users)
                            {
                                if (user.id == score.userID)
                                {
                                    playerName = user.userName;
                                    break;
                                }
                            }

                            // Fallback: If name lookup failed/privacy restricted, use ID or "Player"
                            if (playerName == "Unknown") playerName = score.userID;

                            Debug.Log($"Entry: {score.rank} - {playerName} - {score.value}");
                            leaderboardData.Add(new Leaderboard_ItemData(score.rank, playerName, score.value));
                        }

                        callback?.Invoke(leaderboardData);
                    });
                }
                else
                {
                    Debug.LogError("GPGS: Failed to retrieve leaderboard data (data.Valid is false).");
                    callback?.Invoke(null);
                }
            });
    }

}// CLASS
