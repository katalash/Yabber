﻿using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Yabber
{
    static class YBND3
    {
        public static void Unpack(this BND3 bnd, string sourceName, string targetDir)
        {
            Directory.CreateDirectory(targetDir);
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Indent = true;
            XmlWriter xw = XmlWriter.Create($"{targetDir}\\_yabber-bnd3.xml", xws);
            xw.WriteStartElement("bnd3");

            xw.WriteElementString("filename", sourceName);
            xw.WriteElementString("compression", bnd.Compression.ToString());
            xw.WriteElementString("timestamp", bnd.Timestamp);
            xw.WriteElementString("format", $"0x{bnd.Format:X2}");
            xw.WriteElementString("bigendian", bnd.BigEndian.ToString());
            xw.WriteElementString("unk1", bnd.Unk1.ToString());
            xw.WriteElementString("unk2", $"0x{bnd.Unk2:X8}");

            xw.WriteStartElement("files");
            foreach (BND3.File file in bnd.Files)
            {
                string outPath = Util.UnrootBNDPath(file.Name);

                xw.WriteStartElement("file");
                xw.WriteElementString("id", file.ID.ToString());
                xw.WriteElementString("name", file.Name ?? "<null>");
                xw.WriteElementString("path", outPath);
                xw.WriteElementString("flags", $"0x{file.Flags:X2}");
                xw.WriteEndElement();

                outPath = $"{targetDir}\\{outPath}";
                Directory.CreateDirectory(Path.GetDirectoryName(outPath));
                System.IO.File.WriteAllBytes(outPath, file.Bytes);
            }
            xw.WriteEndElement();

            xw.WriteEndElement();
            xw.Close();
        }

        public static void Repack(string sourceDir, string targetDir)
        {
            BND3 bnd = new BND3();
            XmlDocument xml = new XmlDocument();
            xml.Load($"{sourceDir}\\_yabber-bnd3.xml");

            string filename = xml.SelectSingleNode("bnd3/filename").InnerText;
            Enum.TryParse(xml.SelectSingleNode("bnd3/compression").InnerText, out bnd.Compression);
            bnd.Timestamp = xml.SelectSingleNode("bnd3/timestamp").InnerText;
            bnd.Format = Convert.ToByte(xml.SelectSingleNode("bnd3/format").InnerText, 16);
            bnd.BigEndian = bool.Parse(xml.SelectSingleNode("bnd3/bigendian").InnerText);
            bnd.Unk1 = bool.Parse(xml.SelectSingleNode("bnd3/unk1").InnerText);
            bnd.Unk2 = Convert.ToInt32(xml.SelectSingleNode("bnd3/unk2").InnerText, 16);
            
            foreach (XmlNode fileNode in xml.SelectNodes("bnd3/files/file"))
            {
                int id = int.Parse(fileNode.SelectSingleNode("id").InnerText);
                string name = fileNode.SelectSingleNode("name").InnerText;
                string path = fileNode.SelectSingleNode("path").InnerText;
                byte flags = Convert.ToByte(fileNode.SelectSingleNode("flags").InnerText, 16);

                if (name == "<null>")
                    name = null;
                byte[] bytes = File.ReadAllBytes($"{sourceDir}\\{path}");

                bnd.Files.Add(new BND3.File(id, name, flags, bytes));
            }

            string outPath = $"{targetDir}\\{filename}";
            if (File.Exists(outPath) && !File.Exists(outPath + ".bak"))
                File.Move(outPath, outPath + ".bak");

            bnd.Write(outPath);
        }
    }
}
