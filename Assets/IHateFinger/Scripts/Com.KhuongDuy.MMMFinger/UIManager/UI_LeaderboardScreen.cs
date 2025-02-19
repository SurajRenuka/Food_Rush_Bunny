using Com.KhuongDuy.MMMFinger;
using FancyScrollView.HeliosScrollView;
using GooglePlayGames;
using System.Collections.Generic;
using UnityEngine;

public class UI_LeaderboardScreen : MonoBehaviour
{
    [SerializeField] private GameObject screen;
    [Space]
    [SerializeField] private int selectedIndex = 0;
    [SerializeField] ScrollView scrollView = default;

    [Space]
    [SerializeField] public List<Leaderboard_ItemData> leaderboard_DataList = new List<Leaderboard_ItemData>();

    private void Start()
    {
        scrollView.OnCellClicked(index =>
        {
            selectedIndex = index;
            SelectCell();
        });
        GenerateCells(10);
        scrollView.JumpTo(0);
    }

    /*private void OnEnable()
    {
        GooglePlayGameServicesManager.Instance.GetLeaderboardData((data) =>
        {
            // Handle the leaderboard data here
            leaderboard_DataList.Clear();
            leaderboard_DataList.AddRange(data);

            if (data != null && data.Count > 0)
                GenerateCells(data.Count);
        });
        *//*leaderboard_DataList = new List<Leaderboard_ItemData> { 
            new Leaderboard_ItemData(1, "Test", 99),
            new Leaderboard_ItemData(2, "Test_2", 90),
            new Leaderboard_ItemData(3, "Test_3", 88),
            new Leaderboard_ItemData(4, "Test_4", 80),
            new Leaderboard_ItemData(5, "Test_5", 77),
            new Leaderboard_ItemData(6, "Test_6", 75),
            new Leaderboard_ItemData(7, "Test_7", 70),
            new Leaderboard_ItemData(8, "Test_8", 69),
            new Leaderboard_ItemData(9, "Test_9", 66),
            new Leaderboard_ItemData(10, "Test_10", 62)
        };*//*

        scrollView.JumpTo(0);

        *//*if(leaderboard_DataList.Count <= 0)
            return;
        foreach (var leaderboard in leaderboard_DataList)
        {
            Debug.Log($"<color=green>{leaderboard.index} => {leaderboard.userName} = {leaderboard.highScore}</color>");
        }*//*
    }*/

    public void ShowLeaderboardScreen()
    {
        if (GooglePlayGameServicesManager.Instance.isAuthenticate)
        {
            GooglePlayGameServicesManager.Instance.GetLeaderboardData((data) =>
            {
                // Handle the leaderboard data here
                leaderboard_DataList.Clear();
                leaderboard_DataList.AddRange(data);

                scrollView.UpdateData(leaderboard_DataList);
                /*if (data != null && data.Count > 0)
                    GenerateCells(data.Count);*/
            });
        }


        UIManager.Instance.isLeaderboardShow = true;
        screen.SetActive(true);

        scrollView.JumpTo(0);
        //PlayGamesPlatform.Instance.ShowLeaderboardUI("CgkI9YOPxNwNEAIQAQ");

    }
    public void HideLeaderboardScreen()
    {
        screen.SetActive(false);
        UIManager.Instance.isLeaderboardShow = false;
    }


    void GenerateCells(int dataCount)
    {
        /* var items = Enumerable.Range(0, dataCount)
             .Select(i => new Leaderboard_ItemData(i, "Test", i+10))
             .ToArray();*/
        var items = leaderboard_DataList;

        scrollView.UpdateData(items);
        SelectCell();
    }

    void SelectCell()
    {
        if (scrollView.DataCount == 0)
        {
            return;
        }

        scrollView.UpdateSelection(selectedIndex);
        scrollView.ScrollTo(selectedIndex, 0.4f, EasingCore.Ease.InOutQuint, Alignment.Upper);
    }

}// CLASS
