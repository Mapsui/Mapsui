using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Mapsui.Samples.Common.Maps;

namespace Mapsui.Samples.Maui.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            map = InfoLayersSample.CreateMap();
        }

        [ObservableProperty]
        Map? map;
    }
}
