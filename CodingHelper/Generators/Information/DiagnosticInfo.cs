using Microsoft.CodeAnalysis;

namespace Muggle.TsExtensions.CodingHelper.Generators.Information {
    public readonly record struct DiagnosticInfo {
        public bool Equals(DiagnosticInfo other) {
            return Equals(Descriptor, other.Descriptor) && Equals(Location, other.Location);
        }

        public override int GetHashCode() {
            unchecked {
                return ((Descriptor is not null ? Descriptor.GetHashCode() : 0) * 397) ^
                       (Location is not null ? Location.GetHashCode() : 0);
            }
        }

        public readonly DiagnosticDescriptor Descriptor;
        public readonly Location Location;
        public readonly object[] Arguments;

        public DiagnosticInfo(DiagnosticDescriptor descriptor, Location location, object[] arguments) {
            Descriptor = descriptor;
            Location = location;
            Arguments = arguments;
        }
    }
}