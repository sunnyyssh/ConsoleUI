// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Handles <see cref="IFocusable.ForceTakeFocus"/> event.
/// </summary>
/// <param name="sender"><see cref="IFocusable"/> that wants to take focus.</param>
public delegate void ForceTakeFocusHandler(IFocusable sender);

/// <summary>
/// Handles <see cref="IFocusable.ForceLoseFocus"/> event.
/// </summary>
/// <param name="sender"><see cref="IFocusable"/> that wants to lose focus.</param>
public delegate void ForceLoseFocusHandler(IFocusable sender);

/// <summary>
/// Represents an object that can handle <see cref="ConsoleKeyInfo"/> pressed keys.
/// If you want to specify that object should take part in a focus flow you should implement <see cref="IFocusable"/>.
/// </summary>
public interface IFocusable
{
    /// <summary>
    /// Whether it waits for focus.
    /// </summary>
    public bool IsWaitingFocus { get; } 
    
    /// <summary>
    /// Whether it is focused now. 
    /// </summary>
    public bool IsFocused { get; }

    /// <summary>
    /// Takes focus.
    /// </summary>
    protected internal void TakeFocus();
    
    /// <summary>
    /// Loses focus.
    /// </summary>    
    protected internal void LoseFocus();
    
    /// <summary>
    /// Handles pressed key.
    /// </summary>
    /// <param name="keyInfo">Pressed key.</param>
    /// <returns></returns>
    protected internal bool HandlePressedKey(ConsoleKeyInfo keyInfo);
    
    /// <summary>
    /// Event that occurs when this wants to take focus.
    /// </summary>
    protected internal event ForceTakeFocusHandler ForceTakeFocus;
    
    /// <summary>
    /// Event that occurs when this wants to lose focus.
    /// </summary>
    protected internal event ForceLoseFocusHandler ForceLoseFocus;
}