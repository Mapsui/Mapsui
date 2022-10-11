using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps;

namespace Mapsui.Samples.Maui.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            map = InfoLayersSample.CreateMap();


            var allSamples = AllSamples.GetSamples() ?? new List<ISampleBase>();
            Categories = new ObservableCollection<string>(allSamples.Select(s => s.Category).Distinct().OrderBy(c => c));

            //categoryPicker!.ItemsSource = categories.ToList();
            //categoryPicker.SelectedIndexChanged += categoryPicker_SelectedIndexChanged;
            //categoryPicker.SelectedItem = "Info";
        }

        [ObservableProperty]
        Map? map;

        public ObservableCollection<ISample> Samples { get; } = new();
        public ObservableCollection<string> Categories { get; } = new();

        [ICommand]
        private void CategoryChanged()
        { 

        }


        public void Picker_SelectedIndexChanged(object? sender, EventArgs e)
        {
            
        }
    }
}
