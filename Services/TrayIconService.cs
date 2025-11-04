using System.Diagnostics;
using System.Reflection;

namespace WatchdogApp.Services
{
    /// <summary>
    /// 系统托盘服务
    /// 提供应用程序在系统托盘中的图标和上下文菜单
    /// </summary>
    public class TrayIconService
    {
        private NotifyIcon? _notifyIcon;
        private ContextMenuStrip? _contextMenu;
        private ToolStripMenuItem? _showLogMenuItem;
        private ToolStripMenuItem? _exitMenuItem;
        private ToolStripMenuItem? _aboutMenuItem;
        private EventHandler? _showLogClickHandler;
        private EventHandler? _exitClickHandler;
        private EventHandler? _aboutClickHandler;
        private EventHandler? _doubleClickHandler;

        // 监控信息
        private string _monitoredProcessName = "Application";
        private string _monitoredProcessPath = Path.Combine(AppContext.BaseDirectory, "Application.exe");
        private int _checkInterval = 5000;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TrayIconService()
        {
        }

        /// <summary>
        /// 设置监控信息
        /// </summary>
        /// <param name="processName">进程名</param>
        /// <param name="processPath">进程路径</param>
        /// <param name="interval">检查间隔</param>
        public void SetMonitoringInfo(string processName, string processPath, int interval)
        {
            _monitoredProcessName = processName;
            _monitoredProcessPath = processPath;
            _checkInterval = interval;
        }

