using Microsoft.CodeAnalysis;

namespace Muggle.TsExtensions.CodingHelper.Generators.Information {
    internal record struct ClassInfo {
        public string Name;

        public string NameSpace;

        public Accessibility Accessibility;

        public bool IsRecord;
    }
}
