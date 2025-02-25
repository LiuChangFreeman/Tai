﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UI.Controls.Window
{
    public class DefaultWindow : System.Windows.Window
    {
        #region 1.依赖属性

        public static readonly DependencyProperty PageContainerProperty = DependencyProperty.Register("PageContainer", typeof(PageContainer), typeof(DefaultWindow), new PropertyMetadata(null, new PropertyChangedCallback(OnPageContainerChanged)));
        public PageContainer PageContainer { get { return (PageContainer)GetValue(PageContainerProperty); } set { SetValue(PageContainerProperty, value); } }
        #region 最小化按钮显示状态

        public static readonly DependencyProperty MinimizeVisibilityProperty = DependencyProperty.Register("MinimizeVisibility", typeof(Visibility), typeof(DefaultWindow));

        private static void OnPageContainerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var that = (DefaultWindow)d;
            if (that != null)
            {
                if (e.NewValue != null)
                {
                    that.IsCanBack = that.PageContainer.Index >= 1;

                    that.PageContainer.OnLoadPaged += (s, v) =>
                    {
                        that.IsCanBack = that.PageContainer.Index >= 1;
                    };
                }
            }
        }

        /// <summary>
        /// 最小化按钮显示状态
        /// </summary>
        public Visibility MinimizeVisibility
        {
            get { return (Visibility)GetValue(MinimizeVisibilityProperty); }
            set { SetValue(MinimizeVisibilityProperty, value); }
        }
        #endregion   

        #region 最大化按钮显示状态

        public static readonly DependencyProperty MaximizeVisibilityProperty = DependencyProperty.Register("MaximizeVisibility", typeof(Visibility), typeof(DefaultWindow));

        /// <summary>
        /// 最大化按钮显示状态
        /// </summary>
        public Visibility MaximizeVisibility
        {
            get { return (Visibility)GetValue(MaximizeVisibilityProperty); }
            set { SetValue(MaximizeVisibilityProperty, value); }
        }
        #endregion

        #region 关闭按钮显示状态

        public static readonly DependencyProperty CloseVisibilityProperty = DependencyProperty.Register("CloseVisibility", typeof(Visibility), typeof(DefaultWindow));

        /// <summary>
        /// 关闭按钮显示状态
        /// </summary>
        public Visibility CloseVisibility
        {
            get { return (Visibility)GetValue(CloseVisibilityProperty); }
            set { SetValue(CloseVisibilityProperty, value); }
        }
        #endregion

        #region 拓展内容
        public static readonly DependencyProperty ExtElementProperty = DependencyProperty.Register("ExtElement", typeof(object), typeof(DefaultWindow));
        /// <summary>
        /// 拓展内容
        /// </summary>
        public object ExtElement
        {
            get { return GetValue(ExtElementProperty); }
            set { SetValue(ExtElementProperty, value); }
        }
        #endregion



        #region 是否穿透窗口

        public static readonly DependencyProperty IsThruWindowProperty = DependencyProperty.Register("IsThruWindow", typeof(bool), typeof(DefaultWindow), new PropertyMetadata(false));

        /// <summary>
        /// 是否穿透窗口
        /// </summary>
        public bool IsThruWindow
        {
            get { return (bool)GetValue(IsThruWindowProperty); }
            set { SetValue(IsThruWindowProperty, value); }
        }
        #endregion


        public static readonly DependencyProperty IsCanBackProperty = DependencyProperty.Register("IsCanBack", typeof(bool), typeof(DefaultWindow), new PropertyMetadata(false,new PropertyChangedCallback(OnIsCanBackChanged)));

        private static void OnIsCanBackChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var that=d as DefaultWindow;
            if (that != null)
            {
                if (that.IsCanBack)
                {
                    VisualStateManager.GoToState(that, "CanBackState", true);
                }
                else
                {
                    VisualStateManager.GoToState(that, "Normal", true);
                }
            }
        }

        /// <summary>
        /// 是否可以返回
        /// </summary>
        public bool IsCanBack { get { return (bool)GetValue(IsCanBackProperty); } set { SetValue(IsCanBackProperty, value); } }
        #endregion

        private bool IsWindowClosed_ = false;
        /// <summary>
        /// 指示当前窗口是否已被关闭
        /// </summary>
        public bool IsWindowClosed { get { return IsWindowClosed_; } }

        #region 3.初始化
        public DefaultWindow()
        {
            this.DefaultStyleKey = typeof(DefaultWindow);
            //添加当前窗体到窗体集合

            //命令绑定
            this.CommandBindings.Add(new CommandBinding(DefaultWindowCommands.MinimizeWindowCommand, OnMinimizeWindowCommand));
            this.CommandBindings.Add(new CommandBinding(DefaultWindowCommands.MaximizeWindowCommand, OnMaximizeWindowCommand));
            this.CommandBindings.Add(new CommandBinding(DefaultWindowCommands.RestoreWindowCommand, OnRestoreWindowCommand));
            this.CommandBindings.Add(new CommandBinding(DefaultWindowCommands.CloseWindowCommand, OnCloseWindowCommand));
            this.CommandBindings.Add(new CommandBinding(DefaultWindowCommands.BackCommand, OnBackCommand));

            //  获取程序图标
            if (Icon == null)
            {
                Icon = ToImageSource(System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetEntryAssembly().ManifestModule.FullyQualifiedName));
            }

            Loaded += new RoutedEventHandler(window_Loaded);
        }

        private void OnBackCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (PageContainer != null)
            {
                PageContainer.Back();
                if (PageContainer.Index == 0)
                {
                    IsCanBack = false;
                }
            }
        }

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        public static ImageSource ToImageSource(Icon icon)
        {
            Bitmap bitmap = icon.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))
            {
                throw new Win32Exception();
            }

            return wpfBitmap;
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            OnSystemButtonsVisibility();
        }




        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }


        /// <summary>
        /// 根据窗口ResizeMode设置系统按钮的可视状态
        /// </summary>
        private void OnSystemButtonsVisibility()
        {
            switch (ResizeMode)
            {
                case ResizeMode.NoResize:
                    //仅显示关闭按钮
                    MinimizeVisibility = Visibility.Collapsed;
                    MaximizeVisibility = Visibility.Collapsed;
                    CloseVisibility = Visibility.Visible;
                    break;
                case ResizeMode.CanResize:
                    MinimizeVisibility = Visibility.Visible;
                    MaximizeVisibility = Visibility.Visible;
                    CloseVisibility = Visibility.Visible;
                    break;
                case ResizeMode.CanMinimize:
                    MinimizeVisibility = Visibility.Visible;
                    MaximizeVisibility = Visibility.Collapsed;
                    CloseVisibility = Visibility.Visible;
                    break;
            }
        }
        #endregion

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsWindowClosed_ = true;
        }
        #region 5.命令

        private void OnCloseWindowCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void OnRestoreWindowCommand(object sender, ExecutedRoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
        }

        private void OnMaximizeWindowCommand(object sender, ExecutedRoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        private void OnMinimizeWindowCommand(object sender, ExecutedRoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        #endregion

        #region 6.事件

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            if (IsThruWindow)
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle; uint extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
            }
        }
        #endregion

        #region win32
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int GWL_EXSTYLE = (-20);

        [DllImport("user32", EntryPoint = "SetWindowLong")]
        private static extern uint SetWindowLong(IntPtr hwnd, int nIndex, uint dwNewLong);

        [DllImport("user32", EntryPoint = "GetWindowLong")]
        private static extern uint GetWindowLong(IntPtr hwnd, int nIndex);
        #endregion
    }
}
