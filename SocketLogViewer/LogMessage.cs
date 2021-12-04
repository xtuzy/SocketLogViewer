using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketLogViewer
{
    public class LogMessage: INotifyPropertyChanged
    {
        string log;
        /// <summary>
        /// Log message
        /// </summary>
        public string Log
        {
            get { return log; }
            set
            {
                log = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Log"));
                }
            }
        }

        string platform;
        /// <summary>
        /// iOS,Android,WPF,Mac
        /// </summary>
        public string Platform
        {
            get { return platform; }
            set
            {
                platform = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Platform"));
                }
            }
        }

        string time;
        

        /// <summary>
        /// H/M/S
        /// </summary>
        public string Time
        {
            get { return time; }
            set
            {
                time = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Time"));
                }
            }
        }

        private string color;
        /// <summary>
        /// 该行颜色
        /// </summary>
        public string Color
        {
            get { return color; }
            set
            {
                color = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Color"));
                }
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
