using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;

using Reactive.Bindings;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using Reactive.Bindings.Interactivity;
using Reactive.Bindings.Notifiers;


namespace Wav2Csv
{
    sealed class ShellViewModel
    {
        public ShellViewModel(IConverter converter)
        {
            this.BrowseCommand.Subscribe(async () =>
            {
                var dialog = new Microsoft.Win32.OpenFileDialog()
                {
                    Multiselect = false,
                    Filter = "Audio File|*.wav",
                };
                var result = dialog.ShowDialog(App.Current.MainWindow);
                if (!result.HasValue || !result.Value) return;

                this.SourceFilePath.Value = dialog.FileName;
            });

            this.Progress = Observable.FromEventPattern<ProgressEventArgs>(h => converter.ProgressChanged += h, h => converter.ProgressChanged -= h)
                .Select(x => x.EventArgs.Progress)
                .ObserveOnUIDispatcher()
                .ToReadOnlyReactivePropertySlim();

            this.ConvertCommand = new[]
            {
                this.SourceFilePath.Select(x => File.Exists(x)),
                converter.ObserveProperty(x => x.Converting).Select(x => !x),
            }
            .CombineLatestValuesAreAllTrue()
            .ObserveOnUIDispatcher()
            .ToAsyncReactiveCommand()
            .WithSubscribe(async () =>
            {
                converter.BeginConversion(this.SourceFilePath.Value);
            });
        }

        public ReactivePropertySlim<string> SourceFilePath { get; } = new ReactivePropertySlim<string>();
        public AsyncReactiveCommand BrowseCommand { get; } = new AsyncReactiveCommand();
        public ReadOnlyReactivePropertySlim<int> Progress { get; }
        public AsyncReactiveCommand ConvertCommand { get; } = new AsyncReactiveCommand();
    }
}
