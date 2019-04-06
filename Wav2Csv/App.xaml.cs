using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Prism;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Logging;
using Prism.Unity;
using Prism.Unity.Ioc;


namespace Wav2Csv
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IConverter, Converter>();
            containerRegistry.RegisterSingleton<Converter>();
        }

        protected override Window CreateShell()
        {
            return this.Container.Resolve<Shell>();
        }
    }
}
