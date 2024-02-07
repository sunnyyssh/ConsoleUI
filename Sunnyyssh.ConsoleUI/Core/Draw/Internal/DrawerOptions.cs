namespace Sunnyyssh.ConsoleUI;

internal record DrawerOptions(Color DefaultBackground, Color DefaultForeground, bool ThrowOnBorderConflicts, bool ClearOnStart = false);
// TODO ThrowOnBorderConflicts and ClearOnStart are hollow