namespace lbfox.ViewModels
{
    public class VinReportResponse
    {
        public bool Success { get; set; }
        public string ReportFile { get; set; }
        public int? RemainingPoints { get; set; }
        public string ErrorMessage { get; internal set; }
    }
}