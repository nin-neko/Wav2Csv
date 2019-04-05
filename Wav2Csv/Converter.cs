using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.IO;

using Prism.Mvvm;

using CsvHelper;
using NAudio;
using NAudio.Wave;


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
                            this.RaiseProgressChanged(0);
                            this.Converting = true;
                            try
                            {
                                var left = new List<double>();
                                var right = new List<double>();
                                var isStereo = false;
                                var samplingTime = 0d;

                                var allProgress = 0L;
                                var progress = 0L;
                                void NotifyProgress() => this.RaiseProgressChanged((int)(100 * progress++ / allProgress));

                                using (var waveReader = new WaveFileReader(filePath))
                                {
                                    isStereo = waveReader.WaveFormat.Channels == 2;
                                    samplingTime = 1d / waveReader.WaveFormat.SampleRate;

                                    allProgress = waveReader.SampleCount * 2;

                                    while (true)
                                    {
                                        var signals = waveReader.ReadNextSampleFrame();
                                        if (signals == null) break;
                                        left.Add(signals[0]);
                                        if (isStereo)
                                        {
                                            right.Add(signals[1]);                                            
                                        }

                                        NotifyProgress();
                                    }
                                }

                                var builder = new StringBuilder();
                                using (var textWriter = new StringWriter(builder))
                                using (var csvWriter = new CsvWriter(textWriter))
                                {
                                    csvWriter.WriteField("Time");
                                    csvWriter.WriteField("Left");
                                    if (isStereo)
                                    {
                                        csvWriter.WriteField("Right");
                                    }
                                    csvWriter.NextRecord();

                                    for (int i = 0; i < left.Count; i++)
                                    {
                                        var time = i * samplingTime;
                                        csvWriter.WriteField(time);

                                        csvWriter.WriteField(left[i]);
                                        if (isStereo)
                                        {
                                            csvWriter.WriteField(right[i]);
                                        }
                                        csvWriter.NextRecord();

                                        NotifyProgress();
                                    }
                                }

                                var csvFilePath = Path.ChangeExtension(filePath, "csv");
                                if (File.Exists(csvFilePath))
                                {
                                    File.Delete(csvFilePath);
                                }
                                File.WriteAllText(csvFilePath, builder.ToString());
                            }
                            finally
                            {
                                this.RaiseProgressChanged(100);
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
