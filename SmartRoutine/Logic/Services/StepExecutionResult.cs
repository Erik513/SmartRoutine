

namespace SmartRoutine.Logic.Services
{
    public class StepExecutionResult
    {
        public bool ShouldOpenInInternalBrowser { get; set; }
        public string InternalBrowserUrl { get; set; }

        public static StepExecutionResult None()
        {
            return new StepExecutionResult();
        }

        public static StepExecutionResult OpenInternalUrl(string url)
        {
            return new StepExecutionResult
            {
                ShouldOpenInInternalBrowser = true,
                InternalBrowserUrl = url
            };
        }
    }
}
