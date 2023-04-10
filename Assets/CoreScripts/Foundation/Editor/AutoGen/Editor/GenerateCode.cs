using System.IO;
using UnityEditor;
using DotLiquid;

namespace ible.Editor.Utility
{
    public class GenerateCode
    {
        public static void Generate(string genCodeFilePath, string genCodeTemplate, Hash hash, bool existSkip = false)
        {
            if (existSkip && File.Exists(genCodeFilePath))
                return;

            // 生成代码
            var template = Template.Parse(genCodeTemplate);

            if (!string.IsNullOrEmpty(genCodeFilePath))
            {
                var genCode = template.Render(hash);
                if (File.Exists(genCodeFilePath)) // 存在，比较是否相同
                {
                    if (File.ReadAllText(genCodeFilePath) != genCode)
                    {
                        EditorUtility.ClearProgressBar();
                        // 不同，会触发编译，强制停止Unity后再继续写入
                        if (EditorApplication.isPlaying)
                        {
                            //Log.Error("[CAUTION]AppSettings code modified! Force stop Unity playing");
                            EditorApplication.isPlaying = false;
                        }
                        File.WriteAllText(genCodeFilePath, genCode, System.Text.Encoding.UTF8);
                    }
                }
                else
                    File.WriteAllText(genCodeFilePath, genCode, System.Text.Encoding.UTF8);
            }
        }
    }
}