using UnityEngine;
using UnityEngine.UI;

namespace FancyScrollView.HeliosScrollView
{
    class Cell : FancyScrollRectCell<Leaderboard_ItemData, Context>
    {
        [SerializeField] private Text indexText = default;
        [SerializeField] private Text userNameText = default;
        [SerializeField] private Text highScoreText = default;
        [Space]
        [SerializeField] private Image bg = default;
        [Space]
        [SerializeField] private Button button = default;

        [Space(5)]
        [Header("BG SPRITE")]
        [SerializeField] private Sprite[] bgArray;

        public override void Initialize()
        {
            button.onClick.AddListener(() => Context.OnCellClicked?.Invoke(Index));
        }

        public override void UpdateContent(Leaderboard_ItemData itemData)
        {
            /*message.text = itemData.Message;

            var selected = Context.SelectedIndex == Index;
            image.color = selected
                ? new Color32(0, 255, 255, 100)
                : new Color32(255, 255, 255, 77);*/

            SetBG(itemData.index);

            if (itemData.index > 3)
                indexText.text = $"{itemData.index+1}";
            else
                indexText.text =string.Empty;

            userNameText.text = itemData.userName;
            highScoreText.text = itemData.highScore.ToString();
        }

        protected override void UpdatePosition(float normalizedPosition, float localPosition)
        {
            base.UpdatePosition(normalizedPosition, localPosition);

            /*var wave = Mathf.Sin(normalizedPosition * Mathf.PI * 2) * 65;
            transform.localPosition += Vector3.right * wave;*/
        }

        private void SetBG(int index)
        {
            switch (index)
            {
                case 1:
                    bg.sprite = bgArray[0];
                    break;
                case 2:
                    bg.sprite = bgArray[1];
                    break;
                case 3:
                    bg.sprite = bgArray[2];
                    break;

                default:
                    bg.sprite = bgArray[3];
                    break;
            }
        }

    }//CLASS
}
