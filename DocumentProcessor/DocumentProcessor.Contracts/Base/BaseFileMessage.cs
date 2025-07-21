namespace DocumentProcessor.Contracts.Base
{
    public  record BaseFileMessage : BaseProcessMessage
    {
        public Guid FileId { get; init; }
        public string FileName { get; init; } = string.Empty;
    }
}
