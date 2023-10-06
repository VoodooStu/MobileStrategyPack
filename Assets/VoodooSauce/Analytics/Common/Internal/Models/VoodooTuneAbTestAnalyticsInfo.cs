using System;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    public class VoodooTuneAbTestAnalyticsInfo
    {
        public readonly string AbTestUuid;
        public readonly string AbTestCohortUuid;
        public readonly string AbTestVersionUuid;
        
        public VoodooTuneAbTestAnalyticsInfo(string abTestUuid, string abTestCohortUuid, string abTestVersionUuid)
        {
            AbTestUuid = abTestUuid;
            AbTestCohortUuid = abTestCohortUuid;
            AbTestVersionUuid = abTestVersionUuid;
        }
        
        public VoodooTuneAbTestAnalyticsInfo(string abTestUuid, string abTestCohortUuid)
        {
            AbTestUuid = abTestUuid;
            AbTestCohortUuid = abTestCohortUuid;
            AbTestVersionUuid = "";
        }

        public override bool Equals(object obj) => obj is VoodooTuneAbTestAnalyticsInfo info && IsSameValue(AbTestUuid, info.AbTestUuid) && IsSameValue(AbTestCohortUuid, info.AbTestCohortUuid);

        public override int GetHashCode() => Convert.ToInt32((AbTestUuid + "_" + AbTestCohortUuid).GetHashCode());

        private static bool IsSameValue(string oldValue, string newValue)
        {
            string value1 = string.IsNullOrEmpty(oldValue) ? null : oldValue;
            string value2 = string.IsNullOrEmpty(newValue) ? null : newValue;
            return value1 == value2;
        }

        public override string ToString() => $"{AbTestUuid} - {AbTestCohortUuid} - {AbTestVersionUuid}";
    }
}