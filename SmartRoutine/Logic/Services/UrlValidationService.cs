using SmartRoutine.Logic.Interfaces;
using SmartRoutine.Localization;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SmartRoutine.Logic.Services
{
    public class UrlValidationService : IUrlValidationService
    {
        // Unterstützte Protokolle
        private static readonly string[] SupportedSchemes =
        {
            "http",
            "https",
            "ftp",
            "ftps",
            "mailto",
            "file",
            "data"
        };

        // Bekannte Domain-Erweiterungen für automatische Erkennung
        private static readonly string[] CommonTlds =
        {
            ".com", ".org", ".net", ".de", ".uk", ".fr", ".it", ".es", ".nl",
            ".edu", ".gov", ".mil", ".io", ".co", ".tv", ".live", ".stream",
            ".info", ".biz", ".mobi", ".name", ".pro", ".museum", ".aero", ".to"
        };

        public (bool IsValid, string RepairedUrl, string ErrorMessage) ValidateAndRepairUrl(string url, bool allowEmpty = false)
        {
            // Leere URLs sind optional erlaubt
            if (string.IsNullOrWhiteSpace(url))
            {
                if (allowEmpty)
                    return (true, url, null);
                else
                    return (false, url, Loc.S("Url.Error.Empty"));
            }

            string trimmed = url.Trim();

            // 1. Direkte Validierung
            if (IsValidUrl(trimmed))
                return (true, trimmed, null);

            // 2. Versuche zu reparieren
            string repairedUrl = TryRepairUrl(trimmed);

            // 3. Validiere die reparierte URL
            if (IsValidUrl(repairedUrl))
            {
                return (true, repairedUrl, null);
            }

            // 4. URL ist ungültig und konnte nicht repariert werden
            string error = BuildErrorMessage(trimmed);
            return (false, url, error);
        }

        public bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            string trimmed = url.Trim();

            // Quick prefix checks ("scheme://"; mailto: has no //)
            foreach (string scheme in SupportedSchemes)
            {
                if (trimmed.StartsWith($"{scheme}://", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            if (trimmed.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
                return true;

            // Exact check by parsing
            return Uri.TryCreate(trimmed, UriKind.Absolute, out Uri uri)
                && SupportedSchemes.Any(s => uri.Scheme.Equals(s, StringComparison.OrdinalIgnoreCase));
        }

        public string TryRepairUrl(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // 1. Remove unwanted characters at the start/end
            string trimmed = input.Trim('\'', '"', '<', '>', ' ', '\t', '\n', '\r');

            // 2. Already a valid URL
            if (IsValidUrl(trimmed))
                return trimmed;

            // 3. An e-mail address
            if (IsValidEmail(trimmed))
                return $"mailto:{trimmed}";

            // 4. A typo in the protocol ("htps://", "http//" ...) - before the domain check, which
            //    would take "http//example.com" for a domain and put https:// in front of it
            if (HasMalformedProtocol(trimmed))
                return CorrectProtocolTypos(trimmed);

            // 5. The protocol is missing
            if (!HasProtocol(trimmed))
            {
                if (LooksLikeDomain(trimmed))
                    return $"https://{trimmed}";

                if (LooksLikeIpAddress(trimmed))
                    return $"http://{trimmed}";

                if (LooksLikeLocalPath(trimmed))
                    return $"file:///{trimmed.Replace("\\", "/")}";
            }

            return trimmed;
        }

        // ========== HILFSMETHODEN ==========

        private static string BuildErrorMessage(string url)
        {
            if (url.Contains(" "))
                return Loc.S("Url.Error.Spaces");

            if (url.Contains("@") && !url.Contains("mailto:"))
                return Loc.S("Url.Error.Email");

            if (url.Contains("\\") && !url.StartsWith("file://"))
                return Loc.S("Url.Error.WindowsPath");

            return Loc.S("Url.Error.Other", url);
        }

        private static bool HasProtocol(string url)
        {
            return url.Contains("://") || url.StartsWith("mailto:");
        }

        private static bool LooksLikeDomain(string text)
        {
            if (!text.Contains('.'))
                return false;

            if (text.Contains(' '))
                return false;

            string domainPart = text.Split('/', '?')[0];
            string[] parts = domainPart.Split('.');

            if (parts.Length >= 2)
            {
                string lastPart = parts.Last().ToLower();
                if (lastPart.Length >= 2 && lastPart.Length <= 6)
                {
                    if (lastPart.All(c => char.IsLetter(c)))
                    {
                        return true;
                    }
                }
            }

            foreach (string tld in CommonTlds)
            {
                if (text.ToLowerInvariant().Contains(tld.ToLowerInvariant()))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool LooksLikeIpAddress(string text)
        {
            string pattern = @"^\d{1,3}(\.\d{1,3}){3}(:\d+)?$";
            return Regex.IsMatch(text, pattern);
        }

        private static bool LooksLikeLocalPath(string text)
        {
            if (text.Contains(":\\") || text.StartsWith("\\\\"))
                return true;

            if (text.StartsWith("/") && !text.Contains("://"))
                return true;

            return false;
        }

        // Typo -> correct start. The order matters: the first typo that matches wins.
        private static readonly (string Typo, string Fixed)[] ProtocolTypos =
        {
            ("htttp://", "http://"),
            ("htps://", "https://"),
            ("http//", "http://"),
            ("https//", "https://"),
            ("ttp://", "http://"),
            ("tps://", "https://"),
            ("htp://", "http://"),
            ("http:", "http://")
        };

        private static bool HasMalformedProtocol(string url)
        {
            return ProtocolTypos.Any(t => url.StartsWith(t.Typo, StringComparison.OrdinalIgnoreCase));
        }

        private static string CorrectProtocolTypos(string url)
        {
            foreach (var (typo, fixedStart) in ProtocolTypos)
            {
                if (url.StartsWith(typo, StringComparison.OrdinalIgnoreCase))
                    return fixedStart + url.Substring(typo.Length);
            }

            return url;
        }

        private static bool IsValidEmail(string text)
        {
            return Regex.IsMatch(text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }
}
