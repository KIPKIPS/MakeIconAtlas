using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;
namespace EditorTools.AssetBundle {
    public class AssetPathHelper {
        public const string PATH_RESOURCES = "Assets/Things/";
        public const string PATH_RESOURCES_TEMP = "Assets/Things_temp/";
        public const string POSTFIX_FOLDER = ".folder";
        public const string POSTFIX_SELECTION = ".selection";
        public const string POSTFIX_SINGLE = ".single";
        public const string FBX = ".fbx";
        public const string TTF = ".ttf";

        // folder模式打包中，正则表达式中必须定义最终生成AB文件路径名的子模式

        public const string REGEX_TOKEN_PATH = "path";

        // selection模式打包中，正则表达式中可以定义最终生成AB文件后缀名的子模式

        public const string REGEX_TOKEN_POSTFIX = "postfix";
        private const string TOKEN_ASSETS = "Assets";
        // 打包平台，要在打包开始指定
        public static BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
        public static string patchVersion = null;
        public static string ReplaceSlash(string path) {
            return path.Replace("/", "$");
        }
        public static string EliminateStartToken(string path) {
            if (path.StartsWith(PATH_RESOURCES) == true) {
                return path.Substring(PATH_RESOURCES.Length);
            }
            if (path.StartsWith(PATH_RESOURCES_TEMP) == true) {
                return path.Substring(PATH_RESOURCES_TEMP.Length);
            }
            return path;
        }
        // entryPath:打包入口文件路径 assetPathList:筛选出的文件列表 pattern:筛选文件的匹配模式
        // public static string GetSelectionModeBundlePath(string entryPath, List<string> assetPathList, Regex pattern) {
        //     return GetSelectionModeBundlePath(entryPath, assetPathList[0], pattern).Replace(" ", "");
        // }
        // private static string GetSelectionModeBundlePath(string entryPath, string assetPath, Regex pattern) {
        //     string result = GetSelectionModeStartPath(entryPath);
        //     string postfix = GetPostfix(assetPath, pattern);
        //     return ReplaceSlash(EliminateStartToken(result + postfix + POSTFIX_SELECTION)).ToLower();
        // }
        // public static string GetSingleModeBundlePath(string path) {
        //     return (ReplaceSlash(EliminateStartToken(path)) + POSTFIX_SINGLE).ToLower().Replace(" ", "");
        // }
        // public static string GetFolderModeBundlePath(List<string> assetPathList, Regex pattern) {
        //     return GetFolderModeBundlePath(assetPathList[0], pattern).Replace(" ", "");
        // }
        // private static string GetFolderModeBundlePath(string assetPath, Regex pattern) {
        //     string result = GetFolderModeStartPath(assetPath, pattern);
        //     string postfix = GetPostfix(assetPath, pattern);
        //     return ReplaceSlash(EliminateStartToken(result + postfix + POSTFIX_FOLDER)).ToLower();
        // }
        // private static string GetPostfix(string path, Regex pattern) {
        //     GroupCollection gc = pattern.Match(path).Groups;
        //     string result = gc[REGEX_TOKEN_POSTFIX].Value;
        //     if (string.IsNullOrEmpty(result) == true) {
        //         return string.Empty;
        //     }
        //     return "." + result.ToLower();
        // }
        // folder模式下的资源包路径
        // public static string GetFolderModeStartPath(string path, Regex pattern) {
        //     GroupCollection gc = pattern.Match(path).Groups;
        //     string result = gc[REGEX_TOKEN_PATH].Value;
        //     int lastSlashIndex = result.LastIndexOf(@"/");
        //     if (lastSlashIndex == result.Length - 1) {
        //         result = result.Substring(0, result.Length - 1);
        //     }
        //     return result;
        // }
        // // selection模式下资源包路径
        // public static string GetSelectionModeStartPath(string path) {
        //     return path.Substring(0, path.LastIndexOf("."));
        // }
        // entryPath:打包入口资源路径
        // assetPath:打包入口资源所依赖的资源路径
        // public static string GetObjectKey(string entryPath, string assetPath, Object obj, StrategyNode node) {
        //     string token = string.Empty;
        //     if (string.IsNullOrEmpty(assetPath)) {
        //         throw new Exception("Asset physicalPath should not be empty!");
        //     }

