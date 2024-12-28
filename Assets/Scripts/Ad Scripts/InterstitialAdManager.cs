using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterstitialAdManager : MonoBehaviour
{
    private InterstitialAd _interstitialAd;
    public bool _isAdOpen = false;
    //private string _adUnitId = "ca-app-pub-3940256099942544/1033173712"; //Test
    private string _adUnitId = "ca-app-pub-5973265602264283/5236010264"; // Real

    void Start()
    {
            LoadInterstitialAd();
    }


    // Load the interstitial ad
    public void LoadInterstitialAd()
    {
       // Clean up the old ad before loading a new one.
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        Debug.Log("Loading the interstitial ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        InterstitialAd.Load(_adUnitId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                          + ad.GetResponseInfo());

                _interstitialAd = ad;

                RegisterEventHandlers(_interstitialAd);
            });
    }

    // Show the interstitial ad
    public void ShowInterstitialAd()
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            _interstitialAd.Show();
            _isAdOpen = true;
        }
        else
        {
            Debug.LogError("Interstitial ad is not ready yet.");
        }
    }

    // Register event handlers for the interstitial ad
    private void RegisterEventHandlers(InterstitialAd interstitialAd)
    {
        interstitialAd.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log("Ad paid: " + adValue.Value + " " + adValue.CurrencyCode);
        };

        interstitialAd.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Ad impression recorded.");
        };

        interstitialAd.OnAdClicked += () =>
        {
            Debug.Log("Ad clicked.");
        };

        interstitialAd.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Ad full-screen content opened.");
            // Pause game or audio if necessary
        };

        interstitialAd.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Ad full-screen content closed.");
            // Resume game or audio if paused
            _isAdOpen = false;
            LoadInterstitialAd(); // Preload next ad
        };

        interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Ad failed to open: " + error);
            LoadInterstitialAd(); // Try loading a new ad
        };
    }

    // Clean up when done with the ad
    private void OnDestroy()
    {
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
        }
    }
}
