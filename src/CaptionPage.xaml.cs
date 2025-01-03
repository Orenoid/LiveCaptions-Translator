using System.ComponentModel;
using System.Text;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LiveCaptionsTranslator
{
    public partial class CaptionPage : Page
    {
        private bool autoScrollOriginal = true;
        private bool autoScrollTranslated = true;

        public CaptionPage()
        {
            InitializeComponent();
            DataContext = App.Captions;
            App.Captions.PropertyChanged += CaptionsPropertyChanged;
        }

        private void CaptionsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(App.Captions.Original) && autoScrollOriginal)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    OriginalScroll.ScrollToEnd();
                }), DispatcherPriority.Background);
            }
            else if (e.PropertyName == nameof(App.Captions.Translated))
            {
                if (Encoding.UTF8.GetByteCount(App.Captions.Translated) > 150)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        TranslatedCaption.FontSize = 15;
                    }), DispatcherPriority.Background);
                }
                else
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        TranslatedCaption.FontSize = 18;
                    }), DispatcherPriority.Background);
                }

                if (autoScrollTranslated)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        TranslatedScroll.ScrollToEnd();
                    }), DispatcherPriority.Background);
                }
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                // 检查是否是用户手动滚动（而不是通过代码滚动）
                if (e.ExtentHeightChange == 0)
                {
                    bool isAtBottom = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 1;
                    
                    if (scrollViewer == OriginalScroll)
                    {
                        autoScrollOriginal = isAtBottom;
                    }
                    else if (scrollViewer == TranslatedScroll)
                    {
                        autoScrollTranslated = isAtBottom;
                    }
                }
                // 如果内容高度改变且启用了自动滚动，滚动到底部
                else if ((scrollViewer == OriginalScroll && autoScrollOriginal) ||
                         (scrollViewer == TranslatedScroll && autoScrollTranslated))
                {
                    scrollViewer.ScrollToEnd();
                }
            }
        }
    }
}
