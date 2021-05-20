using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;
namespace EditorTools.UI {
    // 处理Icon资源，分成两种模式，Single：每一张Icon图片做成一个独立的图集，Mutilple：将一个目录下的所有图片合成一个图集
    // Single和Mutilple对应Assets/Icon目录下两个文件夹 因为Icon需要使用内建的Sprite Packer分离通道的功能，所以Icon目录不能放在Resources目录下
    public class IconProcessor {
        public const int ATLAS_MAX_SIZE = 2048;
        public const int FAVOR_ATLAS_SIZE = 1024;
        public const string ICON_ROOT = "Assets/IconOrigin";
        public const string SINGLE_ROOT = "Assets/IconOrigin/Single";
        public const string MUTLIPLE_ROOT = "Assets/IconOrigin/Mutliple";
        // UI预设中不可以引用Icon图标
        public const string ICON_OUT_ROOT = "Assets/Icon";
        public const string SINGLE_OUT_ROOT = "Assets/Icon/Single";
        public const string MUTLIPLE_OUT_ROOT = "Assets/Icon/Mutliple";

        [MenuItem("Assets/MakeIconAtlas", false, 102)]
        public static void Main() { //主方法,MakeIconAtlas进来执行的地方
            Object[] objs = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);//过滤选中的对象,这里只筛选Assets类型的
            foreach (Object obj in objs) { //遍历筛选过后选中的内容
                string path = AssetDatabase.GetAssetPath(obj); //获取obj的路径
                string selectedPath = GetSelectedPath(path); //检测路径,按照上面预定义的路径进行审核,返回通过审核的路径
                if (string.IsNullOrEmpty(selectedPath) == true) { //这里做兼容和判空
                    return;
                }
                Debug.Log("Selected Path: " + selectedPath);
                if (selectedPath.Contains(".png") == true) { //选择的路径包含了.png字段
                    EnterByFile(selectedPath); //文件模式
                } else {
                    EnterByFolder(selectedPath);//文件夹
                }
            }
        }

