﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using V3Lib.Dat;

namespace DatTool
{
    class Program
    {
        const string UsageString = "Usage: DatTool.exe <one or more DAT or CSV files to convert>";

        static void Main(string[] args)
        {
            Console.WriteLine("DAT Tool by CaptainSwag101\n" +
                "Version 1.0.0, built on 2020-08-03\n");

            if (args.Length == 0)
            {
                Console.WriteLine("ERROR: No targets specified.");
                Console.WriteLine(UsageString);
                return;
            }

            foreach (string arg in args)
            {
                FileInfo info = new(arg);
                if (!info.Exists)
                {
                    Console.WriteLine($"ERROR: File \"{arg}\" does not exist, skipping.");
                    continue;
                }

                if (info.Extension.ToLowerInvariant() == ".dat")
                {
                    // Convert DAT to CSV
                    DatFile dat = new();
                    dat.Load(info.FullName);

                    StringBuilder output = new();

                    // Write first row (header)
                    List<string> headerEntries = new();
                    foreach (var def in dat.ColumnDefinitions)
                    {
                        headerEntries.Add($"{def.Name} ({def.Type})");
                    }
                    output.AppendJoin(',', headerEntries);
                    output.Append('\n');

                    // Write row data
                    List<string> rowData = new();
                    for (int row = 0; row < dat.Data.Count; ++row)
                    {
                        StringBuilder rowStr = new();

                        List<string> escapedRowStrs = dat.Data[row];
                        for (int s = 0; s < escapedRowStrs.Count; ++s)
                        {
                            escapedRowStrs[s] = escapedRowStrs[s].Insert(0, "\"").Insert(escapedRowStrs[s].Length + 1, "\"");
                            escapedRowStrs[s] = escapedRowStrs[s].Replace("\n", "\\n").Replace("\r", "\\r");
                        }

                        rowStr.AppendJoin(",", escapedRowStrs);

                        rowData.Add(rowStr.ToString());
                    }
                    output.AppendJoin('\n', rowData);

                    using StreamWriter writer = new(info.FullName.Substring(0, info.FullName.Length - info.Extension.Length) + ".csv", false, Encoding.Unicode);
                    writer.Write(output.ToString());
                }
                else if (info.Extension.ToLowerInvariant() == ".csv")
                {
                    // Convert CSV to DAT
                    DatFile dat = new();

                    using StreamReader reader = new(info.FullName, Encoding.Unicode);

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
                        List<string> rowStrings = new();
                        for (int col = 0; col < rowCells.Length; ++col)
                        {
                            // Update the column definitions with the proper value count
                            colDefinitions[col] = (colDefinitions[col].Name, colDefinitions[col].Type, (ushort)(rowCells[col].Count(c => c == '|') + 1));

                            if (rowCells[col].StartsWith('\"'))
                                rowCells[col] = rowCells[col].Remove(0, 1);

                            if (rowCells[col].EndsWith('\"'))
                                rowCells[col] = rowCells[col].Remove(rowCells[col].Length, 1);

                            rowStrings.Add(rowCells[col].Replace("\\n", "\n").Replace("\\r", "\r"));
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
