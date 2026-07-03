using System;
using System.Collections.Immutable;

namespace Muggle.TsExtensions.CodingHelper.Generators.Information {
    public record struct GatheredInfo<TValue> where TValue : IEquatable<TValue> {

        public TValue Value;
        public ImmutableArray<DiagnosticInfo> DiagnosticInfos;

        public GatheredInfo(TValue value, ImmutableArray<DiagnosticInfo> diagnosticInfos) {
            Value = value;
            DiagnosticInfos = diagnosticInfos;
        }
    }
}