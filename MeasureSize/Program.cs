using System;
using System.IO;
using System.Linq;

namespace MeasureSize
{
    class Program
    {
        static (long Size, int NumFiles) ProcessDirectory(string path, string root) {
            // get an approximate measurement from the simulator build
            // by excluding the simulator directory
            if (path.EndsWith("AcquainiOS.app")) {
                return (0, 0);
            }
            var dirInfos = Directory.GetDirectories(path)
                .OrderBy(d => d)
                .Select(d => ProcessDirectory(d, root)).ToList();

            var dirsSize = dirInfos.Select(i => i.Size).Sum();
            var dirsNumFiles = dirInfos.Select(i => i.NumFiles).Sum();

            var fileInfos = Directory.GetFiles(path)
                .OrderBy(f => f)
                .Select(f => ProcessFile(f, root)).ToList();

            var numFiles = fileInfos.Count();
            var filesSize = fileInfos.Sum();

            return (dirsSize + filesSize, dirsNumFiles + numFiles);
        }
        static long ProcessFile(string path, string root) {
            var size = new FileInfo(path).Length;
            var relativePath = Path.Combine(root, Path.GetRelativePath(root, path));
            Console.WriteLine($"{size/1024,6}KB: {relativePath}");
            return size;
        }

        static int Main(string[] args)
        {
            if (args.Length != 1) {
                Console.Error.WriteLine("usage: MeasureSize <dir_path>");
                return -1;
            }
            var root = args[0];
            if (!Directory.Exists(root)) {
                Console.Error.WriteLine($"directory not found: {root}");
                return -1;
            }

            var (totalSize, totalFiles) = ProcessDirectory(root, root);
            Console.WriteLine("--------");
            Console.WriteLine($"total: {totalSize/1024}KB in {totalFiles} files");
            return 0;
        }
    }
}
