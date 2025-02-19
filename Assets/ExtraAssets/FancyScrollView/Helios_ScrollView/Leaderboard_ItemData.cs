using System;

namespace FancyScrollView.HeliosScrollView
{
    [Serializable]
    public class Leaderboard_ItemData
    {

        public int index;
        public string userName;
        public long highScore;
        

        public Leaderboard_ItemData(int _Index, string _UserName, long _HighScore)
        {
            index = _Index;
            userName = _UserName;
            highScore = _HighScore;
        }
    }// CLASS
}
