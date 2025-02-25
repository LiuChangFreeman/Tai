﻿using Core.Servicers.Instances;
using Core.Servicers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Resources;
using UI.Controls.Window;
using UI.ViewModels;
using UI.Views;

namespace UI
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider serviceProvider;
        private System.Threading.Mutex mutex;

        //  状态栏图标
        private System.Windows.Forms.NotifyIcon statusBarIcon;

        //  保活窗口
        private HideWindow keepaliveWindow;

        //  状态栏图标菜单
        private ContextMenu contextMenu;

        private DefaultWindow mainWindow;
        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            serviceProvider = serviceCollection.BuildServiceProvider();
        }

        #region 获取当前程序是否已运行
        /// <summary>
        /// 获取当前程序是否已运行
        /// </summary>
        private bool IsRuned()
        {
            bool ret;
            mutex = new System.Threading.Mutex(true, System.Reflection.Assembly.GetEntryAssembly().ManifestModule.Name, out ret);
            if (!ret)
            {
#if !DEBUG
                return true;

#endif
            }
            return false;
        }
        #endregion

        #region 状态栏图标
        /// <summary>
        /// 初始化状态栏图标
        /// </summary>
        private void InitStatusBarIcon()
        {
            CreateStatusBarIconMenu();

            statusBarIcon = new System.Windows.Forms.NotifyIcon();
            statusBarIcon.Text = "Tai!";
            Stream iconStream = GetResourceStream(new Uri("pack://application:,,,/Tai;component/Resources/Icons/tai32.ico")).Stream;
            statusBarIcon.Icon = new Icon(iconStream);
            //statusBarIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetEntryAssembly().ManifestModule.Name);

            statusBarIcon.Visible = true;
            statusBarIcon.MouseClick += NotifyIcon_MouseClick;
            statusBarIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
        }
        private void NotifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                //右键单击弹出托盘菜单
                contextMenu.IsOpen = true;
                //激活主窗口，用于处理关闭托盘菜单
                App.Current.MainWindow.Activate();
            }
        }
        private void NotifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                //双击托盘图标显示主窗口
                ShowMainWindow();
            }
        }

        /// <summary>
        /// 创建状态栏图标菜单
        /// </summary>
        private void CreateStatusBarIconMenu()
        {
            contextMenu = new ContextMenu();
            App.Current.Deactivated += (e, c) =>
            {
                contextMenu.IsOpen = false;
            };
            var mainWindowMenuItem = new MenuItem();
            mainWindowMenuItem.Header = "主界面";
            mainWindowMenuItem.Click += MainWindowMenuItem_Click;
            var exitMenuItem = new MenuItem();
            exitMenuItem.Header = "退出";
            exitMenuItem.Click += ExitMenuItem_Click; ;

            contextMenu.Items.Add(mainWindowMenuItem);
            contextMenu.Items.Add(exitMenuItem);

        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Shutdown();
        }

        private void MainWindowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowMainWindow();
        }
        #endregion

        private void ConfigureServices(IServiceCollection services)
        {
            //  核心服务
            services.AddSingleton<IObserver, Observer>();
            services.AddSingleton<IMain, Main>();
            services.AddSingleton<IData, Data>();
            services.AddSingleton<ISleepdiscover, Sleepdiscover>();
            services.AddSingleton<IAppConfig, AppConfig>();
            services.AddSingleton<IDateObserver, DateObserver>();

            //  主窗口
            services.AddSingleton<MainViewModel>();
            services.AddTransient<MainWindow>();

            //  首页
            services.AddTransient<IndexPage>();
            services.AddTransient<IndexPageVM>();

            //  数据页
            services.AddTransient<DataPage>();
            services.AddTransient<DataPageVM>();

            //  设置页
            services.AddTransient<SettingPage>();
            services.AddTransient<SettingPageVM>();

            //  详情页
            services.AddTransient<DetailPage>();
            services.AddTransient<DetailPageVM>();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            //  阻止多开进程
            if (IsRuned())
            {
                Shutdown();
            }

            var main = serviceProvider.GetService<IMain>();
            main.Run();

            //  创建保活窗口
            keepaliveWindow = new HideWindow();
            keepaliveWindow.Title = "Tai Keepalive";
            keepaliveWindow.ShowInTaskbar = false;
            keepaliveWindow.WindowStyle = WindowStyle.None;
            keepaliveWindow.OverridesDefaultStyle = true;
            keepaliveWindow.Show();

            InitStatusBarIcon();
        }

        private void ShowMainWindow()
        {
            var dataContext = serviceProvider.GetService<MainViewModel>();


            if (mainWindow == null || mainWindow.IsWindowClosed)
            {
                mainWindow = serviceProvider.GetService<MainWindow>();

                mainWindow.DataContext = dataContext;
                dataContext.SelectGroup(-1);
                dataContext.Uri = nameof(IndexPage);
            }

            mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Show();
            mainWindow.Activate();
        }
    }
}
