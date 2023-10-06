using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class SDKListPreBuild : IPreprocessBuildWithReport
{
    public int callbackOrder => 9999;

    public void OnPreprocessBuild(BuildReport report)
    {
        SDKListGenerator.GenerateSDKList();
    }
}
