using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using UnityEditorInternal;
using System.Reflection;

/*說明
 功能：
 利用功能表  Tools--> Tags+Layers+SortingLayers,
 建立Tags,Layers,和SortingLayer的Class,
 內含同名的變數,
 方便引用Tags,Layers,和SortingLayer,
 避免打錯字的風險與麻煩

 限制：
 Tags,Layers,和SortingLayer的名稱開頭不能有數字，
 轉成變數名稱時會有問題，前面可加個底線以避免這情況

 參考來源：
Namek/TagsLayersEnumBuilder.cs
https://gist.github.com/Namek/ecafa24a6ae3d730baf1

How do you get a list of Sorting Layers via scripting?
http://answers.unity3d.com/questions/585108/how-do-you-access-sorting-layers-via-scripting.html
*/
enum ClassType { Tags, Layers, SortingLayers }

public class TagsSortingLayersBuilder : EditorWindow
{
    [MenuItem("/Tools/Tags+Layers+SortingLayers")]
    static void RebuildTagsAndLayersAndSortingLayers()
    {
        BuildClassFile(ClassType.Tags);
        BuildClassFile(ClassType.Layers);
        BuildClassFile(ClassType.SortingLayers);
    }

    static void BuildClassFile(ClassType classType)
    {
        var enumsPath = InternalEditorUtility.GetAssetsFolder() + "/Scripts/";
        if (!Directory.Exists(enumsPath))
        {
            Directory.CreateDirectory(enumsPath);
        }
        string fileName = "";
        switch (classType)
        {
            case ClassType.Tags:
                fileName = "Tags.cs";
                RebuildTagsFile(enumsPath + fileName);
                break;
            case ClassType.Layers:
                fileName = "Layers.cs";
                RebuildLayersFile(enumsPath + fileName);
                break;
            case ClassType.SortingLayers:
                fileName = "SortingLayers.cs";
                RebuildSortingLayersFile(enumsPath + fileName);
                break;
            default:
                break;
        }
        AssetDatabase.ImportAsset(enumsPath + fileName, ImportAssetOptions.ForceUpdate);
        Debug.Log("已在" + enumsPath + "建立" + fileName);
    }

    static string note = "//這是自動建立的腳本，不要修改，用/Tools/Tags+Layers+SortingLayers 去建立\n";

    static void RebuildTagsFile(string filePath)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append(note);
        sb.Append("public abstract class Tags {\n");

        var srcArr = InternalEditorUtility.tags;
        var tags = new String[srcArr.Length];
        Array.Copy(srcArr, tags, tags.Length);
        Array.Sort(tags, StringComparer.InvariantCultureIgnoreCase);

        for (int i = 0, n = tags.Length; i < n; ++i)
        {
            string tagName = tags[i];

            sb.Append("\tpublic const string " + GetVariableName(tagName) + " = \"" + tagName + "\";\n");
        }

        sb.Append("}\n");

        File.WriteAllText(filePath, sb.ToString());
    }

    static void RebuildLayersFile(string filePath)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append(note);
        sb.Append("public abstract class Layers {\n");

        var layers = InternalEditorUtility.layers;
        for (int i = 0, n = layers.Length; i < n; ++i)
        {
            string layerName = layers[i];

            sb.Append("\tpublic const string " + GetVariableName(layerName) + " = \"" + layerName + "\";\n");
        }

        sb.Append("\n");

        for (int i = 0, n = layers.Length; i < n; ++i)
        {
            string layerName = layers[i];
            int layerNumber = LayerMask.NameToLayer(layerName);
            string layerMask = layerNumber == 0 ? "1" : ("1 << " + layerNumber);

            sb.Append("\tpublic const int " + GetVariableName(layerName) + "Mask" + " = " + layerMask + ";\n");
        }
        sb.Append("\n");

        for (int i = 0, n = layers.Length; i < n; ++i)
        {
            string layerName = layers[i];
            int layerNumber = LayerMask.NameToLayer(layerName);

            sb.Append("\tpublic const int " + GetVariableName(layerName) + "Number" + " = " + layerNumber + ";\n");
        }
        sb.Append("}\n");

        File.WriteAllText(filePath, sb.ToString());
    }

    static void RebuildSortingLayersFile(string filePath)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append(note);
        sb.Append("public abstract class SortingLayers {\n");

        var srcArr = GetSortingLayerNames();
        var SortingLayerNames = new String[srcArr.Length];
        Array.Copy(srcArr, SortingLayerNames, SortingLayerNames.Length);
        Array.Sort(SortingLayerNames, StringComparer.InvariantCultureIgnoreCase);

        for (int i = 0, n = SortingLayerNames.Length; i < n; ++i)
        {
            string SortingLayerName = SortingLayerNames[i];

            sb.Append("\tpublic const string " + GetVariableName(SortingLayerName) + " = \"" + SortingLayerName + "\";\n");
        }

        sb.Append("}\n");
        File.WriteAllText(filePath, sb.ToString());
    }

    public static string[] GetSortingLayerNames()
    {
        Type internalEditorUtilityType = typeof(InternalEditorUtility);
        PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
        return (string[])sortingLayersProperty.GetValue(null, new object[0]);
    }

    static string GetVariableName(string str)
    {
        return str.Replace(" ", "");
    }
}
