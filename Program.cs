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
            this.Cursor = Cursors.Hide;

            webView = new WebView2 { Dock = DockStyle.Fill };
            this.Controls.Add(webView);

            this.Load += async (s, e) => {
                // 🚀 核心魔法：允许视频带有声音自动播放！
                var options = new CoreWebView2EnvironmentOptions("--autoplay-policy=no-user-gesture-required");
                var env = await CoreWebView2Environment.CreateAsync(null, null, options);
                await webView.EnsureCoreWebView2Async(env);

                // 自动寻找身边的 index.html
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
                    if (args.TryGetAsString() == "exit") Application.Exit();
                };
            };
        }
    }
}
