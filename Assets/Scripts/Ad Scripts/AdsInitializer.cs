using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsInitializer : MonoBehaviour
{
    public void Start()
    {        
        MobileAds.Initialize(initStatus => { });
    }
}
