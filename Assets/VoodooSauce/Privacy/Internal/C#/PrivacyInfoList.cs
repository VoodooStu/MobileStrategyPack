using System.Collections.Generic;
using System.Linq;

namespace Voodoo.Sauce.Privacy
{
    internal class PrivacyInfoList : List<PrivacyInfo>
    {
        internal string[] GetNames()
        {
            return this.Select(info => info.Name).ToArray();
        }

        internal IEnumerable<string> GetPrivacyPolicyUrls()
        {
            return this.Select(info => info.PrivacyPolicyUrl).Distinct();
        }
    }
    
    internal class PrivacyInfo
    {
        public string Name;
        public string PrivacyPolicyUrl;
    }
}