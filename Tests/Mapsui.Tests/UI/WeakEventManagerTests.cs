using System.ComponentModel;
using Mapsui.Fetcher;
using Mapsui.UI;
using NUnit.Framework;

namespace Mapsui.Tests.UI;

[TestFixture]
public class WeakEventManagerTests
{
    private bool _changed;

    [Test]
    public void AddEventWorks_PropertyChanged()
    {
        TestPropertyChanged propertyChanged = new();
        _changed = false;
        propertyChanged.PropertyChanged += TestPropertyChanged;
        propertyChanged.OnPropertyChanged();
        Assert.That(_changed, Is.EqualTo(true));
    }

    private void TestPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _changed = true;
    }

    [Test]
    public void AddEventWorks_DataChanged()
    {
        TestDataChanged dataChanged = new();
        _changed = false;
        dataChanged.DataChanged += TestDataChanged;
        dataChanged.OnDataChanged();
        Assert.That(_changed, Is.EqualTo(true));
    }

    private void TestDataChanged(object sender, DataChangedEventArgs e)
    {
        _changed = true;
    }

    private void TestDataChanged2(object sender, DataChangedEventArgs e)
    {
        _changed = true;
    }

    [Test]
    public void RemoveEventWorks_DataChanged()
    {
        TestDataChanged dataChanged = new();
        _changed = false;
        DataChangedWeakEventManager eventManager = new();
        dataChanged.DataChanged += TestDataChanged;
        dataChanged.DataChanged -= TestDataChanged;
        dataChanged.OnDataChanged();
        eventManager.RaiseEvent(this, new DataChangedEventArgs("LayerName"));
        Assert.That(_changed, Is.EqualTo(false));
    }

    [Test]
    public void RemoveEventWorks_DataChanged_DifferentMethods()
    {
        TestDataChanged dataChanged = new();
        _changed = false;
        DataChangedWeakEventManager eventManager = new();
        dataChanged.DataChanged += TestDataChanged2;
        dataChanged.DataChanged += TestDataChanged;
        dataChanged.DataChanged -= TestDataChanged;
        dataChanged.OnDataChanged();
        Assert.That(_changed, Is.EqualTo(true));
    }
}
