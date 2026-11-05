using System;

namespace SmartRoutine.Logic.Services
{
    /// <summary>
    /// Decides which addresses a web page in the internal browser may hand to Windows (a link that opens
    /// a new window): only web and mail links. Anything else (file:, custom protocols of programs ...)
    /// could start a program from a page and is not opened.
    /// </summary>
    public static class ExternalLinkPolicy
    {
        public static bool IsAllowed(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return false;

            Uri parsed;

            if (!Uri.TryCreate(uri.Trim(), UriKind.Absolute, out parsed))
                return false;

            return parsed.Scheme == Uri.UriSchemeHttp
                || parsed.Scheme == Uri.UriSchemeHttps
                || parsed.Scheme == Uri.UriSchemeMailto;
        }
    }
}
