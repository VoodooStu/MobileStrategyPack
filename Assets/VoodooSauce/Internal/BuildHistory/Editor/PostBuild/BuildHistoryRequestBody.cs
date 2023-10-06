using System;

namespace Voodoo.Sauce.Internal.Editor
{
    // ReSharper disable InconsistentNaming, UnusedAutoPropertyAccessor.Global
    [Serializable]
    public class BuildHistoryRequestBody
    {
        public string vsc_repo_url;
        public string vsc_branch_name;
        public string vsc_user_name;
        public string vsc_commit_id ;
        public string vsc_commit_date ;
        public string unity_version ;
        public string ide_version ;
        public string target_platform ;
        public string build_machine_os ;
        public string target_api ;
        public bool development_mode_active ;
        public bool icon_updated ;
        public bool vs_updated ;
        public bool is_batch_mode ;
        public string build_error_message ;
        public string package_json ;
        public string vs_version ;
        public string app_version ;
        public string bundle_id ;
        public string icon_last_updated_time ;
        public string vs_last_updated_time ;
        public string app_build_number ;
    }
}