using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]

internal enum ProgressionLoadSource
{
    Primary,
    Backup,
    NewSave
}

internal sealed class ProgressionLoadResult
{
    public GlobalProgressionSave Save { get; }
    public ProgressionLoadSource Source { get; }
    public bool RequiresSave { get; }

    public ProgressionLoadResult(
        GlobalProgressionSave save,
        ProgressionLoadSource source,
        bool requiresSave)
    {
        Save = save;
        Source = source;
        RequiresSave = requiresSave;
    }
}

internal interface IProgressionStore
{
    string Location { get; }

    ProgressionLoadResult Load();
    void Save(GlobalProgressionSave save);
    void Reset();
}
