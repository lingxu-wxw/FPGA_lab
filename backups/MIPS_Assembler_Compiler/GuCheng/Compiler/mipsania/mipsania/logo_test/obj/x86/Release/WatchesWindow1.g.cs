﻿#pragma checksum "..\..\..\WatchesWindow1.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "EB8051774225AFC6AA3EA9D90DB4F3DD"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.269
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace LogoScriptIDE {
    
    
    /// <summary>
    /// WatchesWindow1
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
    public partial class WatchesWindow1 : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 10 "..\..\..\WatchesWindow1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid ui_watchesgrid;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\..\WatchesWindow1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGridTextColumn ui_nameColumn;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\WatchesWindow1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGridTextColumn ui_valueColumn;
        
        #line default
        #line hidden
        
        
        #line 17 "..\..\..\WatchesWindow1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGridTextColumn ui_remarksColumn;
        
        #line default
        #line hidden
        
        
        #line 20 "..\..\..\WatchesWindow1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ui_addLine;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\..\WatchesWindow1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ui_deleteLine;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/LogoScript;component/watcheswindow1.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\WatchesWindow1.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 4 "..\..\..\WatchesWindow1.xaml"
            ((LogoScriptIDE.WatchesWindow1)(target)).Closing += new System.ComponentModel.CancelEventHandler(this.Window_Closing);
            
            #line default
            #line hidden
            
            #line 4 "..\..\..\WatchesWindow1.xaml"
            ((LogoScriptIDE.WatchesWindow1)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Window_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.ui_watchesgrid = ((System.Windows.Controls.DataGrid)(target));
            
            #line 13 "..\..\..\WatchesWindow1.xaml"
            this.ui_watchesgrid.CellEditEnding += new System.EventHandler<System.Windows.Controls.DataGridCellEditEndingEventArgs>(this.ui_watchesgrid_CellEditEnding);
            
            #line default
            #line hidden
            
            #line 13 "..\..\..\WatchesWindow1.xaml"
            this.ui_watchesgrid.BeginningEdit += new System.EventHandler<System.Windows.Controls.DataGridBeginningEditEventArgs>(this.ui_watchesgrid_BeginningEdit);
            
            #line default
            #line hidden
            return;
            case 3:
            this.ui_nameColumn = ((System.Windows.Controls.DataGridTextColumn)(target));
            return;
            case 4:
            this.ui_valueColumn = ((System.Windows.Controls.DataGridTextColumn)(target));
            return;
            case 5:
            this.ui_remarksColumn = ((System.Windows.Controls.DataGridTextColumn)(target));
            return;
            case 6:
            this.ui_addLine = ((System.Windows.Controls.Button)(target));
            
            #line 20 "..\..\..\WatchesWindow1.xaml"
            this.ui_addLine.Click += new System.Windows.RoutedEventHandler(this.ui_addLine_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.ui_deleteLine = ((System.Windows.Controls.Button)(target));
            
            #line 21 "..\..\..\WatchesWindow1.xaml"
            this.ui_deleteLine.Click += new System.Windows.RoutedEventHandler(this.ui_deleteLine_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

