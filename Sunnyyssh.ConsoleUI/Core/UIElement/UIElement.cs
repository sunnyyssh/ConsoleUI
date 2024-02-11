﻿using System.Diagnostics;

namespace Sunnyyssh.ConsoleUI;

internal delegate void RedrawElementEventHandler(UIElement sender, RedrawElementEventArgs args);

internal record RedrawElementEventArgs(DrawState State);

public abstract class UIElement
{
    public bool IsDrawn { get; private set; } = false; // I don't know how to do it. It's kinda cringe.

    public Size Size { get; }

    protected int ActualWidth { get; private set; }
    
    protected int ActualHeight { get; private set; }

    public OverlappingPriority OverlappingPriority { get; }
    
    internal event RedrawElementEventHandler? RedrawElement;

    protected void Redraw(DrawState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        RedrawElement?.Invoke(this, new RedrawElementEventArgs(state));
    }

    internal DrawState RequestDrawState(DrawOptions options)
    {
        ActualWidth = options.Width;
        ActualHeight = options.Height;
        return GetDrawState(ActualWidth, ActualHeight);
    }

    protected abstract DrawState GetDrawState(int width, int height);

    internal void OnDraw() // Looks a bit weird but... TODO don't forget to invoke them in UIManager
    {
        IsDrawn = true;
    }

    internal void OnRemove() // Looks a bit weird but... TODO don't forget to invoke them in UIManager
    {
        IsDrawn = false;
    }

    protected UIElement(Size size, OverlappingPriority overlappingPriority)
    {
        Size = size;
        OverlappingPriority = overlappingPriority;
    }
}