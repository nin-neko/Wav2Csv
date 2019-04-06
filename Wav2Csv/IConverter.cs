using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;


namespace Wav2Csv
{
    interface IConverter : INotifyPropertyChanged
    {
        bool Converting { get; }

        void BeginConversion(string sourceFilePath);

        event EventHandler<ProgressEventArgs> ProgressChanged;
        event EventHandler<FaildEventArgs> Failed;
    }

    class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(int progress)
        {
            this.Progress = progress;
        }

        public int Progress { get; }
    }

    class FaildEventArgs : EventArgs
    {
        public FaildEventArgs(Exception ex)
        {
            this.Exception = ex;
        }

        public Exception Exception { get; }
    }
}
