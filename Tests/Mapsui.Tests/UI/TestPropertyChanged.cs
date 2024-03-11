using System.ComponentModel;
using System.Runtime.CompilerServices;
using Mapsui.UI;

namespace Mapsui.Tests.UI;

public class TestPropertyChanged : INotifyPropertyChanged
{
    private PropertyChangedWeakEventManager? _eventMangerPropertyChanged;

    /// <summary>
    /// Called whenever a property changed
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged
    {
        add
        {
            _eventMangerPropertyChanged ??= new();
            _eventMangerPropertyChanged.AddListener(this, value);
        }
        remove => _eventMangerPropertyChanged?.RemoveListener(this, value);
    }

    public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        _eventMangerPropertyChanged?.RaiseEvent(this, new PropertyChangedEventArgs(propertyName));
    }
}
