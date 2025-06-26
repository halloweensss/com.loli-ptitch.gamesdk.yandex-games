using System.IO;
using GameSDK.Extensions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Plugins.YaGames.Editor
{
    public class BuildPostProcessor : IPostprocessBuildWithReport
    {
        private const string YandexSDKScript = "<script src=\"/sdk.js\"></script>";
        
        public int callbackOrder => 0;
        
        public void OnPostprocessBuild(BuildReport report)
        {
            var summary = report.summary;
            
            if (summary.platform != BuildTarget.WebGL)
                return;

            var indexHtmlPath = Path.Combine(summary.outputPath, "index.html");
            
            if(File.Exists(indexHtmlPath) == false)
                return;
            
            var htmlContent = File.ReadAllText(indexHtmlPath);
            var pattern = @"<script\s+src=[""']https:\/\/yandex\.ru\/games\/sdk\/v2[^""']*[""'][^>]*><\/script>";

            htmlContent = BuildExtension.RemovePatterns(htmlContent, pattern);
            htmlContent = BuildExtension.Append(htmlContent, HtmlTag.Head, "YaGamesSDK", YandexSDKScript);
            htmlContent = BuildExtension.RemoveEmptyLines(htmlContent);
            
            File.WriteAllText(indexHtmlPath, htmlContent);
        }
    }
}