namespace SmartRoutine.Logic.Interfaces
{
    public interface IUrlValidationService
    {
        /// <summary>
        /// Validates a URL and repairs it if possible.
        /// </summary>
        /// <param name="url">The URL to validate.</param>
        /// <param name="allowEmpty">Whether an empty URL is allowed.</param>
        /// <returns>A tuple (IsValid, RepairedUrl, ErrorMessage); the message is in the language of the program.</returns>
        (bool IsValid, string RepairedUrl, string ErrorMessage) ValidateAndRepairUrl(string url, bool allowEmpty = false);

        /// <summary>
        /// Checks whether a URL is valid.
        /// </summary>
        bool IsValidUrl(string url);

        /// <summary>
        /// Tries to repair a URL.
        /// </summary>
        string TryRepairUrl(string input);
    }
}
