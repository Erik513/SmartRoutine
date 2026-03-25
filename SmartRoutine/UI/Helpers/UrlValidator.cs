using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SmartRoutine.UI.Helpers
{
    public static class UrlValidator
    {
        // Unterstützte Protokolle (allgemein)
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
        // Versucht eine URL zu reparieren/zu vervollständigen
        public static string TryRepairUrl(string input)
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
                    // Füge https:// voran (Standard für Web)
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
                // Korrigiere häufige Tippfehler
                trimmed = CorrectProtocolTypos(trimmed);
            }

            // 6. Füge fehlendes www. hinzu wenn nötig
            if (ShouldAddWww(trimmed))
            {
                trimmed = AddWwwPrefix(trimmed);
            }

            return trimmed;
        }
        // Validiert ob eine URL gültig ist
        public static bool IsValidUrl(string url)
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
                    // Überprüfe das Schema
                    foreach (string scheme in SupportedSchemes)
                    {
                        if (uri.Scheme.Equals(scheme, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }

                    // Auch andere absolute URIs sind okay
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        // Validiert und repariert URL wenn nötig
        public static (bool IsValid, string RepairedUrl, string ErrorMessage) ValidateAndRepairUrl(string url, bool allowEmpty = false)
        {
            // DEBUG: Zeige den genauen Inhalt der URL
            Console.WriteLine($"URLValidator received: '{url}'");
            Console.WriteLine($"URL length: {url?.Length ?? 0}");
            if (url != null)
            {
                Console.WriteLine($"URL char codes: {string.Join(", ", url.Select(c => (int)c))}");
            }
            // Leere URLs sind optional erlaubt
            if (string.IsNullOrWhiteSpace(url))
            {
                if (allowEmpty)
                    return (true, url, null);  // Erlaubt - KEINE Fehlermeldung!
                else
                    return (false, url, "URL cannot be empty.");  // Nur hier Fehlermeldung
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
                // Erfolgreich repariert - keine Meldung nötig
                return (true, repairedUrl, null);
            }

            // 4. URL ist ungültig und konnte nicht repariert werden
            string error = BuildErrorMessage(trimmed);
            return (false, url, error);
        }

        // Erstellt eine hilfreiche Fehlermeldung
        private static string BuildErrorMessage(string url)
        {
            // Diese Methode wird NUR für nicht-leere, ungültige URLs aufgerufen
            if (url.Contains(" "))
                return "URLs should not contain spaces. Please remove spaces or encode them with %20.";

            if (url.Contains("@") && !url.Contains("mailto:"))
                return "This looks like an email address. Use 'mailto:email@example.com' format.";

            if (url.Contains("\\") && !url.StartsWith("file://"))
                return "This looks like a Windows file path. Use 'file:///C:/path/to/file' format.";

            return $"The entered text is not a valid URL.\n\nEntered: {url}\n\n" +
                   "Valid URL examples:\n" +
                   "• https://example.com\n" +
                   "• http://192.168.1.1\n" +
                   "• mailto:user@example.com\n" +
                   "• file:///C:/Users/Name/file.txt";
        }

        // Hilfsmethoden
        private static bool HasProtocol(string url)
        {
            return url.Contains("://") || url.StartsWith("mailto:");
        }

        private static bool LooksLikeDomain(string text)
        {
            // Muss einen Punkt haben (Domain.TLD)
            if (!text.Contains('.'))
                return false;

            // Sollte keine Leerzeichen enthalten
            if (text.Contains(' '))
                return false;

            // Extrahiere den Domain-Teil (alles vor erstem / oder ?)
            string domainPart = text.Split('/', '?')[0];

            // Prüfe ob der letzte Teil nach dem Punkt eine typische TLD-Länge hat
            string[] parts = domainPart.Split('.');
            if (parts.Length >= 2)
            {
                string lastPart = parts.Last().ToLower();

                // TLDs sind meist 2-6 Zeichen lang
                if (lastPart.Length >= 2 && lastPart.Length <= 6)
                {
                    // Prüfe ob es nur Buchstaben sind (keine Zahlen oder Sonderzeichen)
                    if (lastPart.All(c => char.IsLetter(c)))
                    {
                        return true;
                    }
                }
            }

            // Prüfe auf bekannte TLDs (auch im gesamten Text)
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
            // Einfache IPv4-Erkennung
            string pattern = @"^\d{1,3}(\.\d{1,3}){3}(:\d+)?$";
            return Regex.IsMatch(text, pattern);
        }

        private static bool LooksLikeLocalPath(string text)
        {
            // Windows-Pfad (C:\ oder \\Server\)
            if (text.Contains(":\\") || text.StartsWith("\\\\"))
                return true;

            // Unix-Pfad mit / und kein :// (also kein Protokoll)
            if (text.StartsWith("/") && !text.Contains("://"))
                return true;

            return false;
        }

        private static bool HasMalformedProtocol(string url)
        {
            // Häufige Tippfehler bei Protokollen
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

        private static bool ShouldAddWww(string url)
        {
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return false;

            // Extrahiere den Teil nach dem Protokoll
            string withoutProtocol = url.Contains("://")
                ? url.Substring(url.IndexOf("://") + 3)
                : url;

            // Wenn keine Subdomain vorhanden ist und es eine Domain ist
            return !withoutProtocol.StartsWith("www.", StringComparison.OrdinalIgnoreCase) &&
                   !withoutProtocol.Contains("/") &&
                   withoutProtocol.Contains('.') &&
                   !withoutProtocol.Any(c => char.IsDigit(c)) &&
                   !withoutProtocol.Contains("localhost");
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
