using ApplicationUpdater.Core;
using ApplicationUpdater.Core.Builder;
using ApplicationUpdater.Core.FileHandler;
using ApplicationUpdater.Core.Utils;

internal class Program
{
    private const int EXPECTED_ARGS_COUNT = 3;
    private const string FTP_PARAMS_PATH = "ftp.json";
    
    private static async Task Main(string[] args)
    {
        string? sourcePath = null;
        string? destinationPath = null;
        int systemTarget = -1;

        // Wrong command line arguments
        if (args.Length > 0 && !ValidateArguments(args))
        {
            return;
        }
        else if (args.Length == EXPECTED_ARGS_COUNT)
        {
            sourcePath = args[0];
            destinationPath = args[1];
            systemTarget = int.Parse(args[2]);
        }
        // User input
        else
        {
            LogUsage();
            while (true)
            {
                string userInput = Console.ReadLine()!;
                string[] inputs = userInput.Split(' ');

                if (ValidateArguments(inputs))
                {
                    sourcePath = inputs[0];
                    destinationPath = inputs[1];
                    systemTarget = int.Parse(inputs[2]);
                    break;
                }

            }
        }

        SystemTargetEnum systemTargetEnum = (SystemTargetEnum)systemTarget;
        IFileWriter dataWriter;
        switch (systemTargetEnum)
        {
            case SystemTargetEnum.LocalFileSystem:
                dataWriter = new SystemFileHandler(sourcePath);
                break;
            case SystemTargetEnum.FTP:
                string text = File.ReadAllText(FTP_PARAMS_PATH);
                FTPConfig? ftpConfig = JsonWrapper.Deserialize<FTPConfig>(text);
                if (ftpConfig == null)
                    throw new Exception("FTP config is not valid.");

                dataWriter = new FTPFileHandler(ftpConfig);
                break;
            default:
                throw new NotSupportedException();
        }

        string manifestName = "manifest.json";
        AppBuilder appBuilder = new(dataWriter);
        appBuilder.onStatusUpdate += ConsoleWriteLine;
        await appBuilder.BuildApplication(
            sourcePath,
            destinationPath,
            manifestName);
    }

    private static void LogUsage()
    {
        Console.WriteLine("You must provide 3 arguments: the folder path, the destination path and the system target");
    }

    private static bool ValidateArguments(string[] args)
    {
        if (args.Length != EXPECTED_ARGS_COUNT)
        {
            LogUsage();
            return false;
        }
        else
        {
            bool sourcePathExists = Directory.Exists(args[0]);

            if (!sourcePathExists)
            {
                Console.WriteLine("Source directory not exists.");
                return false;
            }

            if (!int.TryParse(args[2], out int targetType) || !Enum.IsDefined(typeof(SystemTargetEnum), targetType))
            {
                string possibleEnums = new SystemTargetEnumerable().ToString();
                Console.WriteLine($"3rd argument is invalid. Should be an integer of {possibleEnums}");
                return false;
            }

            return sourcePathExists;
        }
    }

    private static void ConsoleWriteLine(string log)
    {
        Console.WriteLine(log);
    }

    /// <summary>
    /// Create a template file for FTP config.
    /// </summary>
    private static void CreateTemplateFile()
    {
        string json = JsonWrapper.Serialize(new FTPConfig("", "", ""));
        string projectDirectory = GetProjectPath().FullName;
        string filePath = Path.Combine(projectDirectory, "Template", "ftp.json");
        File.WriteAllText(filePath, json);
    }

    private static DirectoryInfo GetProjectPath()
    {
        return Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!;
    }
}