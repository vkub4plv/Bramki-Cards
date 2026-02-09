using System.ComponentModel.DataAnnotations.Schema;
using System.Buffers.Binary;
using System.Globalization;

namespace Bramki.Models
{
    public class Person
    {
        public int ID { get; set; }
        public string Name { get; set; } = "";
        public string? CardNumber { get; set; }
        public string? CardNumber2 { get; set; }
        public string? ERPID { get; set; }
        public bool IsActive { get; set; }

        public ICollection<GatePrivilege> GatePrivileges { get; set; } = new List<GatePrivilege>();

        [NotMapped] public bool? GatesForklifts { get; set; }
        [NotMapped] public bool? GatesCranes { get; set; }
        [NotMapped] public bool? GatesGantries { get; set; }

        [NotMapped] public string GatesForkliftsText => GatesForklifts == true ? "Tak" : "Nie";
        [NotMapped] public string GatesCranesText => GatesCranes == true ? "Tak" : "Nie";
        [NotMapped] public string GatesGantriesText => GatesGantries == true ? "Tak" : "Nie";

        // Lunch card number (DEC, D10) derived from CardNumber
        [NotMapped] public string? LunchCardNumber => ToLunchCard(CardNumber);

        // PPE storage cabinets card (HEX, X8) derived from CardNumber
        [NotMapped] public string? PpeStorageCabinetsCardNumber => ToPpeStorageCabinetsCard(CardNumber);

        public string FirstName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name)) return string.Empty;
                var parts = Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return parts.Length <= 1 ? parts[0] : string.Join(' ', parts[..^1]);
            }
        }

        public string Surname
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name)) return string.Empty;
                var parts = Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return parts.Length == 1 ? string.Empty : parts[^1];
            }
        }

        private static string? ToLunchCard(string? dec)
        {
            if (string.IsNullOrWhiteSpace(dec)) return null;

            // Accept big values; use the low 32 bits for the lunch mapping
            if (!ulong.TryParse(dec, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                return null;

            uint low32 = (uint)(v & 0xFFFFFFFFUL);
            uint reversed = BinaryPrimitives.ReverseEndianness(low32);
            return reversed.ToString("D10", CultureInfo.InvariantCulture);
        }

        private static string? ToPpeStorageCabinetsCard(string? dec)
        {
            if (string.IsNullOrWhiteSpace(dec)) return null;

            // Accept big values; use the low 32 bits for the PpeStorageCabinets mapping
            if (!ulong.TryParse(dec, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                return null;

            uint low32 = (uint)(v & 0xFFFFFFFFUL);
            uint reversed = BinaryPrimitives.ReverseEndianness(low32);
            return reversed.ToString("X8", CultureInfo.InvariantCulture); // HEX
        }
    }
}