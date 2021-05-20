using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

using EditorTools.AssetBundle;
using EditorTools.UI;

public class TextureImporterUtil {
    public static bool useToolCut = false;
    //创建贴图导入的设置对象
    public static TextureImporterPlatformSettings CreateImporterSetting(string name, int maxSize, TextureImporterFormat format, int compressionQuality = 100,
         bool allowsAlphaSplitting = false, TextureImporterCompression tc = TextureImporterCompression.Uncompressed) {
        TextureImporterPlatformSettings tips = new TextureImporterPlatformSettings();
        tips.overridden = true;
        tips.name = name;
        tips.maxTextureSize = maxSize;
        tips.format = format;
        tips.textureCompression = tc;
        tips.allowsAlphaSplitting = allowsAlphaSplitting;
        tips.compressionQuality = compressionQuality;

        return tips;
    }
    //导入贴图,并返回一个AssetsImport对象
    public static void CreateReadableTextureImporter(string path) {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) {
            Debug.LogError("发现不是图片的资源, 资源路径 = " + path);
            return;
        }
        importer.textureType = TextureImporterType.Sprite;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.isReadable = true;
        importer.mipmapEnabled = false;
        //为import对象进行导入参数的设置
        importer.SetPlatformTextureSettings(TextureImporterUtil.CreateImporterSetting(GetTextureBuildTargetName(), 2048, TextureImporterFormat.RGBA32));
    }

    // 返回贴图打包的平台名字
    public static string GetTextureBuildTargetName() {
        switch (AssetPathHelper.GetBuildTarget()) {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
            case BuildTarget.StandaloneOSX:
                return "Standalone";
            case BuildTarget.Android:
                return "Android";
            case BuildTarget.iOS:
                return "iPhone";
        }
        return "Standalone";
    }
    //多图导入Importer的设置 参数列表含义
    //path 路径,rects uv坐标和大小
    public static void CreateMultipleSpriteImporter(string path, Rect[] rects, string[] spriteNames, Vector4[] borders, int width, int height, int maxSize, TextureImporterFormat format) {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter; //贴图导入器
        if (importer == null) { Debug.LogError("发现不是图片的资源, 资源路径 = " + path); return; } //资源不属于图片资源,无法获取资源上的贴图导入器
        importer.textureType = TextureImporterType.Sprite;//按照Sprite(2D and UI)的模式导入
        importer.spriteImportMode = SpriteImportMode.Multiple;//导入的类型为多图导入
        importer.spritePixelsPerUnit = 1; //图片中多少个像素对应Unity项目中的1个unit
        SpriteMetaData[] metaDatas = new SpriteMetaData[spriteNames.Length]; //用于生成Sprite的编辑器数据
        for (int i = 0; i < metaDatas.Length; i++) {
            SpriteMetaData metaData = new SpriteMetaData();
            metaData.name = spriteNames[i];
            Rect rect = rects[i];
            //布局的计算 布局计算公式及图解见下图
            if (rects.Length > 1) {
                //根据sprite在图集中的rect来计算像素单位的矩形
                metaData.rect = new Rect(
                     rect.xMin * width + TextureClamper.BORDER, //左边的百分比 * 图集宽度 + 边界
                     rect.yMin * height + TextureClamper.BORDER, //下边的百分比 * 图集高度 + 边界
                     rect.width * width - TextureClamper.BORDER * 2,//sprite所占整张图集的宽度百分比 * 图集宽度 - 边界 * 2
                    rect.height * height - TextureClamper.BORDER * 2//sprite所占整张图集的高度百分比 * 图集高度 - 边界 * 2
                );
            } else {
                metaData.rect = new Rect(rect.xMin * width, rect.yMin * height, rect.width * width, rect.height * height);
            }
            //sprite的边缘边框大小(以像素为单位)
            if (borders != null) {
                metaData.border = borders[i];
            }
            metaData.pivot = new Vector2(0.5f, 0.5f);
            metaDatas[i] = metaData;
        }

        importer.spritesheet = metaDatas;//用于表示与各sprite图形对应的图集部分的数组。
        importer.maxTextureSize = maxSize;//sprite的尺寸
        importer.isReadable = false;//关闭可读
        importer.mipmapEnabled = false;//mipMapEnable关闭
        importer.crunchedCompression = true;//使用经过 Crunch 处理的压缩(如果可用)
        importer.spritePackingTag = "";
        importer.SetPlatformTextureSettings(TextureImporterUtil.CreateImporterSetting(GetTextureBuildTargetName(), 2048, format, 100, false));
    }
}
