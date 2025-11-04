using System.Diagnostics;
using WatchdogApp.Services;

class Watchdog
{
    private static string processName = "Application";
    private static string processPath = Path.Combine(AppContext.BaseDirectory, "Application.exe");
    private static int checkInterval = 5000; // 默认间隔时间，5000毫秒
    private static Thread? watchdogThread;
    private static bool isRunning = true;
    private static TrayIconService? trayIconService;

    [STAThread]
    static void Main(string[] args)
    {
        try
        {
            // 接受停止命令: 特殊用法使用；无法二次直接启动使用，因为操作的非同一个进程
            if (args.Length == 1 && args[0] == "STOP")
            {
                // 真正的结束进程在发卡机端处理，只有更新时才会杀死看门狗
                StopWatchdog();
                return;
            }

            // 检查命令行参数并设置监控目标
            if (args.Length >= 2)
            {
                processName = args[0];
                processPath = args[1];

                if (args.Length >= 3 && !int.TryParse(args[2], out checkInterval))
                {
                    LogMessage("间隔时间格式错误，使用默认值: 5000 毫秒。");
                    checkInterval = 5000;
                }
            }
            else
            {
                // 没有参数或参数不足时显示提示信息并退出
                string helpMessage = "看门狗必须通过参数打开。\n\n" +
                                    "用法1: WatchdogApp.exe <进程名> <进程路径> [<间隔时间>]\n" +
                                    "用法2: WatchdogApp.exe STOP (停止看门狗)";

                LogMessage(helpMessage);

                // 使用封装的MessageBoxHelper显示带自定义图标的消息框
                MessageBoxHelper.Show(helpMessage, "看门狗提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 用户确认后退出程序
                // return;
            }

            // 记录启动信息
            LogMessage($"看门狗启动，监控进程: {processName}，路径: {processPath}，间隔: {checkInterval} 毫秒");

            // 初始化Windows Forms应用程序
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 初始化托盘图标服务
            trayIconService = new TrayIconService();
            // 设置监控信息
            trayIconService.SetMonitoringInfo(processName, processPath, checkInterval);
            // 初始化托盘图标
            trayIconService.InitializeTrayIcon();

            // 创建并启动看门狗线程
            watchdogThread = new Thread(WatchdogLoop)
            {
                IsBackground = true,
                Name = "WatchdogThread"
            };
            watchdogThread.Start();

            // 运行应用程序消息循环
            Application.Run();
        }
        catch (Exception ex)
        {
            // 记录异常
            LogException(ex);
        }
        finally
        {
            // 清理资源
            Cleanup();
        }
    }

    // 看门狗主循环
    private static void WatchdogLoop()
    {
        while (isRunning)
        {
            try
            {
                // 获取监控进程
                Process[] processes = Process.GetProcessesByName(processName);

                // 如果进程不存在，则启动
                if (processes.Length == 0)
                {
                    LogMessage($"{processName} 未运行，尝试启动...");
                    StartProcess(processPath);
                }
                else
                {
                    LogMessage($"{processName} 正在运行...");
                }

                // 扫描频率
                Thread.Sleep(checkInterval);
            }
            catch (Exception ex)
            {
                LogException(ex);
                // 等待 5 秒后再继续检查，以防频繁抛出异常导致过载
                Thread.Sleep(5000);
            }
        }
    }

    // 启动目标进程
    private static void StartProcess(string processPath)
    {
        try
        {
            // 检查文件是否存在
            if (!File.Exists(processPath))
            {
                LogMessage("目标程序文件不存在: " + processPath);
                return;
            }

            Process.Start(processPath);
            LogMessage("目标程序已成功启动。");
        }
        catch (Exception ex)
        {
            LogException(ex, "启动目标程序时发生异常: ");
        }
    }

    // 停止看门狗
    private static void StopWatchdog()
    {
        LogMessage("停止看门狗程序。");
        Cleanup();
        Environment.Exit(0);
    }

    // 清理资源
    private static void Cleanup()
    {
        isRunning = false;
        if (watchdogThread != null && watchdogThread.IsAlive)
        {
            watchdogThread.Join(2000); // 等待线程结束，最多2秒
        }
        trayIconService?.Dispose();
    }

    // 记录消息
    private static void LogMessage(string message)
    {
        // 创建日志目录
        string logDir = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(logDir);

        // 写入日志文件
        string logFile = Path.Combine(logDir, $"watchdog_{DateTime.Now:yyyy-MM-dd}.log");
        string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

        try
        {
            File.AppendAllText(logFile, logEntry + Environment.NewLine);
        }
        catch { }
    }

    // 记录异常
    private static void LogException(Exception ex, string prefix = "")
    {
        LogMessage(prefix + ex.Message);
    }
}
