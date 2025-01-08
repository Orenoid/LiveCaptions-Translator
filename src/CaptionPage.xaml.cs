using System.ComponentModel;
using System.Text;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Specialized;

namespace LiveCaptionsTranslator
{
    public partial class CaptionPage : Page
    {
        public CaptionPage()
        {
            InitializeComponent();
            DataContext = App.Captions;
            App.Captions.Captions.CollectionChanged += Captions_CollectionChanged;
        }

        private void Captions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (App.Captions.Captions.Count == 0) return;

            var lastItem = CaptionList.Items[^1];
            var container = CaptionList.ItemContainerGenerator.ContainerFromItem(lastItem) as ListBoxItem;
            if (container == null) return;

            var textBlock = container.ContentTemplate.FindName("CaptionText", container) as TextBlock;
            if (textBlock == null) return;

            if (Encoding.UTF8.GetByteCount(lastItem.ToString() ?? "") > 150)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    textBlock.FontSize = 13;
                }), DispatcherPriority.Background);
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    textBlock.FontSize = 15;
                }), DispatcherPriority.Background);
            }

            // 自动滚动到最后一项
            CaptionList.ScrollIntoView(lastItem);
        }
    }
}
