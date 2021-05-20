using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;
namespace EditorTools.AssetBundle {
    public class AssetPathHelper {
        public static BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
        public static BuildTarget GetBuildTarget() {
            return buildTarget;
        }
    }
}
