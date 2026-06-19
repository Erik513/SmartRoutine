using SmartRoutine.Logic.Interfaces;
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
                    return (false, url, "Die URL darf nicht leer sein.");
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

            // Schnelle Präfix-Checks
            foreach (string scheme in SupportedSchemes)
            {
                if (trimmed.StartsWith($"{scheme}://", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            // Auch mailto: ohne // ist gültig
            if (trimmed.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
                return true;

            // URI-Parsing für genaue Validierung
            try
            {
                if (Uri.TryCreate(trimmed, UriKind.Absolute, out Uri uri))
                {
                    foreach (string scheme in SupportedSchemes)
                    {
                        if (uri.Scheme.Equals(scheme, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                    return SupportedSchemes.Any(s => uri.Scheme.Equals(s, StringComparison.OrdinalIgnoreCase));
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        public string TryRepairUrl(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            string trimmed = input.Trim();

            // 1. Entferne unerwünschte Zeichen am Anfang/Ende
            trimmed = trimmed.Trim('\'', '"', '<', '>', ' ', '\t', '\n', '\r');

            // 2. Wenn es bereits eine gültige URL ist, zurückgeben
            if (IsValidUrl(trimmed))
                return trimmed;

            // 3. Prüfe ob es eine E-Mail-Adresse ist
            if (IsValidEmail(trimmed))
                return $"mailto:{trimmed}";

            // 4. Prüfe ob ein Protokoll fehlt
            if (!HasProtocol(trimmed))
            {
                // 4a. Prüfe ob es wie eine Domain aussieht
                if (LooksLikeDomain(trimmed))
                {
                    return $"https://{trimmed}";
                }

                // 4b. Prüfe ob es eine IP-Adresse ist
                if (LooksLikeIpAddress(trimmed))
                {
                    return $"http://{trimmed}";
                }

                // 4c. Prüfe ob es ein lokaler Pfad ist
                if (LooksLikeLocalPath(trimmed))
                {
                    return $"file:///{trimmed.Replace("\\", "/")}";
                }
            }

            // 5. Prüfe ob Protokoll korrigiert werden muss
            if (HasMalformedProtocol(trimmed))
            {
                trimmed = CorrectProtocolTypos(trimmed);
            }

            return trimmed;
        }

        // ========== HILFSMETHODEN ==========

        private static string BuildErrorMessage(string url)
        {
            if (url.Contains(" "))
                return "URLs sollten keine Leerzeichen enthalten.";

            if (url.Contains("@") && !url.Contains("mailto:"))
                return "Das sieht nach einer E-Mail-Adresse aus. Verwende das Format „mailto:email@example.com“.";

            if (url.Contains("\\") && !url.StartsWith("file://"))
                return "Das sieht nach einem Windows-Dateipfad aus. Verwende das Format „file:///C:/Pfad/zur/Datei“.";

            return $"Der eingegebene Text ist keine gültige URL.\n\nEingetragen: {url}\n\n" +
                   "Beispiele für gültige URLs:\n" +
                   "• https://example.com\n" +
                   "• http://192.168.1.1\n" +
                   "• mailto:user@example.com\n" +
                   "• file:///C:/Users/Name/file.txt";
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

        private static bool HasMalformedProtocol(string url)
        {
            string[] malformedProtocols =
            {
                "htttp://", "htps://", "http//", "https//",
                "ttp://", "tps://", "htp://", "http:"
            };

            foreach (string malformed in malformedProtocols)
            {
                if (url.StartsWith(malformed, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static string CorrectProtocolTypos(string url)
        {
            string lowerUrl = url.ToLower();

            if (lowerUrl.StartsWith("htttp://"))
                return "http://" + url.Substring("htttp://".Length);
            if (lowerUrl.StartsWith("htps://"))
                return "https://" + url.Substring("htps://".Length);
            if (lowerUrl.StartsWith("http//"))
                return "http://" + url.Substring("http//".Length);
            if (lowerUrl.StartsWith("https//"))
                return "https://" + url.Substring("https//".Length);
            if (lowerUrl.StartsWith("ttp://"))
                return "http://" + url.Substring("ttp://".Length);
            if (lowerUrl.StartsWith("tps://"))
                return "https://" + url.Substring("tps://".Length);
            if (lowerUrl.StartsWith("htp://"))
                return "http://" + url.Substring("htp://".Length);
            if (lowerUrl.StartsWith("http:"))
                return "http://" + url.Substring("http:".Length);

            return url;
        }

        private static string AddWwwPrefix(string url)
        {
            int protocolEnd = url.IndexOf("://") + 3;
            return url.Insert(protocolEnd, "www.");
        }

        private static bool IsValidEmail(string text)
        {
            try
            {
                string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(text, pattern);
            }
            catch
            {
                return false;
            }
        }
    }
}