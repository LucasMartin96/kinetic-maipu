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

        public async Task<ProcessStartResult> StartProcessAsync(ProcessStartRequest request)
        {
            // Validar que la carpeta existe
            if (string.IsNullOrEmpty(request.FolderPath) || !Directory.Exists(request.FolderPath))
                throw new ArgumentException("Invalid folder path");

            // Buscar archivos .txt en la carpeta
            var textFiles = Directory.GetFiles(request.FolderPath, "*.txt");
            if (textFiles.Length == 0)
                throw new ArgumentException("No text files found in the specified folder");

            // Crear DTO de proceso con estado inicial PENDING
            var processDto = new ProcessDTO
            {
                ProcessId = Guid.NewGuid(),
                Status = "PENDING",
                TotalFiles = textFiles.Length,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                FolderPath = Path.GetRelativePath("/app/documents", request.FolderPath).Replace("\\", "/")
            };

            // Guardar proceso en base de datos
            var processId = await _processRepository.CreateProcessAsync(processDto);

            // Normalizar paths de archivos para publicar
            var fileNames = textFiles.Select(f => Path.GetRelativePath("/app/documents", f).Replace("\\", "/")).ToList();

            // Publicar evento para iniciar procesamiento (Saga)
            await _publishEndpoint.Publish(new ProcessStartedEvent
            {
                ProcessId = processId,
                FileNames = fileNames
            });

            _logger.LogInformation("Process {ProcessId} started successfully with {FileCount} files", processId, textFiles.Length);

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

            // Contar archivos procesados (COMPLETED o FAILED)
            var processedFiles = process.Files?.Count(f => f.Status == "COMPLETED" || f.Status == "FAILED") ?? 0;
            var percentage = process.TotalFiles > 0 ? (processedFiles * 100) / process.TotalFiles : 0;

            DateTime? estimatedCompletion = null;

            // Calcular estimación solo si proceso está corriendo o finalizado y hay datos para calcular promedio
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

                        // No mostrar estimado si es mayor a 24 horas (podría ser inválido)
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

            // TODO: Idealmente estos cálculos deberían hacerse en el worker o cachearse en DB para evitar costo runtime.
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
