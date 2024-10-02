﻿using System.Windows.Automation;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LiveCaptionsTranslator
{
    public class Caption : INotifyPropertyChanged
    {
        private static Caption? instance = null;
        public event PropertyChangedEventHandler? PropertyChanged;

        private static readonly char[] PUNC_EOS = ".?!。？！".ToCharArray();
        private static readonly char[] PUNC_COMMA = ",，、—\n".ToCharArray();

        private string original = "";
        private string translated = "";
        private char _lastChar = '\0';

        private int maxIdleInterval;
        private int maxSyncInterval;
        private int _idleCount = 0;
        private int _syncCount = 0;

        public string Original
        {
            get => original;
            set
            {
                original = value;
                OnPerpertyChanged("Original");
            }
        }
        public string Translated
        {
            get => translated;
            set
            {
                translated = value;
                OnPerpertyChanged("Translated");
            }
        }

        private Caption()
        {
            maxIdleInterval = 10;
            maxSyncInterval = 5;
        }

        private Caption(int maxIdleInterval, int maxSyncInterval)
        {
            this.maxIdleInterval = maxIdleInterval;
            this.maxSyncInterval = maxSyncInterval;
        }

        public static Caption GetInstance()
        {
            if (instance != null)
                return instance;
            instance = new Caption();
            return instance;
        }

        public static Caption GetInstance(int maxIdleInterval, int maxSyncInterval)
        {
            if (instance != null)
                return instance;
            instance = new Caption(maxIdleInterval, maxSyncInterval);
            return instance;
        }

        public void OnPerpertyChanged([CallerMemberName] string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public void Sync(AutomationElement window)
        {
            while (true)
            {
                string fullText = GetCaptions(window).Trim();
                if (string.IsNullOrEmpty(fullText))
                    continue;
                foreach (char eos in PUNC_EOS)
                    fullText = fullText.Replace($"{eos}\n", $"{eos}");

                int lastEOSIndex = -1;
                for (int i = fullText.Length; i > 0; i--)
                {
                    if (Array.IndexOf(PUNC_EOS, fullText[i - 1]) == -1)
                    {
                        lastEOSIndex = fullText[0..i].LastIndexOfAny(PUNC_EOS);
                        break;
                    }
                }

                string latestCaption = fullText.Substring(lastEOSIndex + 1);
                while (Encoding.UTF8.GetByteCount(latestCaption) > 100)
                {
                    int commaIndex = latestCaption.IndexOfAny(PUNC_COMMA);
                    if (commaIndex < 0 || commaIndex + 1 == latestCaption.Length)
                        break;
                    latestCaption = latestCaption.Substring(commaIndex + 1);
                }
                latestCaption = latestCaption.Replace("\n", "——");

                if (Original.CompareTo(latestCaption) != 0)
                {
                    _idleCount = 0;
                    _syncCount++;
                    Original = latestCaption;
                    _lastChar = latestCaption[^1];
                }
                else
                {
                    _idleCount++;
                }
                Thread.Sleep(50);
            }
        }

        public async Task Translate()
        {
            while (true)
            {
                if (_idleCount == maxIdleInterval || _syncCount > maxSyncInterval ||
                    Array.IndexOf(PUNC_EOS, _lastChar) != -1 || Array.IndexOf(PUNC_COMMA, _lastChar) != -1)
                {
                    _syncCount = 0;
                    Translated = await TranslateAPI.OpenAI(Original);
                    if (Array.IndexOf(PUNC_EOS, _lastChar) != -1)
                        Thread.Sleep(1000);
                }
                Thread.Sleep(50);
            }
        }

        static string GetCaptions(AutomationElement window)
        {
            var treeWalker = TreeWalker.RawViewWalker;
            return GetCaptions(treeWalker, window);
        }

        static string GetCaptions(TreeWalker walker, AutomationElement window)
        {
            var stack = new Stack<AutomationElement>();
            stack.Push(window);

            while (stack.Count > 0)
            {
                var element = stack.Pop();
                if (element.Current.AutomationId.CompareTo("CaptionsTextBlock") == 0)
                    return element.Current.Name;

                var child = walker.GetFirstChild(element);
                while (child != null)
                {
                    stack.Push(child);
                    child = walker.GetNextSibling(child);
                }
            }
            return string.Empty;
        }
    }
}