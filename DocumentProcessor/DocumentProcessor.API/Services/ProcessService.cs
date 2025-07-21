using MassTransit;
using DocumentProcessor.Contracts.Events;
using DocumentProcessor.Contracts.Dtos;
using DocumentProcessor.Dao.Interfaces;
using DocumentProcessor.API.Models;
using DocumentProcessor.Contracts;

namespace DocumentProcessor.API.Services
{

    public class ProcessService : IProcessService
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IApiProcessRepository _processRepository;
        private readonly ILogger<ProcessService> _logger;

        public ProcessService(
            IPublishEndpoint publishEndpoint,
            IApiProcessRepository processRepository,
            ILogger<ProcessService> logger)
        {
            _publishEndpoint = publishEndpoint;
            _processRepository = processRepository;
            _logger = logger;
        }

        public async Task<ProcessStartResult> StartProcessAsync(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                throw new ArgumentException("At least one file is required");

            if (files.Count > 50)
                throw new ArgumentException("Maximum 50 files allowed per process");

            var allowedExtensions = new[] { ".txt", ".md", ".csv" };
            foreach (var file in files)
            {
                if (file.Length == 0)
                    throw new ArgumentException($"File {file.FileName} is empty");

                if (file.Length > 10 * 1024 * 1024) 
                    throw new ArgumentException($"File {file.FileName} exceeds maximum size of 10MB");

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                    throw new ArgumentException($"File {file.FileName} has unsupported extension. Allowed: {string.Join(", ", allowedExtensions)}");

                if (string.IsNullOrWhiteSpace(file.FileName))
                    throw new ArgumentException("File name cannot be empty");
            }

            var totalSize = files.Sum(f => f.Length);
            if (totalSize > 100 * 1024 * 1024) 
                throw new ArgumentException("Total file size exceeds limit of 100MB");

            var processId = Guid.NewGuid();
            var processFolder = Path.Combine("/app/documents", processId.ToString());
            Directory.CreateDirectory(processFolder);

            var fileNames = new List<string>();
            foreach (var file in files)
            {
                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(processFolder, uniqueFileName);
                
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
                
                fileNames.Add(uniqueFileName);
            }

            var processDto = new ProcessDTO
            {
                ProcessId = processId,
                Status = "PENDING",
                TotalFiles = files.Count,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                FolderPath = processId.ToString() // Guardar solo el processId como folder path
            };

            await _processRepository.CreateProcessAsync(processDto);

            await _publishEndpoint.Publish(new ProcessStartedEvent
            {
                ProcessId = processId,
                FileNames = fileNames
            });

            _logger.LogInformation("Process {ProcessId} started successfully with {FileCount} files", processId, files.Count);

            return new ProcessStartResult
            {
                ProcessId = processId,
                Message = "Process started successfully"
            };
        }

        public async Task<bool> StopProcessAsync(Guid processId)
        {
            var process = await _processRepository.GetProcessByIdAsync(processId);
            if (process == null)
                throw new KeyNotFoundException("Process not found");

            if (process.Status == ProcessStatus.Completed || process.Status == ProcessStatus.Failed)
                throw new InvalidOperationException("Process is already finished");

            process.Status = ProcessStatus.Stopped;
            process.UpdatedAt = DateTime.UtcNow;
            await _processRepository.UpdateProcessAsync(process);

            _logger.LogInformation("Process {ProcessId} stopped successfully", processId);

            return true;
        }

