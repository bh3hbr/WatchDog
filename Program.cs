using System.Diagnostics;

class Watchdog
{
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

            // 检查命令行参数是否正确
            if (args.Length < 2)
            {
                throw new ArgumentException("缺少参数。用法: WatchdogApp.exe <进程名> <进程路径> [<间隔时间>] \n");
            }

            // 获取命令行参数
            string processName = args[0];
            string processPath = args[1];
            int checkInterval = 5000; // 默认间隔时间，5000毫秒

            if (args.Length >= 3 && !int.TryParse(args[2], out checkInterval))
            {
                Console.WriteLine("间隔时间格式错误，使用默认值: 5000 毫秒。");
            }

            Console.WriteLine($"看门狗启动，监控进程: {processName}，路径: {processPath}，间隔: {checkInterval} 毫秒");

            while (true)
            {
                try
                {
                    // 获取自助机进程
                    Process[] processes = Process.GetProcessesByName(processName);

                    // 如果进程不存在，则启动
                    if (processes.Length == 0)
                    {
                        Console.WriteLine($"{processName} 未运行，尝试启动...");
                        StartProcess(processPath);
                    }
                    else
                    {
                        Console.WriteLine($"{processName} 正在运行...");
                    }

                    // 扫描频率
                    Thread.Sleep(checkInterval);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"监控过程中发生异常: {ex.Message}");
                    // 等待 5 秒后再继续检查，以防频繁抛出异常导致过载
                    Thread.Sleep(5000);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"程序发生异常: {ex.Message}");
        }
        finally
        {
            // 保证程序不会直接退出，等待用户按键
            WaitForExit();
        }
    }

    // 启动目标进程
    static void StartProcess(string processPath)
    {
        try
        {
            Process.Start(processPath);
            Console.WriteLine("目标程序已成功启动。");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"启动目标程序时发生异常: {ex.Message}");
        }
    }

    static void StopWatchdog()
    {
        Console.WriteLine("停止看门狗程序。");
        Environment.Exit(0);
    }

    // 等待用户按键，防止程序退出
    static void WaitForExit()
    {
        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }
}
