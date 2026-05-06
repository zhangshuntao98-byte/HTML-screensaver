using System;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.IO;

namespace MyScreensaver
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // 全局异常拦截：遇到任何崩溃直接静默退出，不再弹烦人的报错框
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (sender, e) => { Environment.Exit(0); };
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => { Environment.Exit(0); };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // 解析屏保参数
            string arg = args.Length > 0 ? args[0].ToLower().Trim().Substring(0, 2) : "";
            if (arg == "/c") {
                MessageBox.Show("请在此网页中扫码购买或输入激活码。", "屏保设置");
                return;
            } else if (arg == "/p") {
                return; 
            }
            
            Application.Run(new ScreensaverForm());
        }
    }

    public class ScreensaverForm : Form
    {
        private WebView2 webView;

        public ScreensaverForm()
        {
            // 全屏置顶无边框
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            
            // 隐藏鼠标指针
            Cursor.Hide(); 

            webView = new WebView2 { Dock = DockStyle.Fill };
            this.Controls.Add(webView);

            // 【终极修复1：使用 Shown 而不是 Load】
            // 确保窗口句柄 100% 创建完毕，防止出现"无效的窗口句柄"报错
            this.Shown += async (s, e) => {
                try 
                {
                    // 【终极修复2：强制指定具有读写权限的缓存目录】
                    // 解决 Windows 自动触发时处于 System32 目录没有写入权限导致瞬间崩溃的痛点
                    string userDataFolder = Path.Combine(Path.GetTempPath(), "MyScreensaverCache");
                    
                    // 允许视频带有声音自动播放
                    var options = new CoreWebView2EnvironmentOptions("--autoplay-policy=no-user-gesture-required");
                    var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder, options);
                    await webView.EnsureCoreWebView2Async(env);

                    // 自动寻找身边的 index.html (注意获取绝对路径的写法)
                    string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "index.html");
                    if (File.Exists(htmlPath)) {
                        webView.Source = new Uri(htmlPath);
                    } else {
                        webView.NavigateToString("<h1>找不到 index.html，请确保文件在同目录下。</h1>");
                    }

                    // 注入 JS 监听鼠标退出
                    string js = @"
                        let sx = -1, sy = -1;
                        document.addEventListener('mousemove', (e) => {
                            if (sx === -1) { sx = e.screenX; sy = e.screenY; return; }
                            if (Math.abs(e.screenX - sx) > 5 || Math.abs(e.screenY - sy) > 5) {
                                window.chrome.webview.postMessage('exit');
                            }
                        });
                        document.addEventListener('keydown', () => window.chrome.webview.postMessage('exit'));
                    ";
                    await webView.CoreWebView2.ExecuteScriptAsync(js);

                    webView.CoreWebView2.WebMessageReceived += (sender, args) => {
                        if (args.TryGetWebMessageAsString() == "exit") {
                            Environment.Exit(0);
                        }
                    };
                }
                catch (Exception)
                {
                    // 如果仍然遇到任何奇怪的环境问题，安静地退出，保护客户体验
                    Environment.Exit(0);
                }
            };
        }

        // 窗口关闭时主动释放 WebView2 资源
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try 
            {
                if (webView != null) 
                {
                    webView.Dispose();
                }
            } 
            catch { }
            
            base.OnFormClosed(e);
        }
    }
}
