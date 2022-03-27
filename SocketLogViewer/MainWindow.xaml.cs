using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SocketLogViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<LogMessage> messages = new ObservableCollection<LogMessage>();
        MySettings Setting;
        List<string> Colors = new List<string>
            {
                "#bcaaa4","#ffab91",
                "#ffcc80","#ffe082","#ffe082","#fff59d",
                "#e6ee9c","#c5e1a5","#80deea","#90caf9",
                "#b39ddb","#f48fb1","#1976d2"
            };
        public MainWindow()
        {
            InitializeComponent();

            //读取设置
            Setting = MySettings.Read();
            if (Setting == null)
                Setting = new MySettings() { KeyWords = new List<KeyValuePair<string, string>>() };
            foreach (var tagAndColor in Setting.KeyWords)
            {
                CreateTagButton(tagAndColor);
            }
            //排除已经有的颜色
            var app = MessageServerApp.MessageServerApp.CreatServerAppProgram();
            this.Title = $"LogViewer {app.MyIp}:{MessageServerApp.MessageServerApp.Port}";

            logListView.ItemsSource = messages;

            var str = new StringBuilder();
            /*app.AcceptedMessageEvent += (s) =>
            {
                if(logs.Count > 60)
                {
                    logs.RemoveRange(0, 15);
                }

                if (s.Contains('\n'))
                {
                    var arr = s.Split('\n');
                    str.Append(arr[0]);

                    var sentence = str.ToString() + '\n';
                    var color = ColorSetting(sentence);
                    richTextBox.Dispatcher.Invoke(() =>
                    {
                       
                        richTextBox.AppendText(sentence, color);
                        richTextBox.ScrollToEnd();//自动滚动到底部
                        logs.Add(new KeyValuePair<string, string>(sentence,color));
                        
                    });

                    if (arr.Length > 2)
                    {
                        for (var index = 1; index < arr.Length - 2; index++)
                        {
                            sentence = arr[index] + '\n';
                            color = ColorSetting(sentence);
                            richTextBox.Dispatcher.Invoke(() =>
                            {
                                richTextBox.AppendText(sentence, color);
                                logs.Add(new KeyValuePair<string, string>(sentence, color));
                            });
                        }
                    }

                    str.Clear();
                    str.Append(arr[arr.Length - 1]);
                }
                else
                {
                    str.Append(s);
                }
            };*/

            app.AcceptedMessageEvent += (s) =>
            {
                logListView.Dispatcher.Invoke(() =>
                {
                    if (messages.Count > 300)
                    {
                        for (var i = 0; i < 15; i++)
                            messages.RemoveAt(0);
                    }
                });

                if (s.Contains('\n'))
                {
                    var arr = s.Split('\n');
                    str.Append(arr[0]);

                    var sentence = str.ToString();
                    var color = ColorSetting(sentence);
                    /* richTextBox.Dispatcher.Invoke(() =>
                     {

                         richTextBox.AppendText(sentence, color);
                         richTextBox.ScrollToEnd();//自动滚动到底部
                         logs.Add(new KeyValuePair<string, string>(sentence, color));

                     });*/
                    logListView.Dispatcher.Invoke(() =>
                    {
                        var s = sentence.Split('~');
                        messages.Add(new LogMessage() { Log = s[0], Platform = s[1], Time = s[2], Color = color });
                    });
                    if (arr.Length > 2)
                    {
                        for (var index = 1; index < arr.Length - 2; index++)
                        {
                            sentence = arr[index];
                            color = ColorSetting(sentence);
                            /*richTextBox.Dispatcher.Invoke(() =>
                            {
                                richTextBox.AppendText(sentence, color);
                                logs.Add(new KeyValuePair<string, string>(sentence, color));
                            });*/

                            logListView.Dispatcher.Invoke(() =>
                            {
                                var s = sentence.Split('~');
                                messages.Add(new LogMessage() { Log = s[0], Platform = s[1], Time = s[2], Color = color });
                            });

                        }
                    }

                    Debug.WriteLine($"messages:{messages.Count}");
                    str.Clear();
                    str.Append(arr[arr.Length - 1]);
                }
                else
                {
                    str.Append(s);
                }
            };
        }

        string ColorSetting(string s)
        {
            foreach (var keyValue in Setting.KeyWords)
            {
                if (s.Contains(keyValue.Key))//不考虑含有多个关键词的
                {
                    return keyValue.Value;
                }
            }
            return "White";
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            messages.Clear();
            //TextRange tr1 = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            //tr1.Text = "";
        }

        private void AddKeyWordsButton_Click(object sender, RoutedEventArgs e)
        {
            if (KeyWordsInputTextBox.Text == "")
                return;
            if (Setting.KeyWords.Count == 10)//限制不超过十个关键词
                return;
            var tagText = KeyWordsInputTextBox.Text;
            var color = ColorSelect();
            var tagAndColor = new KeyValuePair<string, string>(tagText, color);
            CreateTagButton(tagAndColor);
            Setting.KeyWords.Add(tagAndColor);
            MySettings.Save(Setting);
            KeyWordsInputTextBox.Text = "";
        }

        /// <summary>
        /// 从颜色列表中选择一个
        /// </summary>
        /// <returns></returns>
        string ColorSelect()
        {
            //浅色,部分选自Material
            var color = Colors[0];
            Colors.RemoveAt(0);
            return color;
        }

        void CreateTagButton(KeyValuePair<string, string> tagAndColor)
        {
            var tagButton = new Button() { Content = tagAndColor.Key, Background = (SolidColorBrush)(new BrushConverter().ConvertFromString(tagAndColor.Value)) };

            //添加
            var tempMessages = new LogMessage[messages.Count];
            messages.CopyTo(tempMessages, 0);
            foreach (var item in tempMessages)
            {
                if (item.Log.Contains(tagAndColor.Key))
                    item.Color = tagAndColor.Value;
            }
            messages.Clear();
            foreach (var item in tempMessages)
            {
                messages.Add(item);
            }

            //移除
            tagButton.Click += (sender, e) =>
            {
                KeyWordsContainer.Children.Remove(tagButton);
                KeyValuePair<string, string> keyValuePair;
                foreach (var keyValue in Setting.KeyWords)
                {
                    if (keyValue.Key == tagAndColor.Key)
                    {
                        Setting.KeyWords.Remove(keyValue);
                        var tempMessages = new LogMessage[messages.Count];
                        messages.CopyTo(tempMessages, 0);
                        foreach (var item in tempMessages)
                        {
                            if (item.Log.Contains(tagAndColor.Key) && item.Color == tagAndColor.Value)
                                item.Color = "White";
                        }
                        messages.Clear();
                        foreach (var item in tempMessages)
                        {
                            messages.Add(item);
                        }
                        break;
                    }
                }
                Colors.Add(tagAndColor.Value);
                MySettings.Save(Setting);
            };
            KeyWordsContainer.Children.Insert(0, tagButton);
        }
    }

    public static class RichTextBoxExtension
    {

        /// <summary>
        /// 参考:https://stackoverflow.com/a/23402165/13254773
        /// </summary>
        /// <param name="box"></param>
        /// <param name="text"></param>
        /// <param name="color">"CornflowerBlue"或者"0xffffff"</param>
        public static void AppendText(this RichTextBox box, string text, string color)
        {
            BrushConverter bc = new BrushConverter();
            TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            tr.Text = text;
            try
            {
                tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                    bc.ConvertFromString(color));
            }
            catch (FormatException) { }
        }
    }
}
