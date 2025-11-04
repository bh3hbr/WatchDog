using System.Reflection;

namespace WatchdogApp.Services
{
    /// <summary> 
    /// 消息框辅助类，用于显示带有自定义图标的消息框 
    /// </summary> 
    public static class MessageBoxHelper
    {
        /// <summary> 
        /// 显示带有自定义图标的消息框 
        /// </summary> 
        /// <param name="text">要显示的文本</param> 
        /// <param name="caption">消息框标题</param> 
        /// <param name="buttons">按钮组合</param> 
        /// <param name="icon">消息框图标</param> 
        /// <returns>用户的响应</returns> 
        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            // 尝试加载自定义图标 
            Icon? customIcon = null;
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
                        customIcon = new Icon(icoStream);
                    }
                }
            }
            catch { }

            if (customIcon != null)
            {
                // 创建一个临时表单来显示带自定义图标的消息框 
                using (Form tempForm = new Form())
                {
                    tempForm.Icon = customIcon;
                    return MessageBox.Show(tempForm, text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
                }
            }
            else
            {
                // 如果没有自定义图标，显示标准消息框 
                return MessageBox.Show(text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
            }
        }

        /// <summary> 
        /// 显示带有自定义图标的信息消息框 
        /// </summary> 
        public static DialogResult ShowInformation(string text, string caption)
        {
            return Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary> 
        /// 显示带有自定义图标的错误消息框 
        /// </summary> 
        public static DialogResult ShowError(string text, string caption)
        {
            return Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary> 
        /// 显示带有自定义图标的警告消息框 
        /// </summary> 
        public static DialogResult ShowWarning(string text, string caption)
        {
            return Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}