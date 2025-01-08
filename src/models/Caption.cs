using System.Windows.Automation;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

using LiveCaptionsTranslator.controllers;

namespace LiveCaptionsTranslator.models
{
    public class Caption : INotifyPropertyChanged
    {
        private static Caption? instance = null;
        public event PropertyChangedEventHandler? PropertyChanged;

        private static readonly char[] PUNC_EOS = ".?!。？！".ToCharArray();
        private static readonly char[] PUNC_COMMA = ",，、—\n".ToCharArray();

        private ObservableCollection<string> captions = new();

        public bool PauseFlag { get; set; } = false;
        public bool TranslateFlag { get; set; } = false;
        private bool EOSFlag { get; set; } = false;

        public ObservableCollection<string> Captions
        {
            get => captions;
            private set
            {
                captions = value;
                OnPropertyChanged("Captions");
            }
        }

        private Caption() { }

        public static Caption GetInstance()
        {
            if (instance != null)
                return instance;
            instance = new Caption();
            return instance;
        }

        public void OnPropertyChanged([CallerMemberName] string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public void Sync()
        {
            int idleCount = 0;
            int syncCount = 0;

            while (true)
            {
                if (PauseFlag || App.Window == null)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                string fullText = GetCaptions(App.Window).Trim();
                if (string.IsNullOrEmpty(fullText))
                    continue;
                foreach (char eos in PUNC_EOS)
                    fullText = fullText.Replace($"{eos}\n", $"{eos}");

                // 如果列表为空，直接添加第一行
                if (Captions.Count == 0)
                {
                    Captions.Add(fullText);
                    continue;
                }

                // 检查是否包含最后完整句子（倒数第二句，如果存在的话）
                string lastCompleteText = Captions.Count > 1 ? Captions[^2] : "";
                string newText = fullText;

                if (!string.IsNullOrEmpty(lastCompleteText))
                {
                    if (!fullText.Contains(lastCompleteText))
                    {
                        // 找不到最后完整句子，直接使用全部文本
                        newText = fullText;
                    }
                    else
                    {
                        // 找到最后完整句子，从它之后开始处理
                        int startIndex = fullText.IndexOf(lastCompleteText) + lastCompleteText.Length;
                        newText = fullText[startIndex..].Trim();
                    }
                }

                // 检查新文本中是否有完整句子
                int newTextEOSIndex = newText.LastIndexOfAny(PUNC_EOS);
                if (newTextEOSIndex != -1)
                {
                    string beforeEOS = newText[..(newTextEOSIndex + 1)];
                    string afterEOS = newText[(newTextEOSIndex + 1)..].Trim();

                    // 更新或添加完整句子
                    if (Captions.Count > 1)
                        Captions[^1] = beforeEOS;
                    else
                        Captions.Add(beforeEOS);

                    // 总是添加新的实时字幕行，即使是空的
                    Captions.Add(afterEOS);

                    syncCount = 0;
                    EOSFlag = true;
                }
                else if (Captions[^1] != newText)
                {
                    // 更新实时字幕
                    Captions[^1] = newText;
                    idleCount = 0;
                    syncCount++;
                    EOSFlag = false;
                }
                else
                {
                    idleCount++;
                }

                if (syncCount > App.Settings.MaxSyncInterval || 
                    idleCount == App.Settings.MaxIdleInterval)
                {
                    syncCount = 0;
                    // TranslateFlag = true;
                }
                Thread.Sleep(50);
            }
        }

        public async Task Translate()
        {
            // var controller = new TranslationController();
            // while (true)
            // {
            //     for (int pauseCount = 0; PauseFlag; pauseCount++)
            //     {
            //         if (pauseCount > 60 && App.Window != null)
            //         {
            //             App.Window = null;
            //             LiveCaptionsHandler.KillLiveCaptions();
            //         }
            //         Thread.Sleep(1000);
            //     }

            //     if (TranslateFlag)
            //     {
            //         Translated = await controller.TranslateAndLogAsync(Original);
            //         TranslateFlag = false;
            //         if (EOSFlag)
            //             Thread.Sleep(1000);
            //     }
            //     Thread.Sleep(50);
            // }
            await Task.CompletedTask; // 保持方法签名不变
        }

        public static string GetCaptions(AutomationElement window)
        {
            var captionsTextBlock = LiveCaptionsHandler.FindElementByAId(window, "CaptionsTextBlock");
            if (captionsTextBlock == null)
                return string.Empty;
            return captionsTextBlock.Current.Name;
        }
    }
}
