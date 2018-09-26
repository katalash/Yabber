﻿using SoulsFormats;
using System;
using System.IO;
using System.Reflection;

namespace Yabber
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(
                    "\n" +
                    " Yabber has no GUI.\n" +
                    " Drag and drop a file onto the exe to unpack it,\n" +
                    " or an unpacked folder to repack it.\n\n" +
                    " DCX files will be transparently decompressed and recompressed;\n" +
                    " If you need to decompress or recompress an unsupported format,\n" +
                    " use Yabber.DCX.exe instead.\n\n" +
                    " Yabber version: " + Assembly.GetExecutingAssembly().GetName().Version + "\n" +
                    " Supported formats: BND3, BND4, BXF3, BXF4\n" +
                    " Press any key to exit."
                    );
                Console.ReadKey();
                return;
            }

            bool pause = false;

            foreach (string path in args)
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        pause |= RepackDir(path);
                    }
                    else if (File.Exists(path))
                    {
                        pause |= UnpackFile(path);
                    }
                    else
                    {
                        Console.WriteLine($"File or directory not found: {path}");
                        pause = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unhandled exception: {ex}");
                    pause = true;
                }

                Console.WriteLine();
            }

            if (pause)
            {
                Console.WriteLine("One or more errors were encountered and displayed above.\nPress any key to exit.");
                Console.ReadKey();
            }
        }

        private static bool UnpackFile(string sourceFile)
        {
            string sourceDir = Path.GetDirectoryName(sourceFile);
            string filename = Path.GetFileName(sourceFile);
            string targetDir = $"{sourceDir}\\{filename.Replace('.', '-')}";
            if (DCX.Is(sourceFile))
            {
                Console.WriteLine($"Decompressing DCX: {filename}...");
                byte[] bytes = DCX.Decompress(sourceFile, out DCX.Type compression);
                if (BND3.Is(bytes))
                {
                    Console.WriteLine($"Unpacking BND3: {filename}...");
                    BND3 bnd = BND3.Read(bytes);
                    bnd.Compression = compression;
                    bnd.Unpack(filename, targetDir);
                }
                else if (BND4.Is(bytes))
                {
                    Console.WriteLine($"Unpacking BND4: {filename}...");
                    BND4 bnd = BND4.Read(bytes);
                    bnd.Compression = compression;
                    bnd.Unpack(filename, targetDir);
                }
                else
                {
                    Console.WriteLine($"File format not recognized: {filename}");
                    return true;
                }
            }
            else
            {
                if (BND3.Is(sourceFile))
                {
                    Console.WriteLine($"Unpacking BND3: {filename}...");
                    BND3 bnd = BND3.Read(sourceFile);
                    bnd.Unpack(filename, targetDir);
                }
                else if (BND4.Is(sourceFile))
                {
                    Console.WriteLine($"Unpacking BND4: {filename}...");
                    BND4 bnd = BND4.Read(sourceFile);
                    bnd.Unpack(filename, targetDir);
                }
                else if (BXF3.IsBHD(sourceFile))
                {
                    string bdtExtension = Path.GetExtension(filename).Replace("bhd", "bdt");
                    string bdtFilename = $"{Path.GetFileNameWithoutExtension(filename)}{bdtExtension}";
                    string bdtPath = $"{sourceDir}\\{bdtFilename}";
                    if (File.Exists(bdtPath))
                    {
                        Console.WriteLine($"Unpacking BXF3: {filename}...");
                        BXF3 bxf = BXF3.Read(sourceFile, bdtPath);
                        bxf.Unpack(filename, bdtFilename, targetDir);
                    }
                    else
                    {
                        Console.WriteLine($"BDT not found for BHD: {filename}");
                        return true;
                    }
                }
                else if (BXF4.IsBHD(sourceFile))
                {
                    string bdtExtension = Path.GetExtension(filename).Replace("bhd", "bdt");
                    string bdtFilename = $"{Path.GetFileNameWithoutExtension(filename)}{bdtExtension}";
                    string bdtPath = $"{sourceDir}\\{bdtFilename}";
                    if (File.Exists(bdtPath))
                    {
                        Console.WriteLine($"Unpacking BXF4: {filename}...");
                        BXF4 bxf = BXF4.Read(sourceFile, bdtPath);
                        bxf.Unpack(filename, bdtFilename, targetDir);
                    }
                    else
                    {
                        Console.WriteLine($"BDT not found for BHD: {filename}");
                        return true;
                    }
                }
                else
                {
                    Console.WriteLine($"File format not recognized: {filename}");
                    return true;
                }
            }
            return false;
        }

        private static bool RepackDir(string sourceDir)
        {
            string sourceName = new DirectoryInfo(sourceDir).Name;
            string targetDir = new DirectoryInfo(sourceDir).Parent.FullName;
            if (File.Exists($"{sourceDir}\\_yabber-bnd3.xml"))
            {
                Console.WriteLine($"Repacking BND3: {sourceName}...");
                YBND3.Repack(sourceDir, targetDir);
            }
            else if (File.Exists($"{sourceDir}\\_yabber-bnd4.xml"))
            {
                Console.WriteLine($"Repacking BND4: {sourceName}...");
                YBND4.Repack(sourceDir, targetDir);
            }
            else if (File.Exists($"{sourceDir}\\_yabber-bxf3.xml"))
            {
                Console.WriteLine($"Repacking BXF3: {sourceName}...");
                YBXF3.Repack(sourceDir, targetDir);
            }
            else if (File.Exists($"{sourceDir}\\_yabber-bxf4.xml"))
            {
                Console.WriteLine($"Repacking BXF4: {sourceName}...");
                YBXF4.Repack(sourceDir, targetDir);
            }
            else
            {
                Console.WriteLine($"Yabber XML not found in: {sourceName}");
                return true;
            }
            return false;
        }
    }
}
