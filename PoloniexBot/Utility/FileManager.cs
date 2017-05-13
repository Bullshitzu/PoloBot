using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Utility {
    public static class FileManager {

        public static string[] ReadFile (string path) {
            if (!File.Exists(path)) {
                ErrorLog.ReportError("File at specified path does not exist! - " + path);
                return null;
            }

            StreamReader reader;

            try {
                reader = new StreamReader(path);
            }
            catch (Exception e) {
                ErrorLog.ReportError("Failed opening file for read: \"" + path + "\"", e);
                return null;
            }

            List<string> lines = new List<string>();

            try {
                while (!reader.EndOfStream) {
                    lines.Add(reader.ReadLine());
                }
            }
            catch (Exception e) {
                ErrorLog.ReportError("Error while reading file \"" + path + "\"", e);
                return null;
            }
            finally {
                reader.Close();
            }

            return lines.ToArray();
        }

        public static void SaveFile (string path, string[] lines) {
            WriteFile(path, lines, false);
        }
        public static void SaveFileConcat (string path, string[] lines) {
            WriteFile(path, lines, true);
        }

        private static void WriteFile (string path, string[] lines, bool concat) {
            // Console.WriteLine("writing " + lines.Length + " lines to " + path);

            StreamWriter writer;

            try {
                writer = new StreamWriter(path, concat);
            }
            catch (Exception e) {
                ErrorLog.ReportError("Failed opening file for write: \"" + path + "\"", e);
                return;
            }

            try {
                for (int i = 0; i < lines.Length; i++) {
                    writer.WriteLine(lines[i]);
                }
            }
            catch (Exception e) {
                ErrorLog.ReportError("Error while writing file \"" + path + "\"", e);
                return;
            }
            finally {
                writer.Close();
            }
        }
    }
}