        /// <summary>
        /// 初始化托盘图标
        /// </summary>
        public void InitializeTrayIcon()
        {
            try
            {
                // 创建托盘图标
                _notifyIcon = new NotifyIcon
                {
                    Icon = LoadCustomIcon(),
                    Text = GetTrayTooltipText(),
                    Visible = true
                };

                // 创建上下文菜单
                _contextMenu = new ContextMenuStrip
                {
                    // 确保菜单项没有多余的图标空间
                    ShowImageMargin = false
                };

                // 打开日志目录菜单项
                _showLogMenuItem = new ToolStripMenuItem("打开日志目录");
                _showLogClickHandler = (s, e) =>
                {
                    try
                    {
                        // 注意：WatchdogApp当前没有日志目录，这里可以根据实际情况修改
                        var logPath = Path.Combine(AppContext.BaseDirectory, "logs");
                        if (Directory.Exists(logPath))
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = logPath,
                                UseShellExecute = true
                            });
                        }
                        else
                        {
                            // 如果日志目录不存在，创建它
                            Directory.CreateDirectory(logPath);
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = logPath,
                                UseShellExecute = true
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        // 这里可以添加日志记录
                        MessageBoxHelper.Show("打开日志目录失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                _showLogMenuItem.Click += _showLogClickHandler;

                // 关于菜单项
                _aboutMenuItem = new ToolStripMenuItem("关于");
                _aboutClickHandler = (s, e) =>
                {
                    try
                    {
                        string aboutMessage = "看门狗应用程序\n\n" +
                                            "版本: 1.0\n" +
                                            "功能: 监控指定程序运行状态，异常时自动重启\n\n" +
                                            $"当前监控: {_monitoredProcessName}\n" +
                                            $"程序路径: {_monitoredProcessPath}\n" +
                                            $"检查间隔: {_checkInterval}毫秒";
                        MessageBoxHelper.Show(aboutMessage, "关于看门狗", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBoxHelper.Show("显示关于信息时发生错误: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                _aboutMenuItem.Click += _aboutClickHandler;

                // 退出菜单项
                _exitMenuItem = new ToolStripMenuItem("退出看门狗");
                _exitClickHandler = (s, e) =>
                {
                    Dispose();
                    Environment.Exit(0);
                };
                _exitMenuItem.Click += _exitClickHandler;

                // 添加菜单项到上下文菜单
                _contextMenu.Items.Add(_showLogMenuItem);
                _contextMenu.Items.Add(_aboutMenuItem);
                _contextMenu.Items.Add(new ToolStripSeparator());
                _contextMenu.Items.Add(_exitMenuItem);

                // 设置上下文菜单
                _notifyIcon.ContextMenuStrip = _contextMenu;

                // 双击事件
                _doubleClickHandler = (s, e) =>
                {
                    // 双击显示监控信息，确保使用自定义图标
                    if (_notifyIcon != null)
                    {
                        // 由于ShowBalloonTip方法会自动使用当前设置的_notifyIcon.Icon作为通知图标
                        // 这里直接调用即可，无需额外设置
                        _notifyIcon.ShowBalloonTip(2000, "看门狗服务", GetMonitoringStatusText(), ToolTipIcon.None);
                    }
                };
                _notifyIcon.DoubleClick += _doubleClickHandler;

                // 显示启动提示，确保使用自定义图标
                // 使用ToolTipIcon.None以确保完全使用自定义图标，不叠加系统图标
                _notifyIcon.ShowBalloonTip(2000, "看门狗服务", GetMonitoringStatusText(), ToolTipIcon.None);
            }
            catch (Exception ex)
            {
                // 这里可以添加日志记录
                MessageBoxHelper.Show("初始化托盘图标时发生错误: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 获取托盘工具提示文本
        /// </summary>
        /// <returns>工具提示文本</returns>
        private string GetTrayTooltipText()
        {
            return string.Format("看门狗服务\n监控: {0}\n检查间隔: {1}ms",
                                _monitoredProcessName,
                                _checkInterval);
        }

        /// <summary>
        /// 获取监控状态文本
        /// </summary>
        /// <returns>监控状态文本</returns>
        private string GetMonitoringStatusText()
        {
            return string.Format("服务正在运行中\n\n监控程序: {0}\n检查间隔: {1}毫秒",
                                _monitoredProcessName,
                                _checkInterval);
        }

        /// <summary>
        /// 加载自定义图标方法
        /// 从嵌入资源中加载图标，不再依赖外部文件
        /// </summary>
        /// <returns>图标对象</returns>
        private Icon LoadCustomIcon()
        {
            try
            {
                // 获取当前程序集
                Assembly assembly = Assembly.GetExecutingAssembly();
                string assemblyName = assembly.GetName().Name;

                // 尝试从嵌入资源加载ICO图标
                string icoResourceName = $"{assemblyName}.icon.watchDog_ico.ico";
                using (Stream icoStream = assembly.GetManifestResourceStream(icoResourceName))
                {
                    if (icoStream != null)
                    {
                        return new Icon(icoStream);
                    }
                    else
                    {
                        // 调试信息：显示可用的资源名称
                        string[] resources = assembly.GetManifestResourceNames();
                        string resourceList = string.Join("\n", resources);
                        MessageBoxHelper.Show($"未找到嵌入的ICO图标资源: {icoResourceName}\n\n可用资源:\n{resourceList}", "资源加载提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                // 如果找不到或无法加载ICO资源，尝试使用PNG资源
                string pngResourceName = $"{assemblyName}.icon.watchDog.png";
                using (Stream pngStream = assembly.GetManifestResourceStream(pngResourceName))
                {
                    if (pngStream != null)
                    {
                        try
                        {
                            using (Bitmap bitmap = new Bitmap(pngStream))
                            {
                                // 确保图标大小合适系统托盘
                                int iconSize = SystemInformation.SmallIconSize.Width;
                                using (Bitmap resizedBitmap = new Bitmap(bitmap, new Size(iconSize, iconSize)))
                                {
                                    return Icon.FromHandle(resizedBitmap.GetHicon());
                                }
                            }
                        }
                        catch (Exception pngEx)
                        {
                            MessageBoxHelper.Show($"加载PNG图标资源失败: {pngEx.Message}", "图标加载错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBoxHelper.Show($"未找到嵌入的PNG图标资源: {pngResourceName}", "资源加载提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                // 如果找不到自定义图标或都加载失败，返回系统默认图标
                return SystemIcons.Application;
            }
            catch (Exception ex)
            {
                // 捕获所有异常并显示
                MessageBoxHelper.Show($"加载图标时发生未预期错误: {ex.Message}", "图标加载异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return SystemIcons.Application;
            }
        }

        /// <summary>
        /// 获取自定义图标用于MessageBox
        /// 这个方法用于确保在LoadCustomIcon内部调用的MessageBox不会导致递归调用
        /// </summary>
        /// <returns>图标对象或null</returns>
        private Icon? GetMessageBoxIcon()
        {
            try
            {
                // 获取当前程序集
                Assembly assembly = Assembly.GetExecutingAssembly();
                string assemblyName = assembly.GetName().Name;
                string icoResourceName = $"{assemblyName}.icon.watchDog_ico.ico";

                using (Stream icoStream = assembly.GetManifestResourceStream(icoResourceName))
                {
                    if (icoStream != null)
                    {
                        return new Icon(icoStream);
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }



        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            // 取消事件订阅
            if (_showLogMenuItem != null && _showLogClickHandler != null)
            {
                _showLogMenuItem.Click -= _showLogClickHandler;
            }
            if (_exitMenuItem != null && _exitClickHandler != null)
            {
                _exitMenuItem.Click -= _exitClickHandler;
            }
            if (_aboutMenuItem != null && _aboutClickHandler != null)
            {
                _aboutMenuItem.Click -= _aboutClickHandler;
            }
            if (_notifyIcon != null && _doubleClickHandler != null)
            {
                _notifyIcon.DoubleClick -= _doubleClickHandler;
            }

            // 释放UI资源
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }

            if (_contextMenu != null)
            {
                _contextMenu.Dispose();
                _contextMenu = null;
            }

            // 清空引用，帮助GC回收
            _showLogMenuItem = null;
            _exitMenuItem = null;
            _aboutMenuItem = null;
            _showLogClickHandler = null;
            _exitClickHandler = null;
            _aboutClickHandler = null;
            _doubleClickHandler = null;

        }
    }
}