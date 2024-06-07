using ApplicationUpdater.Core;
using ApplicationUpdater.Core.FileHandler;
using ApplicationUpdater.Core.Logger;
using ApplicationUpdater.Core.Updater;
using ApplicationUpdater.Core.Utils;

internal class Program
{
    private const int EXPECTED_ARGS_COUNT = 3;
    private const string HTTP_CONFIG_PATH = "http.json";

    private static async Task Main(string[] args)
    {
        string? remoteBuildPath;
        string? localAppPath;
        int systemTarget;

        // Wrong command line arguments
        if (args.Length > 0 && !ValidateArguments(args))
        {
            return;
        }
        else if (args.Length == EXPECTED_ARGS_COUNT)
        {
            remoteBuildPath = args[0];
            localAppPath = args[1];
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
                    remoteBuildPath = inputs[0];
                    localAppPath = inputs[1];
                    systemTarget = int.Parse(inputs[2]);
                    break;
                }
            }
        }

        SystemTargetEnum systemTargetEnum = (SystemTargetEnum)systemTarget;
        IFileLoader dataLoader;
        ILogger logger = new ConsoleLogger();
        switch (systemTargetEnum)
        {
            case SystemTargetEnum.LocalFileSystem:
                dataLoader = new SystemFileHandler(remoteBuildPath);
                break;
            case SystemTargetEnum.FTP:
                throw new NotImplementedException("FTP download is not implemented yet.");
            case SystemTargetEnum.HTTP:
                string httpText = File.ReadAllText(HTTP_CONFIG_PATH);
                HttpConfig? httpConfig = JsonWrapper.Deserialize<HttpConfig>(httpText);
                if (httpConfig == null)
                    throw new Exception("HTTP config is not valid.");

                dataLoader = new HttpClientFileHandler(httpConfig, logger);
                break;
            default:
                throw new NotSupportedException();
        }

        string manifestName = "manifest.json";
        AppUpdater appUpdater = new(dataLoader);
        await appUpdater.UpdateApplication(
            localAppPath, 
            manifestName);
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

            if (!int.TryParse(args[2], out int targetType) || !Enum.IsDefined(typeof(SystemTargetEnum), targetType))
            {
                string possibleEnums = new SystemTargetEnumerable().ToString();
                Console.WriteLine($"3rd argument is invalid. Should be an integer of {possibleEnums}");
                return false;
            }

            return true;
        }
    }

    private static void LogUsage()
    {
        Console.WriteLine("You must provide 2 arguments: the remote build path, the local destination path");
    }

    /// <summary>
    /// Create a template file for HTTP config.
    /// </summary>
    private static void CreateTemplateFile()
    {
        string json = JsonWrapper.Serialize(new HttpConfig() {url = "" });
        string projectDirectory = GetProjectPath().FullName;
        string filePath = Path.Combine(projectDirectory, "Template", "http.json");
        IOUtils.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, json);
    }

    private static DirectoryInfo GetProjectPath()
    {
        return Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!;
    }
}