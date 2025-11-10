using Microsoft.AspNetCore.Components;

namespace Mapsui.Samples.Blazor.Pages;

public partial class Page2 : ComponentBase
{
    protected int ActivePanel { get; set; } = 0;

    protected void SetActivePanel(int index)
    {
        if (ActivePanel == index) return;
        ActivePanel = index;
        StateHasChanged();
    }
}
