using System;

namespace FancyScrollView.HeliosScrollView
{
    class Context : FancyScrollRectContext
    {
        public int SelectedIndex = -1;
        public Action<int> OnCellClicked;
    }
}