        //     if (node.mode == PackageMode.SINGLE) {
        //         token = GetSingleModeBundlePath(assetPath);
        //         //字体ttf和模型文件是特殊的类型，一个Asset中包含多个Object
        //         //AssetDatabase.LoadAllAssetsAtPath(assetPath)返回长度大于一
        //         if (token.Contains(FBX) == true || token.Contains(TTF) == true) {
        //             token = token + obj.GetType().Name + obj.name;
        //         }
        //     } else if (node.mode == PackageMode.FOLDER) {
        //         token = GetFolderModeBundlePath(assetPath, node.pattern) + obj.GetType().Name + obj.name;
        //     } else if (node.mode == PackageMode.SELECTION) {
        //         token = GetSelectionModeBundlePath(entryPath, assetPath, node.pattern) + obj.GetType().Name + obj.name;
        //     }
        //     return token;
        // }
        // public static string ToFileSystemPath(string assetPath) {
        //     return Application.dataPath.Replace(TOKEN_ASSETS, "") + assetPath;
        // }
        // public static string ToAssetPath(string systemPath) {
        //     return "Assets" + systemPath.Substring(Application.dataPath.Length);
        // }
        // 获取AB文件的输出路径
        // public static string GetOutputPath(string assetbundleName) {
        //     string basePath = AssetBuildStrategyManager.outputPath;
        //     return basePath + assetbundleName;
        // }
        // public static Dictionary<BuildTarget, string> GetBuildTargetIdentifierDict() {
        //     Dictionary<BuildTarget, string> result = new Dictionary<BuildTarget, string>();
        //     result.Add(BuildTarget.Android, "android");
        //     result.Add(BuildTarget.StandaloneWindows, "pc");
        //     result.Add(BuildTarget.StandaloneWindows64, "pc");
        //     result.Add(BuildTarget.StandaloneOSX, "mac");
        //     result.Add(BuildTarget.iOS, "ios");
        //     return result;
        // }
        // public static string GetBuildTarget(BuildTarget pbuildTarget) {
        //     switch (pbuildTarget) {
        //         case BuildTarget.Android:
        //             return "android";
        //         case BuildTarget.StandaloneWindows:
        //             return "pc";
        //         case BuildTarget.StandaloneWindows64:
        //             return "pc";
        //         case BuildTarget.StandaloneOSX:
        //             return "mac";
        //         case BuildTarget.iOS:
        //             return "ios";
        //         default:
        //             throw new Exception("无法识别平台" + pbuildTarget.ToString());
        //     }
        // }

        // public static BuildTarget GetBuildTarget(string platform) {
        //     switch (platform) {
        //         case "pc":
        //             // 如果是在mac开发，在mac上运行的，这里改成BuildTarget.StandaloneOSXIntel64
        //             // 但如果是使用mac编译，在windows系统上运行，这里还是BuildTarget.StandaloneWindows64
        //             return BuildTarget.StandaloneWindows64;
        //         case "ios":
        //             return BuildTarget.iOS;
        //         case "android":
        //             return BuildTarget.Android;
        //         case "mac":
        //             return BuildTarget.StandaloneOSX;
        //         default:
        //             throw new System.Exception("无法识别的平台标识: " + platform);
        //     }
        // }
        // public static string GetBuildTargetTxt() {
        //     return GetBuildTarget(GetBuildTarget());
        // }
        public static BuildTarget GetBuildTarget() {
            return buildTarget;
        }
        // public static string GetPatchVersion() {
        //     return patchVersion;
        // }
    }
}
