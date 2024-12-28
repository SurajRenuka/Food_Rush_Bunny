using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public string theme;
    public bool isChristmasUnlocked;
    public GameObject LockChristmas, SelectedJungle, SelectedChristmas, NotSelectedJungle, NotSelectedChristmas;
    public int PlayerHaveCoins;
    public Text coinsDisplay;
    // Start is called before the first frame update
    void Start()
    {
        theme = PlayerPrefs.GetString("Theme");

        //Give bonus for testing
        /*if (PlayerPrefs.GetInt("IsFirst") != 10)
        {
            PlayerPrefs.SetInt("IsFirst", 10);
            PlayerPrefs.SetInt("Coins", 2100);
        }*/
        PlayerHaveCoins = PlayerPrefs.GetInt("Coins");
        if(PlayerPrefs.GetInt("IsChristmasUnlocked") == 1)
        {
            isChristmasUnlocked = true;
        }
        else
        {
            isChristmasUnlocked = false;
        }
        UpdateUI();
    }

    // Update is called once per frame
    void UpdateUI()
    {
        PlayerHaveCoins = PlayerPrefs.GetInt("Coins");
        coinsDisplay.text = PlayerHaveCoins.ToString();
        if (PlayerPrefs.GetInt("IsChristmasUnlocked") == 1)
        {
            isChristmasUnlocked = true;
        }
        else
        {
            isChristmasUnlocked = false;
        }

        if (isChristmasUnlocked)
        {
            LockChristmas.SetActive(false);
        }
        else {
            if (theme == "Christmas")
            {
                theme = "Jungle";
            }
            LockChristmas.SetActive(true);
            SelectedJungle.SetActive(true);
            NotSelectedJungle.SetActive(false);
            NotSelectedChristmas.SetActive(true);
            SelectedChristmas.SetActive(false);
        }

        if(theme == "Jungle")
        {
            SelectedJungle.SetActive(true);
            SelectedChristmas.SetActive(false);
            NotSelectedJungle.SetActive(false);
            NotSelectedChristmas.SetActive(true);
            SelectedChristmas.SetActive(false);
        }
        if (theme == "Christmas")
        {
            SelectedJungle.SetActive(false);
            NotSelectedJungle.SetActive(true);
            SelectedChristmas.SetActive(true);
            NotSelectedChristmas.SetActive(false);
            LockChristmas.SetActive(false);
        }
    }

    public void BuyChristmasTheme()
    {
        PlayerHaveCoins = PlayerPrefs.GetInt("Coins");
        if (PlayerHaveCoins >= 2000)
        {
            PlayerHaveCoins -= 2000;
            PlayerPrefs.SetInt("IsChristmasUnlocked", 1);
            isChristmasUnlocked = true;
            PlayerPrefs.SetInt("Coins", PlayerHaveCoins);
            theme = "Christmas";
            UpdateUI();
        }
    }
}
