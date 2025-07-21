using MassTransit;
using DocumentProcessor.Contracts.Events;
using DocumentProcessor.Contracts.Dtos;
using DocumentProcessor.Dao.Interfaces;
using DocumentProcessor.API.Models;

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
            if (string.IsNullOrEmpty(request.FolderPath) || !Directory.Exists(request.FolderPath))
            {
                throw new ArgumentException("Invalid folder path");
            }

            var textFiles = Directory.GetFiles(request.FolderPath, "*.txt");
            if (textFiles.Length == 0)
            {
                throw new ArgumentException("No text files found in the specified folder");
            }

            var processDto = new ProcessDTO
            {
                ProcessId = Guid.NewGuid(),
                Status = "PENDING",
                TotalFiles = textFiles.Length,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                FolderPath = Path.GetRelativePath("/app/documents", request.FolderPath).Replace("\\", "/")
            };

            var processId = await _processRepository.CreateProcessAsync(processDto);

            var root = request.FolderPath;
            var fileNames = textFiles.Select(f => Path.GetRelativePath("/app/documents", f).Replace("\\", "/")).ToList();

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
            {
                throw new KeyNotFoundException("Process not found");
            }

            if (process.Status == "COMPLETED" || process.Status == "FAILED")
            {
                throw new InvalidOperationException("Process is already finished");
            }

            process.Status = "STOPPED";
            process.UpdatedAt = DateTime.UtcNow;
            await _processRepository.UpdateProcessAsync(process);

            _logger.LogInformation("Process {ProcessId} stopped successfully", processId);

            return true;
        }

        public async Task<ProcessStatusResponse> GetProcessStatusAsync(Guid processId)
        {
            var process = await _processRepository.GetProcessByIdAsync(processId);
            if (process == null)
            {
                throw new KeyNotFoundException("Process not found");
            }

            var processedFiles = process.CompletedFiles + process.FailedFiles;
            var percentage = process.TotalFiles > 0 ? (processedFiles * 100) / process.TotalFiles : 0;

            DateTime? estimatedCompletion = null;
            if (process.Status == "RUNNING" && processedFiles > 0 && process.StartedAt.HasValue)
            {
                var elapsedTime = DateTime.UtcNow - process.StartedAt.Value;
                var averageTimePerFile = elapsedTime.TotalMilliseconds / processedFiles;
                var remainingFiles = process.TotalFiles - processedFiles;
                var estimatedRemainingTime = TimeSpan.FromMilliseconds(averageTimePerFile * remainingFiles);
                estimatedCompletion = DateTime.UtcNow.Add(estimatedRemainingTime);
            }
            else if (process.Status == "COMPLETED" && process.CompletedAt.HasValue)
            {
                estimatedCompletion = process.CompletedAt.Value;
            }

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
            {
                throw new KeyNotFoundException("Process not found");
            }

            if (process.Status != "COMPLETED")
            {
                throw new InvalidOperationException("Process is not completed yet");
            }

            var completedFiles = process.Files?.Where(f => f.Status == "COMPLETED").ToList() ?? new List<FileDTO>();
            var totalWords = completedFiles.Sum(f => f.WordCount);
            var totalLines = completedFiles.Sum(f => f.LineCount);

            // TODO: este procesamiento deberia estar en la base ya... o la primera vez que se hace aqui deberia ir a la base... catchear.. o que el worker lo calcule al finalizar! tmabien podria ser...
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