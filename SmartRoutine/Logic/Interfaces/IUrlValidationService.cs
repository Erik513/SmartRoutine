using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutine.Logic.Interfaces
{
    public interface IUrlValidationService
    {
        /// <summary>
        /// Validiert und repariert eine URL wenn möglich
        /// </summary>
        /// <param name="url">Die zu validierende URL</param>
        /// <param name="allowEmpty">Ob eine leere URL erlaubt ist</param>
        /// <returns>Tuple mit (IstGültig, ReparierteUrl, Fehlermeldung)</returns>
        (bool IsValid, string RepairedUrl, string ErrorMessage) ValidateAndRepairUrl(string url, bool allowEmpty = false);

        /// <summary>
        /// Prüft ob eine URL gültig ist
        /// </summary>
        bool IsValidUrl(string url);

        /// <summary>
        /// Versucht eine URL zu reparieren
        /// </summary>
        string TryRepairUrl(string input);
    }
}