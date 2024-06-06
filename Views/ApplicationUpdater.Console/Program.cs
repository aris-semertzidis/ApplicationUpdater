using ApplicationUpdater.Core;
using ApplicationUpdater.Core.FileHandler;
using ApplicationUpdater.Core.Updater;
using ApplicationUpdater.Core.Utils;

internal class Program
{
    private const int EXPECTED_ARGS_COUNT = 3;
    private const string FTP_PARAMS_PATH = "ftp.json";

    private static DirectoryInfo GetProjectPath()
    {
        return Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!;
    }

    private static async Task Main(string[] args)
    {
        string? remoteBuildPath = null;
        string? localAppPath = null;
        int systemTarget = -1;

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
        switch (systemTargetEnum)
        {
            case SystemTargetEnum.LocalFileSystem:
                dataLoader = new SystemFileHandler();
                break;
            case SystemTargetEnum.FTP:
                string text = File.ReadAllText(FTP_PARAMS_PATH);
                FTPConfig? ftpConfig = JsonWrapper.Deserialize<FTPConfig>(text);
                if (ftpConfig == null)
                    throw new Exception("FTP config is not valid.");

                dataLoader = new FTPFileHandler(ftpConfig);
                break;
            default:
                throw new NotSupportedException();
        }

        string manifestName = "manifest.json";
        AppUpdater appUpdater = new(dataLoader);
        appUpdater.onStatusUpdate += ConsoleWriteLine;
        await appUpdater.UpdateApplication(
            remoteBuildPath,
            localAppPath, 
            manifestName);
    }

    private static void ConsoleWriteLine(string log)
    {
        Console.WriteLine(log);
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
}