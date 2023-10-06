using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Voodoo.Sauce.Common.Utils;

// ReSharper disable CheckNamespace
namespace Voodoo.Sauce.Internal.Editor
{
    public class VsBuildHistoryPostBuild: IPostprocessBuildWithReport
    {
        public int callbackOrder => int.MaxValue;
        
        public void OnPostprocessBuild(BuildReport report)
        {
            try {
                string jsonBody = JsonUtility.ToJson(GetBuildHistoryData(report));
                BuildHistoryApi.SendEvents(jsonBody);
            } catch (Exception ex) {
                Debug.LogError("Error happened while sending analytics data, this wont break the build,"
                    + " but please share the exception below to the VoodooSauce Team");
                Debug.LogException(ex);
            }
        }

        private static BuildHistoryRequestBody GetBuildHistoryData(BuildReport report)
        {
            var buildHistoryRequestBody = new BuildHistoryRequestBody {
                vsc_commit_id = ShellHelper.GetGitCommitHash(),
                vsc_repo_url = ShellHelper.GetGitRepoUrl(),
                vsc_branch_name = ShellHelper.GetGitActiveBranch(),
                vsc_commit_date = ShellHelper.GetGitLastCommitTime(),
                vsc_user_name = ShellHelper.GetGitUsername(),
                app_version = BuildHistoryDataExtractHelper.GetApplicationVersion(),
                unity_version = Application.unityVersion,
                target_platform = EditorUserBuildSettings.activeBuildTarget.ToString(),
                build_error_message = ExtractBuildErrors(report),
                build_machine_os = SystemInfo.operatingSystem,
                bundle_id = Application.identifier,
                development_mode_active = BuildHistoryDataExtractHelper.IsDevelopmentBuild(),
                icon_last_updated_time = ShellHelper.GetLastTimeIconUpdated(),
                icon_updated = ShellHelper.IsIconUpdated(),
                ide_version = GetIdeVersion(),
                is_batch_mode = BuildHistoryDataExtractHelper.IsBatchMode(),
                target_api = BuildHistoryDataExtractHelper.GetAndroidTargetApi(),
                vs_updated = ShellHelper.IsVoodooVersionUpdated(),
                vs_version = BuildHistoryDataExtractHelper.GetVsVersion(),
                vs_last_updated_time = ShellHelper.GetLastTimeVoodooVersionUpdated(),
                app_build_number = BuildHistoryDataExtractHelper.GetBuildNumber(),
                package_json = BuildHistoryDataExtractHelper.GetPackageJson()
            };

            return buildHistoryRequestBody;
        }
        
        private static string GetIdeVersion()
        {
            if(PlatformUtils.IS_OSX && PlatformUtils.UNITY_IOS)
                return ShellHelper.GetXcodeVersion();
            if(PlatformUtils.UNITY_ANDROID) 
                return "Gradle "+ShellHelper.GetAndroidGradleVersion();
            return "";
        }

        private static string ExtractBuildErrors(BuildReport report)
        {
            if (report == null) return "";
            if (report.summary.totalErrors == 0) return "";
            
            var errorMessage = "";
            var steps = report.steps;
            foreach (var step in steps)
            {
                foreach (var buildStepMessage in step.messages)
                {
                    if (buildStepMessage.type == LogType.Error)
                    {
                        errorMessage += ("\n" + buildStepMessage.content);
                    }
                }
            }

            return "BuildPlayer failure: Total error: " + report.summary.totalErrors + "\n MESSAGE:\n" + errorMessage;
        }
    }
}