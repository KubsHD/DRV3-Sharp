﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using V3Lib.Dat;

namespace DatTool
{
    class Program
    {
        const string UsageString = "Usage: DatTool.exe <one or more DAT or CSV files to convert>";

        static void Main(string[] args)
        {
            Console.WriteLine("DAT Tool by CaptainSwag101\n" +
                "Version 0.0.3, built on 2020-05-28\n");

            if (args.Length == 0)
            {
                Console.WriteLine("ERROR: No targets specified.");
                Console.WriteLine(UsageString);
                return;
            }

            foreach (string arg in args)
            {
                FileInfo info = new FileInfo(arg);
                if (!info.Exists)
                {
                    Console.WriteLine($"ERROR: File \"{arg}\" does not exist, skipping.");
                    continue;
                }

                if (info.Extension.ToLowerInvariant() == ".dat")
                {
                    // Convert DAT to ODS
                    DatFile dat = new DatFile();
                    dat.Load(info.FullName);

                    dat.SaveAsODS(info.FullName.Substring(0, info.FullName.Length - info.Extension.Length) + ".ods");
                }
                else if (info.Extension.ToLowerInvariant() == ".csv")
                {
                    // Convert CSV to DAT
                    DatFile dat = new DatFile();

                    using StreamReader reader = new StreamReader(info.FullName, Encoding.Unicode);

                    // First line is column definitions
                    string[] header = reader.ReadLine().Split(',');
                    var colDefinitions = new List<(string Name, string Type, ushort Count)>();
                    foreach (string headerPiece in header)
                    {
                        string name = headerPiece.Split('(').First();
                        string type = headerPiece.Split('(').Last().TrimEnd(')');
                        colDefinitions.Add((name, type, 0));
                    }

                    // Read row data
                    while (!reader.EndOfStream)
                    {
                        string[] rowCells = reader.ReadLine().Split(',');
                        List<string> rowStrings = new List<string>();
                        for (int col = 0; col < rowCells.Length; ++col)
                        {
                            // Update the column definitions with the proper value count
                            colDefinitions[col] = (colDefinitions[col].Name, colDefinitions[col].Type, (ushort)(rowCells[col].Count(c => c == '|') + 1));

                            rowStrings.Add(rowCells[col]);
                        }
                        dat.Data.Add(rowStrings);
                    }
                    dat.ColumnDefinitions = colDefinitions;

                    dat.Save(info.FullName.Substring(0, info.FullName.Length - info.Extension.Length) + ".dat");
                }
                else
                {
                    Console.WriteLine($"ERROR: Invalid file extension \"{info.Extension}\".");
                    continue;
                }
            }
        }
    }
}
