using System;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ScriptRunnerLib;


namespace FileMoveHelper
{
    public class FileMoveHelperConfig
    {
        public string TaskFolder { get; set; } = "";
        public List<string> TaskNames { get; set; } = new List<string>();
    };

    internal class Program
    {
        const string configName = "FileMoveHelper.config";
        static void Main(string[] args)
        {
            if (args.Length > 0) {
            }
            var config = new FileMoveHelperConfig();
            string exePathLoc = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string exePath = new String(Path.GetDirectoryName(exePathLoc));
            string configPath = Path.Combine(exePath, configName);

            if (!File.Exists(configPath)) {
                SaveDefaultConfig(configPath);
                Console.WriteLine("Config not found, default is saved");
                return;
            }
            string jsonString = File.ReadAllText(configPath);
            FileMoveHelperConfig? curConfig =
                JsonSerializer.Deserialize<FileMoveHelperConfig>(jsonString);
            if (curConfig == null)
            {
                Console.WriteLine("Failed to read a Config, you can rename or delete it to create a default");
                return;
            }
            var fcm = new FilesCopyMover(curConfig.TaskFolder);
            fcm.ExecuteTasks();
            Console.WriteLine($"{fcm.Message}");
            return;
        }

        private static void SaveDefaultConfig(string configPath)
        {
            var cfg = new FileMoveHelperConfig
            {
                TaskFolder  = @".\defaultTaskFoder",
                TaskNames  = new List<string>() { "defaultTask.filesmover" }
            };
            string jsonString = JsonSerializer.Serialize(cfg);
            File.WriteAllText(configPath, jsonString);

        }

    }

}