        //文件模式
        private static void EnterByFile(string path) {
            if (path.Contains(SINGLE_ROOT) == true) { //路径包含单图路径
                PackSingleMode(path);//打包单图
            } else if (path.Contains(MUTLIPLE_ROOT) == true) { //包含多图路径
                string folderPath = GetFolderPath(path); //记录文件夹路径
                PackFolderMode(folderPath);//按文件夹路径打包文件夹内图片的图集
            }
        }
        //文件夹模式
        private static void EnterByFolder(string path) {
            if (path.Contains(SINGLE_ROOT) == true) { //包含单图路径
                string[] paths = GetAssetPaths(path);//选中单图路径下的所有文件,并把每个文件的路径返回到paths数组
                foreach (string s in paths) { //遍历资源路径列表
                    PackSingleMode(s);//打单图
                }
            } else if (path.Contains(MUTLIPLE_ROOT) == true) { //包含多图路径
                if (path.Length > MUTLIPLE_ROOT.Length) { //路径string长度大于多图根路径长度,说明选择了一个多图文件夹路径
                    PackFolderMode(path);//按多图路径打包
                } else { //处理选中了多图文件夹根目录的情况,把根目录包含的子文件夹都进行打包
                    string[] paths = GetSubFolderPaths(path); //划分成每个多图文件夹目录
                    foreach (string s in paths) { //遍历多个目录
                        PackFolderMode(s); //按每个多图路径打包
                    }
                }
            }
        }
        //传入一个文件夹的路径
        private static string[] GetSubFolderPaths(string folderPath) {
            string[] result = Directory.GetDirectories(folderPath); //返回文件夹目录下所有文件夹的路径到result数组
            for (int i = 0; i < result.Length; i++) { //遍历数组
                result[i] = result[i].Replace(@"\", @"/"); //把所有的'\'替换成'/'
            }
            return result; //返回替换好文件路径的路径数组
        }
        //路径下的所有文件,并把每个文件的路径返回到result数组
        private static string[] GetAssetPaths(string folderPath) {
            //筛选
            //SearchOption.TopDirectoryOnly默认选项，仅包含当前目录     SearchOption.AllDirectories包含所有子目录
            // "*.*",代表模糊搜索,检索出带.的文件,"s => s.Contains(".meta") == false"表示过滤掉meta文件
            string[] result = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly).Where<string>(s => s.Contains(".meta") == false).ToArray<string>();
            for (int i = 0; i < result.Length; i++) { //通过过滤的文件列表,此时只包含图片资源
                result[i] = result[i].Replace(@"\", @"/");//把所有的'\'替换成'/'
            }
            return result;
        }
        //获取文件路径的目录
        private static string GetFolderPath(string path) {
            int lastSlashIndex = path.LastIndexOf(@"/"); //获取最后一个'/'的字符索引
            return path.Substring(0, lastSlashIndex);//0 - 最后一个/字符索引即文件的目录
        }
        //文件夹打包模式
        private static void PackFolderMode(string path) {
            string[] paths = GetAssetPaths(path); //获取文件夹下所有子文件Asset类型的路径,过滤掉Unity的.meta文件
            foreach (string s in paths) { //遍历Asset的路径列表,将每一个图片都进行导入
                ImportReadableTexture(s); //导入贴图
            }
            //atlasPath 是最终输出的图片路径.png
            string atlasPath = path.Replace(MUTLIPLE_ROOT, MUTLIPLE_OUT_ROOT) + ".png";//这里的path表示目录路径且结尾不含/,将多图路径替换成多图输出路径
            string folderPath = Path.GetDirectoryName(atlasPath);//返回指定路径字符串的目录信息,存在哪个文件夹
            if (Directory.Exists(folderPath) == false) { //不存在该文件夹就创建一个新的
                Directory.CreateDirectory(folderPath);
            }
            CreateFolderModeAtlas(paths, atlasPath); //paths assets的路径列表
        }

        private static void PackSingleMode(string path) {
            ImportReadableTexture(path); //导入贴图
            string atlasPath = path.Replace(SINGLE_ROOT, SINGLE_OUT_ROOT);//单图的输出路径
            string folderPath = Path.GetDirectoryName(atlasPath);//返回指定路径字符串的目录信息,存在哪个文件夹
            if (Directory.Exists(folderPath) == false) { //不存在该文件夹就创建一个新的
                Directory.CreateDirectory(folderPath);
            }
            CreateSingleModeAtlas(path, atlasPath);//单图创建模式 path assets的路径列表
        }
        //将path路径的单图生成单图图集到atlasPath路径
        private static void CreateSingleModeAtlas(string path, string atlasPath) {
            Texture2D texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D; //把texture2D的贴图数据加载出来
            AtlasWriter.Write(texture, atlasPath);//将贴图数据写入atlasPath
            AssetDatabase.ImportAsset(atlasPath, ImportAssetOptions.ForceUpdate);//导入数据
            TextureImporter importer = AssetImporter.GetAtPath(atlasPath) as TextureImporter;//获取路径下的单图图片上的TextureImporter
            if (importer == null) { Debug.LogError("发现不是图片的资源, 资源路径 = " + atlasPath); return; } //不是图片资源就报错,找不到TextureImporter
            importer.textureType = TextureImporterType.Sprite;//贴图模式为Sprite
            importer.spriteImportMode = SpriteImportMode.Single;//导入模式为单图
            importer.spritePixelsPerUnit = 1;//图片中多少个像素对应Unity项目中的1个unit
            importer.maxTextureSize = ATLAS_MAX_SIZE;//最大尺寸
            importer.isReadable = false;//关闭可读
            importer.mipmapEnabled = false;//mipMapEnable关闭
            importer.crunchedCompression = true;//使用经过 Crunch 处理的压缩(如果可用)
            importer.spritePackingTag = "";
            //TextureImporterUtil.SetAtlasPackingTag(atlasPath, texture.name);
            AssetDatabase.ImportAsset(atlasPath, ImportAssetOptions.ForceUpdate);//导入资源数据
        }
        //将一个多图文件夹的所有贴图资源的纹理数据打包填充成一张大贴图
        private static void CreateFolderModeAtlas(string[] paths, string atlasPath) {
            Texture2D[] textures = new Texture2D[paths.Length]; //Texture数组,一个多图文件夹中包含的图片个数,每个图片资源对应一个Texture
            string[] textureNames = new string[textures.Length]; //texture的资源名称数组
            for (int i = 0; i < textures.Length; i++) { //遍历
                textures[i] = AssetDatabase.LoadAssetAtPath(paths[i], typeof(Texture2D)) as Texture2D; //按路径加载贴图资源
                textureNames[i] = textures[i].name;//命名
                textures[i] = TextureClamper.Clamp(textures[i]); //填充图集的Texture数据
            }
            Texture2D atlas = new Texture2D(ATLAS_MAX_SIZE, ATLAS_MAX_SIZE); //创建图集
            //将多个纹理打包到一个纹理图集中。Rect[] 包含每个输入纹理在图集中的 UV 坐标的矩形数组，如果打包失败，则为 null。
            //rects 为每个texture的矩形框数组
            Rect[] rects = atlas.PackTextures(textures, 0, ATLAS_MAX_SIZE, false);//textures要打包到图集的纹理的数组,0 纹理间的像素填充,ATLAS_MAX_SIZE 纹理最大尺寸,false,不可读?false
            AtlasWriter.Write(atlas, atlasPath);//写入纹理数据
            AssetDatabase.ImportAsset(atlasPath, ImportAssetOptions.ForceUpdate);//导入atlasPath路径下的资源,由用户发起的资源导入。
            //将每一张sprite导入一个大图集,路径,uv信息,名称,边界vector4精灵的边缘边框大小[以像素为单位]X=左边框/Y=下边框/Z=右边框/W=上边框
            TextureImporterUtil.CreateMultipleSpriteImporter(
                 atlasPath,//大图的路径
                 rects,//每张Sprite的矩形UV框
                textureNames,//每张Sprite的名称
                new Vector4[textures.Length],//border数组
                atlas.width,//图集的宽度
                atlas.height,//图集的高度
                ATLAS_MAX_SIZE,//图集的最大尺寸
                 TextureImporterFormat.DXT5 //纹理导入的格式
             );
            //TextureImporterUtil.SetAtlasPackingTag(atlasPath, GetPackingTagFromPath(atlasPath));
            AssetDatabase.ImportAsset(atlasPath, ImportAssetOptions.ForceUpdate);//导入atlasPath路径下的资源,由用户发起的资源导入。
        }

        //按照打包路径获取打包Tag,这个规则需要约定好
        private static string GetPackingTagFromPath(string path) {
            int lastSlashIndex = path.LastIndexOf(@"/");//获取最后一个斜杠的索引
            int lastDotIndex = path.LastIndexOf(@".");//获取最后一个点的索引
            return path.Substring(lastSlashIndex + 1, (lastDotIndex - lastSlashIndex - 1));//最后一个斜杠的后一位开始,到最后一个点的位置
        }
        //导入贴图
        private static void ImportReadableTexture(string path) {
            TextureImporterUtil.CreateReadableTextureImporter(path);//创建导入器
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        //检测资源文件路径和文件夹路径
        private static string GetSelectedPath(string path) {
            if (path.Contains(SINGLE_ROOT) == false && path.Contains(MUTLIPLE_ROOT) == false) { //路径不包含单图路径也不包含多图路径,检测不通过
                Debug.LogError("选择的路径是： " + path + " 错误，请选择Assets/IconOrigin/Single或Assets/IconOrigin/Mutliple目录下icon资源~");
                return string.Empty;
            }
            if (path.Contains(SINGLE_OUT_ROOT) == true || path.Contains(MUTLIPLE_OUT_ROOT) == true) { //路径包含了单图输出目录或者多图输出目录,检测不通过
                Debug.LogError("选择的路径是： " + path + " 错误，请选择Assets/IconOrigin/Single或Assets/IconOrigin/Mutliple目录下icon资源~");
                return string.Empty;
            }
            return path;
        }
    }
}