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



    public void GetLeaderboardData(Action<List<Leaderboard_ItemData>> callback)
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

    }

}// CLASS
