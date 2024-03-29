// See https://aka.ms/new-console-template for more information

using ClockifyImport.ConsoleApp;
using System.Reflection;

string configPrefix = Core.GetConfigValue("Configuration");

bool cont = true;
while (cont)
{
    Console.Clear();
    Console.WriteLine("Welcome to the Clockify Data Importer!");
    Console.WriteLine("Version " + Assembly.GetExecutingAssembly().GetName().Version);
    Console.WriteLine();

    string sourceFolder = Core.GetConfigValue("FilePath", configPrefix).TrimEnd('/').TrimEnd('\\');
    var files = Directory.GetFiles(sourceFolder);

    Console.WriteLine("Source Folder: " + sourceFolder + " (" + files.Length + " file" + (files.Length == 1 ? "" : "s") + ")");
    Console.WriteLine("0. Change source folder");
    int fnpi = 1;
    Dictionary<string, string> fileNumPaths = new Dictionary<string, string>();
    foreach (var file in files)
    {
        Console.WriteLine(fnpi + ". " + file.Replace(sourceFolder + "\\", "./"));
        fileNumPaths.Add((fnpi++).ToString(), file);

    }
    Console.WriteLine("Select an option from above or press [Enter] to continue");
    string fileResp = Console.ReadLine()!;
    if (string.IsNullOrWhiteSpace(fileResp))
    {
        // Do nothing
    }
    else if (fileResp == "0")
    {
        Console.WriteLine("New source path:");
        sourceFolder = Console.ReadLine()!;
    }
    else
    {
        try
        {
            string filePath = fileNumPaths[fileResp];
            if (File.Exists(filePath))
            {
                ClockifyImporter.ProcessFile(sourceFolder, filePath, configPrefix);
            }
            else
            {
                Core.UserMessage("Unable to find file \"" + filePath + "\". Please try again.");
            }
        }
        catch (Exception ex)
        {
            Core.UserMessage(ex.Message);
        }
    }

    Console.WriteLine("Press [Enter] to continue or [Q]+[Enter] to quit the application");
    string resp = Console.ReadLine()!; // The `!` here tells Visual Studio to treat Console.ReadLine() as non-nullable (Used to suppress warnings)
    if (resp.Equals("q", StringComparison.OrdinalIgnoreCase))
    {
        cont = false;
    }
}
