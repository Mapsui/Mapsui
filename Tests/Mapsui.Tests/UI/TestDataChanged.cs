using Mapsui.Fetcher;
using Mapsui.UI;

namespace Mapsui.Tests.UI;

public class TestDataChanged
{
    private DataChangedWeakEventManager? _eventMangerDataChanged;

    /// <summary>
    /// Called whenever a property changed
    /// </summary>
    public event DataChangedEventHandler? DataChanged
    {
        add
        {
            _eventMangerDataChanged ??= new();
            _eventMangerDataChanged.AddListener(this, value);
        }
        remove => _eventMangerDataChanged?.RemoveListener(this, value);
    }

    public virtual void OnDataChanged()
    {
        _eventMangerDataChanged?.RaiseEvent(this, new DataChangedEventArgs("LayerName"));
    }
}
