using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;

using Prism.Mvvm;


namespace Wav2Csv
{
    class Converter : BindableBase, IConverter
    {
        private readonly Channel<string> converter = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        });

        public Converter()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var reader = converter.Reader;
                    while (await reader.WaitToReadAsync())
                    {
                        while (reader.TryRead(out var filePath))
                        {
                            this.Converting = true;
                            try
                            {
                                for (int i = 0; i < 10; i++)
                                {
                                    await Task.Delay(100);
                                    this.RaiseProgressChanged(i * 10);
                                }
                                
                            }
                            finally
                            {
                                this.Converting = false;
                            }
                        }
                    }
                }
            });
        }

        private bool converting;
        public bool Converting
        {
            get => converting;
            private set => this.SetProperty(ref converting, value);
        }

        public void BeginConversion(string sourceFilePath)
        {
            converter.Writer.TryWrite(sourceFilePath);
        }

        public event EventHandler<ProgressEventArgs> ProgressChanged;

        private void RaiseProgressChanged(int progress)
            => this.ProgressChanged?.Invoke(this, new ProgressEventArgs(progress));
    }
}