        public async Task<ProcessStatusResponse> GetProcessStatusAsync(Guid processId)
        {
            var process = await _processRepository.GetProcessByIdAsync(processId);
            if (process == null)
                throw new KeyNotFoundException("Process not found");

            var processedFiles = process.Files?.Count(f => f.Status == "COMPLETED" || f.Status == "FAILED") ?? 0;
            var percentage = process.TotalFiles > 0 ? (processedFiles * 100) / process.TotalFiles : 0;

            DateTime? estimatedCompletion = null;

            if ((process.Status == ProcessStatus.Running || process.Status == ProcessStatus.Completed)
                && processedFiles > 1 && process.StartedAt.HasValue)
            {
                var files = process.Files?.Where(f => f.Status == "COMPLETED" || f.Status == "FAILED")
                    .OrderBy(f => f.UpdatedAt)
                    .ToList() ?? new List<FileDTO>();

                if (files.Count > 1)
                {
                    var elapsedTime = files.Last().UpdatedAt - process.StartedAt.Value;
                    var avg = elapsedTime.TotalMilliseconds / files.Count;
                    if (avg > 0)
                    {
                        var remainingFiles = process.TotalFiles - processedFiles;
                        var estimatedRemainingTime = TimeSpan.FromMilliseconds(avg * remainingFiles);
                        var ect = DateTime.UtcNow.Add(estimatedRemainingTime);

                        // No mostrar estimado si es mayor a 24 horas (podr�a ser inv�lido)
                        estimatedCompletion = ect > DateTime.UtcNow.AddHours(24) ? null : ect;
                    }
                }
            }
            else if (process.Status == ProcessStatus.Completed && process.CompletedAt.HasValue)
            {
                estimatedCompletion = process.CompletedAt.Value;
            }

            _logger.LogInformation("Status: {Status}, ProcessedFiles: {ProcessedFiles}, StartedAt: {StartedAt}, ECT: {ECT}",
                process.Status, processedFiles, process.StartedAt, estimatedCompletion);

            return new ProcessStatusResponse
            {
                ProcessId = process.ProcessId,
                Status = process.Status,
                StartedAt = process.StartedAt ?? DateTime.UtcNow,
                EstimatedCompletion = estimatedCompletion,
                Progress = new ProgressInfo
                {
                    TotalFiles = process.TotalFiles,
                    ProcessedFiles = processedFiles,
                    Percentage = percentage
                }
            };
        }

        public async Task<List<ProcessSummary>> ListProcessesAsync()
        {
            var processes = await _processRepository.GetAllProcessesAsync();

            return processes
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new ProcessSummary
                {
                    ProcessId = p.ProcessId,
                    Status = p.Status,
                    TotalFiles = p.TotalFiles,
                    CompletedFiles = p.CompletedFiles,
                    FailedFiles = p.FailedFiles,
                    StartedAt = p.StartedAt ?? DateTime.UtcNow,
                    CreatedAt = p.CreatedAt
                })
                .ToList();
        }

        public async Task<ProcessResultsResponse> GetProcessResultsAsync(Guid processId)
        {
            var process = await _processRepository.GetProcessByIdAsync(processId);
            if (process == null)
                throw new KeyNotFoundException("Process not found");

            if (process.Status != ProcessStatus.Completed)
                throw new InvalidOperationException("Process is not completed yet");

            var completedFiles = process.Files?.Where(f => f.Status == "COMPLETED").ToList() ?? new List<FileDTO>();

            var totalWords = completedFiles.Sum(f => f.WordCount);
            var totalLines = completedFiles.Sum(f => f.LineCount);

            // TODO: Idealmente estos c�lculos deber�an hacerse en el worker o cachearse en DB para evitar costo runtime.
            var allFrequentWords = completedFiles
                .SelectMany(f =>
                {
                    if (string.IsNullOrEmpty(f.MostFrequentWords))
                        return new List<string>();

                    return f.MostFrequentWords.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(w => w.Trim())
                        .Where(w => !string.IsNullOrEmpty(w))
                        .ToList();
                })
                .GroupBy(w => w)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Key)
                .ToList();

            return new ProcessResultsResponse
            {
                ProcessId = process.ProcessId,
                Status = process.Status,
                Results = new ProcessResults
                {
                    TotalWords = totalWords,
                    TotalLines = totalLines,
                    MostFrequentWords = allFrequentWords,
                    FilesProcessed = completedFiles.Select(f => f.FileName).ToList()
                }
            };
        }
    }
}
