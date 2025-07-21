namespace DocumentProcessor.Master.Services;

public static class OperationNames
{
    public const string InitializeProcess = nameof(InitializeProcess);
    public const string HandleFileReady = nameof(HandleFileReady);
    public const string HandleFileProcessed = nameof(HandleFileProcessed);
    public const string HandleFilePersisted = nameof(HandleFilePersisted);
    public const string HandleFileFailed = nameof(HandleFileFailed);
    public const string InitializeFilesCommand = nameof(InitializeFilesCommand);
    public const string ProcessFileCommand = nameof(ProcessFileCommand);
    public const string PersistFileResultCommand = nameof(PersistFileResultCommand);
    public const string UpdateProcessStatusCommand = nameof(UpdateProcessStatusCommand);
} 