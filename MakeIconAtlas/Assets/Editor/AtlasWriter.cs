using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorTools.UI {
    public class AtlasWriter {
        public const int FAVOR_ATLAS_SIZE = 2048;

        public static void Write(Texture2D atlas, string path) { //将Texture2D数据写入路径的图片资源
            byte[] pngData = atlas.EncodeToPNG(); //编码成png的字节数组
            string pngPath = Application.dataPath + path.Replace("Assets", "");//获取真实的物理路径
            File.WriteAllBytes(pngPath, pngData);//将字节流数据写入路径的png文件

            LogAtlasSize(atlas, path);//打印结果信息
        }

        private static void LogAtlasSize(Texture2D atlas, string path) {
            if (atlas.width > FAVOR_ATLAS_SIZE || atlas.height > FAVOR_ATLAS_SIZE) {
                Debug.Log(string.Format("<color=#ff0000>【警告】图集宽度或高度超过2048像素： {0} </color>", path));//大于2048 x 2048就警告
            } else {
                Debug.Log(string.Format("<color=#13ffe7>图集 {0} 尺寸为： {1}x{2}</color>", path, atlas.width, atlas.height));
            }
        }
    }
}
