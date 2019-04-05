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
    }

    class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(int progress)
        {
            this.Progress = progress;
        }

        public int Progress { get; }
    }
}
