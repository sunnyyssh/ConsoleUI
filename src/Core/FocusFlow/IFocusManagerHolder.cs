// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Represents an object that holds <see cref="FocusFlowManager"/> instance and can manages focus flow.
/// </summary>
internal interface IFocusManagerHolder : IFocusable
{
    /// <summary>
    /// Gets <see cref="FocusFlowManager"/> instance.
    /// </summary>
    /// <returns><see cref="FocusFlowManager"/> instance.</returns>
    protected internal FocusFlowManager GetFocusManager();
}