using System;

namespace MandagsSpil.Client.Layout.AppbarComponents;

public class AppbarState
{
    private string? _appBarText =  "Test";

    public string AppBarText
    {
        get => _appBarText ?? string.Empty;
        set
        {
            _appBarText = value;
            NotifyStateChanged();
        }
    }

    private bool _hamburger { get; set; } = true;
    public bool Hamburger
    {
        get => _hamburger;
        set
        {
            _hamburger = value;
            NotifyStateChanged();
        }
    }
    private bool _backButton { get; set; }
    public bool BackButton
    {
        get => _backButton;
        set
        {
            _backButton = value;
            NotifyStateChanged();
        }
    }

    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();
}
