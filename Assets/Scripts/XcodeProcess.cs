#if UNITY_EDITOR && UNITY_IOS
using UnityEngine;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using UnityEditor.Callbacks;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class XcodeProcess
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget BuildTarget, string path)
    {
        if (BuildTarget == BuildTarget.iOS)
        {
            string viewControllerBasePath = path + "/Classes/UI/UnityViewControllerBase+iOS.mm";

            if (File.Exists(viewControllerBasePath))
            {
                string viewControllerBaseString = File.ReadAllText(viewControllerBasePath);

                viewControllerBaseString = System.Text.RegularExpressions.Regex.Replace(viewControllerBaseString, @"- \(UIRectEdge\)preferredScreenEdgesDeferringSystemGestures\n{\n[\s\S]*?}", "- (UIRectEdge)preferredScreenEdgesDeferringSystemGestures\n{\n    return UIRectEdgeAll; \n}");

                File.WriteAllText(viewControllerBasePath, viewControllerBaseString);
            }
            else
            {
                Debug.LogError(viewControllerBasePath + " Can not find");

            }

            string plistPath = path + "/Info.plist";

            if (File.Exists(viewControllerBasePath))
            {
                PlistDocument plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(plistPath));

                PlistElementDict rootDict = plist.root;

                rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);
                rootDict.SetString("NSLocationWhenInUseUsageDescription", "需要定位权限");

                File.WriteAllText(plistPath, plist.WriteToString());
            }
            else
            {
                Debug.LogError(plistPath + " Can not find");
            }


        }
    }
}
#endif