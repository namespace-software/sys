public static class ScenarioLaunchRequest
{
    public static bool HasPending { get; private set; }
    public static EnumScenarioMode Mode { get; private set; }
    public static string DisplayName { get; private set; }

    public static void Set(EnumScenarioMode mode, string displayName = null)
    {
        Mode = mode;
        DisplayName = displayName;
        HasPending = true;
    }

    public static bool TryConsume(out EnumScenarioMode mode)
    {
        mode = Mode;
        if (!HasPending)
            return false;

        HasPending = false;
        return true;
    }
}
