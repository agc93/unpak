using System.IO;
using System.Linq;

namespace UnPak.Console
{
    public class DirectoryService
    {
        public DirectoryInfo GetPackRoot(DirectoryInfo inputDir) {
            var allDirs = inputDir.EnumerateDirectories("*", SearchOption.AllDirectories);
            var firstContent = allDirs.FirstOrDefault(d => d.Name == "Content" && d.GetDirectories().Any());
            return firstContent != null ? firstContent.Parent : inputDir;
        }

        public string GetTargetName(DirectoryInfo packDir) {
            return $"{packDir.Parent?.Name}.pak";
        }
    }
}