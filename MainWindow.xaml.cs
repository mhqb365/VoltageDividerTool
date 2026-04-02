using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VoltageDividerTool
{
    public partial class MainWindow : Window
    {
        private enum AppMode { Divider, Parallel, Regulator, Vnode, ColorCode, SmdCode }
        private AppMode currentMode = AppMode.Divider;
        private string currentLang = "vi";
        private string configPath = "config.txt";
        
        private List<TextBox> dividerResistors = new List<TextBox>();
        private List<TextBox> parallelResistors = new List<TextBox>();
        private List<TextBox> voutOutputs = new List<TextBox>();

        private class ResistorColor
        {
            public string NameEn { get; set; }
            public string NameVi { get; set; }
            public Color Hex { get; set; }
            public Color FgFormat { get; set; }
            public int? Value { get; set; }
            public double? Multiplier { get; set; }
            public double? Tolerance { get; set; }
            public int? TempCoef { get; set; }
        }

        private List<ResistorColor> colorBands = new List<ResistorColor>
        {
            new ResistorColor { NameEn = "Black", NameVi = "Đen", Hex = Color.FromRgb(0, 0, 0), FgFormat = Colors.White, Value = 0, Multiplier = 1, TempCoef = 250 },
            new ResistorColor { NameEn = "Brown", NameVi = "Nâu", Hex = Color.FromRgb(139, 69, 19), FgFormat = Colors.White, Value = 1, Multiplier = 10, Tolerance = 1, TempCoef = 100 },
            new ResistorColor { NameEn = "Red", NameVi = "Đỏ", Hex = Color.FromRgb(255, 0, 0), FgFormat = Colors.White, Value = 2, Multiplier = 100, Tolerance = 2, TempCoef = 50 },
            new ResistorColor { NameEn = "Orange", NameVi = "Cam", Hex = Color.FromRgb(255, 165, 0), FgFormat = Colors.Black, Value = 3, Multiplier = 1000, TempCoef = 15 },
            new ResistorColor { NameEn = "Yellow", NameVi = "Vàng", Hex = Color.FromRgb(255, 255, 0), FgFormat = Colors.Black, Value = 4, Multiplier = 10000, TempCoef = 25 },
            new ResistorColor { NameEn = "Green", NameVi = "Lục", Hex = Color.FromRgb(0, 128, 0), FgFormat = Colors.White, Value = 5, Multiplier = 100000, Tolerance = 0.5, TempCoef = 20 },
            new ResistorColor { NameEn = "Blue", NameVi = "Lam", Hex = Color.FromRgb(0, 0, 255), FgFormat = Colors.White, Value = 6, Multiplier = 1000000, Tolerance = 0.25, TempCoef = 10 },
            new ResistorColor { NameEn = "Violet", NameVi = "Tím", Hex = Color.FromRgb(238, 130, 238), FgFormat = Colors.Black, Value = 7, Multiplier = 10000000, Tolerance = 0.1, TempCoef = 5 },
            new ResistorColor { NameEn = "Gray", NameVi = "Xám", Hex = Color.FromRgb(128, 128, 128), FgFormat = Colors.White, Value = 8, Multiplier = 100000000, Tolerance = 0.05, TempCoef = 1 },
            new ResistorColor { NameEn = "White", NameVi = "Trắng", Hex = Color.FromRgb(255, 255, 255), FgFormat = Colors.Black, Value = 9, Multiplier = 1000000000 },
            new ResistorColor { NameEn = "Gold", NameVi = "Vàng kim", Hex = Color.FromRgb(255, 215, 0), FgFormat = Colors.Black, Multiplier = 0.1, Tolerance = 5 },
            new ResistorColor { NameEn = "Silver", NameVi = "Bạc", Hex = Color.FromRgb(192, 192, 192), FgFormat = Colors.Black, Multiplier = 0.01, Tolerance = 10 }
        };

        private int[] selectedColorIndices = new int[6] { 1, 0, 2, 10, 1, 1 }; // Default all valid indices to prevent crash

        public MainWindow()
        {
            InitializeComponent();
            LoadConfig();
            
            // Add default 2 resistors for each mode
            AddResistorRow(AppMode.Divider);
            AddResistorRow(AppMode.Divider);
            AddResistorRow(AppMode.Parallel);
            AddResistorRow(AppMode.Parallel);
            
            ApplyLanguage();
            InitializeColorBandsUI();
            UpdateAll();
        }

        private void LoadConfig()
        {
            try
            {
                if (File.Exists(configPath))
                {
                    string content = File.ReadAllText(configPath);
                    currentLang = content.Contains("Language=en") ? "en" : "vi";
                }
            }
            catch { }
        }

        private void SaveConfig()
        {
            try { File.WriteAllText(configPath, $"Language={currentLang}"); } catch { }
        }

        private void ApplyLanguage()
        {
            if (MainHeader == null) return;

            // First COLLAPSE all panels to avoid overlapping
            if (DividerPanel != null) DividerPanel.Visibility = Visibility.Collapsed;
            if (ParallelPanel != null) ParallelPanel.Visibility = Visibility.Collapsed;
            if (RegulatorPanel != null) RegulatorPanel.Visibility = Visibility.Collapsed;
            if (VnodePanel != null) VnodePanel.Visibility = Visibility.Collapsed;
            if (ColorCodePanel != null) ColorCodePanel.Visibility = Visibility.Collapsed;
            if (SmdPanel != null) SmdPanel.Visibility = Visibility.Collapsed;
            if (VinRow != null) VinRow.Visibility = Visibility.Collapsed;

            // Now show only the relevant one based on currentMode
            if (currentMode == AppMode.Divider)
            {
                MainHeader.Text = currentLang == "en" ? "VOLTAGE DIVIDER" : "CẦU PHÂN ÁP";
                DividerPanel.Visibility = Visibility.Visible;
                VinRow.Visibility = Visibility.Visible;
            }
            else if (currentMode == AppMode.Parallel)
            {
                MainHeader.Text = currentLang == "en" ? "PARALLEL RESISTANCE" : "TRỞ SONG SONG";
                ParallelPanel.Visibility = Visibility.Visible;
            }
            else if (currentMode == AppMode.Regulator)
            {
                MainHeader.Text = currentLang == "en" ? "IC REGULATOR" : "IC ỔN ÁP";
                RegulatorPanel.Visibility = Visibility.Visible;
            }
            else if (currentMode == AppMode.Vnode)
            {
                MainHeader.Text = currentLang == "en" ? "VOLTAGE NODE" : "ĐIỆN ÁP NÚT";
                VnodePanel.Visibility = Visibility.Visible;
            }
            else if (currentMode == AppMode.ColorCode)
            {
                MainHeader.Text = currentLang == "en" ? "RESISTOR COLOR CODE" : "MÀU ĐIỆN TRỞ";
                ColorCodePanel.Visibility = Visibility.Visible;
            }
            else // SmdCode
            {
                MainHeader.Text = currentLang == "en" ? "SMD RESISTOR CODE" : "MÃ ĐIỆN TRỞ DÁN";
                SmdPanel.Visibility = Visibility.Visible;
                
                if (TxtSmdHeader != null) TxtSmdHeader.Text = currentLang == "en" ? "ENTER SMD CODE (3 OR 4 DIGITS)" : "NHẬP MÃ SMD (3 HOẶC 4 SỐ)";
                if (TxtSmdValueLabel != null) TxtSmdValueLabel.Text = currentLang == "en" ? "RESISTANCE VALUE" : "GIÁ TRỊ ĐIỆN TRỞ";
            }

            if (Rb4Band != null) Rb4Band.Content = currentLang == "en" ? "4 Bands" : "4 Vòng";
            if (Rb5Band != null) Rb5Band.Content = currentLang == "en" ? "5 Bands" : "5 Vòng";
            if (Rb6Band != null) Rb6Band.Content = currentLang == "en" ? "6 Bands" : "6 Vòng";

            ResetBtn.Content = currentLang == "en" ? "↺  RESET" : "↺  ĐẶT LẠI";
            
            if (TxtAbout != null)
                TxtAbout.Text = currentLang == "en" ? "About" : "Thông tin";

            BtnLangEn.Opacity = currentLang == "en" ? 1.0 : 0.4;
            BtnLangVi.Opacity = currentLang == "vi" ? 1.0 : 0.4;

            UpdateLabels(dividerResistors, "R");
            UpdateLabels(parallelResistors, "R");

            for (int i = 0; i < voutOutputs.Count; i++)
            {
                var inputGrid = voutOutputs[i].Parent as Grid;
                var rowGrid = inputGrid?.Parent as Grid;
                var btn = rowGrid?.Children[0] as Button;
                if (btn != null) btn.Content = $"Vout{i + 1}";
            }
        }

        private void UpdateLabels(List<TextBox> inputs, string prefix)
        {
            for (int i = 0; i < inputs.Count; i++)
            {
                var grid = inputs[i].Parent as Grid;
                var rowGrid = grid?.Parent as Grid;
                var btn = rowGrid?.Children[0] as Button;
                if (btn != null) btn.Content = $"{prefix}{i + 1}";
            }
        }

        private void SwitchLang_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
                currentLang = btn.Tag as string;
                SaveConfig();
                ApplyLanguage();
                if (currentMode == AppMode.ColorCode) InitializeColorBandsUI();
            }
        }

        private void BtnGitHub_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://github.com/mhqb365/VoltageDividerTool",
                    UseShellExecute = true
                });
            }
            catch { }
        }

        private void ChangeMode_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            string modeStr = btn?.Tag?.ToString();
            if (modeStr == null) return;

            if (modeStr == "Divider") currentMode = AppMode.Divider;
            else if (modeStr == "Parallel") currentMode = AppMode.Parallel;
            else if (modeStr == "Regulator") currentMode = AppMode.Regulator;
            else if (modeStr == "ColorCode") currentMode = AppMode.ColorCode;
            else if (modeStr == "SmdCode") currentMode = AppMode.SmdCode;
            else currentMode = AppMode.Vnode;

            // Update Sidebar UI
            var secondaryBrush = (Brush)FindResource("SecondaryTextBrush");
            
            bool isDivider = currentMode == AppMode.Divider;
            BorderModeDivider.Background = isDivider ? new SolidColorBrush(Color.FromRgb(28, 36, 47)) : Brushes.Transparent;
            ((TextBlock)((StackPanel)BorderModeDivider.Child).Children[0]).Foreground = isDivider ? Brushes.White : secondaryBrush;
            ((TextBlock)((StackPanel)BorderModeDivider.Child).Children[1]).Foreground = isDivider ? Brushes.White : secondaryBrush;

            bool isParallel = currentMode == AppMode.Parallel;
            BorderModeParallel.Background = isParallel ? new SolidColorBrush(Color.FromRgb(28, 36, 47)) : Brushes.Transparent;
            ((TextBlock)((StackPanel)BorderModeParallel.Child).Children[0]).Foreground = isParallel ? Brushes.White : secondaryBrush;
            ((TextBlock)((StackPanel)BorderModeParallel.Child).Children[1]).Foreground = isParallel ? Brushes.White : secondaryBrush;

            bool isRegulator = currentMode == AppMode.Regulator;
            BorderModeRegulator.Background = isRegulator ? new SolidColorBrush(Color.FromRgb(28, 36, 47)) : Brushes.Transparent;
            ((TextBlock)((StackPanel)BorderModeRegulator.Child).Children[0]).Foreground = isRegulator ? Brushes.White : secondaryBrush;
            ((TextBlock)((StackPanel)BorderModeRegulator.Child).Children[1]).Foreground = isRegulator ? Brushes.White : secondaryBrush;

            bool isVnode = currentMode == AppMode.Vnode;
            if (BorderModeVnode != null)
            {
                BorderModeVnode.Background = isVnode ? new SolidColorBrush(Color.FromRgb(28, 36, 47)) : Brushes.Transparent;
                ((TextBlock)((StackPanel)BorderModeVnode.Child).Children[0]).Foreground = isVnode ? Brushes.White : secondaryBrush;
                ((TextBlock)((StackPanel)BorderModeVnode.Child).Children[1]).Foreground = isVnode ? Brushes.White : secondaryBrush;
            }

            bool isColorMode = currentMode == AppMode.ColorCode;
            if (BorderModeColor != null)
            {
                BorderModeColor.Background = isColorMode ? new SolidColorBrush(Color.FromRgb(28, 36, 47)) : Brushes.Transparent;
                ((TextBlock)((StackPanel)BorderModeColor.Child).Children[0]).Foreground = isColorMode ? Brushes.White : secondaryBrush;
                ((TextBlock)((StackPanel)BorderModeColor.Child).Children[1]).Foreground = isColorMode ? Brushes.White : secondaryBrush;
            }

            bool isSmdMode = currentMode == AppMode.SmdCode;
            if (BorderModeSmd != null)
            {
                BorderModeSmd.Background = isSmdMode ? new SolidColorBrush(Color.FromRgb(28, 36, 47)) : Brushes.Transparent;
                ((TextBlock)((StackPanel)BorderModeSmd.Child).Children[0]).Foreground = isSmdMode ? Brushes.White : secondaryBrush;
                ((TextBlock)((StackPanel)BorderModeSmd.Child).Children[1]).Foreground = isSmdMode ? Brushes.White : secondaryBrush;
            }

            ApplyLanguage();
            UpdateAll();
        }

        private void AddResistorRow(AppMode mode)
        {
            var container = mode == AppMode.Divider ? DividerResistorsContainer : ParallelResistorsContainer;
            var inputs = mode == AppMode.Divider ? dividerResistors : parallelResistors;
            int index = inputs.Count + 1;
            
            Grid rowGrid = new Grid { Margin = new Thickness(0, 0, 0, 12) };
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });

            Button btnCalc = new Button { 
                Content = $"R{index}", Style = (Style)FindResource("CalcButton"), Width = 75, HorizontalAlignment = HorizontalAlignment.Left
            };
            btnCalc.Click += BtnCalc_Click;

            Grid inputGrid = new Grid();
            TextBox txt = new TextBox { Style = (Style)FindResource("ModernTextBox"), Padding = new Thickness(10, 8, 30, 8) };
            txt.TextChanged += (s, e) => UpdateAll();
            
            Button btnClear = new Button {
                Content = "✕", Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68)),
                Background = Brushes.Transparent, BorderThickness = new Thickness(0), FontSize = 16,
                Cursor = System.Windows.Input.Cursors.Hand, HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0), Visibility = Visibility.Collapsed
            };
            btnClear.Click += (s, e) => txt.Text = "";
            txt.TextChanged += (s, e) => btnClear.Visibility = string.IsNullOrEmpty(txt.Text) ? Visibility.Collapsed : Visibility.Visible;
            inputGrid.Children.Add(txt);
            inputGrid.Children.Add(btnClear);

            Button btnRemove = new Button {
                Content = "", FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Foreground = new SolidColorBrush(Color.FromRgb(156, 173, 193)), Background = Brushes.Transparent,
                BorderThickness = new Thickness(0), FontSize = 14, Cursor = System.Windows.Input.Cursors.Hand,
                Tag = new object[] { mode, rowGrid }
            };
            btnRemove.Click += BtnRemoveResistor_Click;

            Grid.SetColumn(btnCalc, 0); Grid.SetColumn(inputGrid, 1); Grid.SetColumn(btnRemove, 2);
            rowGrid.Children.Add(btnCalc); rowGrid.Children.Add(inputGrid); rowGrid.Children.Add(btnRemove);

            container.Children.Add(rowGrid);
            inputs.Add(txt);

            if (mode == AppMode.Divider && inputs.Count > 1) AddVoutRow(inputs.Count - 1);
            UpdateRemoveButtons(mode);
        }

        private void AddVoutRow(int index)
        {
            Grid voutGrid = new Grid { Margin = new Thickness(0, 0, 0, 12) };
            voutGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            voutGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Button btnVout = new Button { Content = $"Vout{index}", Style = (Style)FindResource("CalcButton"), Width = 75, HorizontalAlignment = HorizontalAlignment.Left };
            btnVout.Click += BtnCalc_Click;

            Grid inputGrid = new Grid();
            TextBox txtVout = new TextBox { Style = (Style)FindResource("ModernTextBox"), Padding = new Thickness(10, 8, 30, 8), Tag = "Vout" };
            txtVout.TextChanged += (s, e) => UpdateAll();

            Button btnClear = new Button {
                Content = "✕", Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68)),
                Background = Brushes.Transparent, BorderThickness = new Thickness(0), FontSize = 16,
                Cursor = System.Windows.Input.Cursors.Hand, HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0), Visibility = Visibility.Collapsed
            };
            btnClear.Click += (s, e) => txtVout.Text = "";
            txtVout.TextChanged += (s, e) => btnClear.Visibility = string.IsNullOrEmpty(txtVout.Text) ? Visibility.Collapsed : Visibility.Visible;
            inputGrid.Children.Add(txtVout);
            inputGrid.Children.Add(btnClear);

            Grid.SetColumn(btnVout, 0); Grid.SetColumn(inputGrid, 1);
            voutGrid.Children.Add(btnVout); voutGrid.Children.Add(inputGrid);

            DividerResultsContainer.Children.Add(voutGrid);
            voutOutputs.Add(txtVout);
        }

        private void BtnAddResistor_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            AppMode mode = btn?.Tag?.ToString() == "Divider" ? AppMode.Divider : AppMode.Parallel;
            AddResistorRow(mode);
            ApplyLanguage();
            UpdateAll();
        }

        private void BtnRemoveResistor_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var data = btn?.Tag as object[];
            if (data == null) return;

            AppMode mode = (AppMode)data[0];
            Grid rowGrid = (Grid)data[1];
            var inputs = mode == AppMode.Divider ? dividerResistors : parallelResistors;
            var container = mode == AppMode.Divider ? DividerResistorsContainer : ParallelResistorsContainer;

            if (inputs.Count <= 2) return;

            int index = container.Children.IndexOf(rowGrid);
            container.Children.Remove(rowGrid);
            inputs.RemoveAt(index);

            if (mode == AppMode.Divider && voutOutputs.Count > 0)
            {
                var lastVout = voutOutputs.Last();
                var inputGrid = lastVout.Parent as Grid;
                var voutRow = inputGrid?.Parent as Grid;
                if (voutRow != null) DividerResultsContainer.Children.Remove(voutRow);
                voutOutputs.RemoveAt(voutOutputs.Count - 1);
            }

            UpdateRemoveButtons(mode);
            ApplyLanguage();
            UpdateAll();
        }

        private void UpdateRemoveButtons(AppMode mode)
        {
            var container = mode == AppMode.Divider ? DividerResistorsContainer : ParallelResistorsContainer;
            var inputs = mode == AppMode.Divider ? dividerResistors : parallelResistors;
            foreach (Grid row in container.Children)
            {
                var removeBtn = row.Children[2] as Button;
                if (removeBtn != null) removeBtn.Visibility = inputs.Count > 2 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            InputVin.Text = "";
            InputRTotal.Text = "";
            InputRegR1.Text = "";
            InputRegR2.Text = "";
            InputRegVout.Text = "";
            InputVref.Text = "1.25";
            if (InputSmdCode != null) InputSmdCode.Text = "";
            
            if (InputVnodeV1 != null) { InputVnodeV1.Text = ""; InputVnodeV2.Text = ""; InputVnodeR1.Text = ""; InputVnodeR2.Text = ""; InputVnodeR3.Text = ""; InputVnodeV3.Text = ""; }

            var inputs = currentMode == AppMode.Divider ? dividerResistors : parallelResistors;
            var container = currentMode == AppMode.Divider ? DividerResistorsContainer : ParallelResistorsContainer;

            foreach (var r in dividerResistors) r.Text = "";
            foreach (var r in parallelResistors) r.Text = "";
            foreach (var v in voutOutputs) v.Text = "";

            while (inputs.Count > 2)
            {
                int lastIndex = inputs.Count - 1;
                var row = container.Children[lastIndex] as Grid;
                container.Children.Remove(row);
                inputs.RemoveAt(lastIndex);

                if (currentMode == AppMode.Divider && voutOutputs.Count > 0)
                {
                    var lastVout = voutOutputs.Last();
                    var inputGrid = lastVout.Parent as Grid;
                    var voutRow = inputGrid?.Parent as Grid;
                    if (voutRow != null) DividerResultsContainer.Children.Remove(voutRow);
                    voutOutputs.RemoveAt(voutOutputs.Count - 1);
                }
            }

            UpdateRemoveButtons(currentMode);
            ApplyLanguage();
            UpdateAll();
        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = sender as TextBox;
            if (txt != null)
            {
                var grid = txt.Parent as Grid;
                if (grid != null && grid.Children.Count > 1)
                {
                    var btnClear = grid.Children[1] as Button;
                    if (btnClear != null && btnClear.Content?.ToString() == "✕")
                    {
                        btnClear.Visibility = string.IsNullOrEmpty(txt.Text) ? Visibility.Collapsed : Visibility.Visible;
                    }
                }
            }
            UpdateAll();
        }

        private void UpdateAll()
        {
            if (CircuitCanvas == null) return;
            RenderDiagram();
        }

        private void RenderDiagram()
        {
            if (CircuitCanvas == null) return;
            CircuitCanvas.Children.Clear();
            if (currentMode == AppMode.Divider) RenderDividerDiagram();
            else if (currentMode == AppMode.Parallel) RenderParallelDiagram();
            else if (currentMode == AppMode.Regulator) RenderRegulatorDiagram();
            else if (currentMode == AppMode.Vnode) RenderVnodeDiagram();
            else if (currentMode == AppMode.ColorCode) RenderColorCodeDiagram();
            else if (currentMode == AppMode.SmdCode) RenderSmdDiagram();
        }

        private void RenderDividerDiagram()
        {
            int n = dividerResistors.Count;
            CircuitCanvas.Height = 100 + (n * 100);
            double x = 100;
            var grayBrush = new SolidColorBrush(Color.FromRgb(156, 173, 193));
            var accentBrush = (Brush)FindResource("AccentBrush");
            var yellowBrush = (Brush)FindResource("AccentYellowBrush");

            Border vinBorder = new Border { Background = accentBrush, CornerRadius = new CornerRadius(2), Width = 50, Height = 30 };
            Canvas.SetLeft(vinBorder, 75); Canvas.SetTop(vinBorder, 5);
            vinBorder.Child = new TextBlock { Text = string.IsNullOrEmpty(InputVin.Text) ? "0V" : $"{InputVin.Text}V", Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 13, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            CircuitCanvas.Children.Add(vinBorder);

            CircuitCanvas.Children.Add(new Line { X1 = x, Y1 = 35, X2 = x, Y2 = 80, Stroke = grayBrush, StrokeThickness = 3 });

            for (int i = 0; i < n; i++)
            {
                double resistorTop = 80 + (i * 100);
                Border rBox = new Border { Width = 50, Height = 60, Background = new SolidColorBrush(Color.FromRgb(38, 49, 66)), BorderBrush = grayBrush, BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(2) };
                Canvas.SetLeft(rBox, 75); Canvas.SetTop(rBox, resistorTop);
                rBox.Child = new TextBlock { Text = $"R{i + 1}", Foreground = grayBrush, FontWeight = FontWeights.Bold, FontSize = 11, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                CircuitCanvas.Children.Add(rBox);

                Border rValBorder = new Border { Background = accentBrush, Padding = new Thickness(8, 3, 8, 3), CornerRadius = new CornerRadius(2) };
                Canvas.SetLeft(rValBorder, 135); Canvas.SetTop(rValBorder, resistorTop + 15);
                rValBorder.Child = new TextBlock { Text = string.IsNullOrEmpty(dividerResistors[i].Text) ? "0Ω" : $"{dividerResistors[i].Text}Ω", Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 11 };
                CircuitCanvas.Children.Add(rValBorder);

                if (i < n - 1)
                {
                    double junctionY = resistorTop + 80;
                    CircuitCanvas.Children.Add(new Line { X1 = x, Y1 = resistorTop + 60, X2 = x, Y2 = resistorTop + 100, Stroke = grayBrush, StrokeThickness = 3 });
                    Ellipse dot = new Ellipse { Width = 8, Height = 8, Fill = accentBrush };
                    Canvas.SetLeft(dot, 96); Canvas.SetTop(dot, junctionY - 4);
                    CircuitCanvas.Children.Add(dot);
                    CircuitCanvas.Children.Add(new Line { X1 = x + 4, Y1 = junctionY, X2 = 165, Y2 = junctionY, Stroke = grayBrush, StrokeThickness = 2 });
                    Border voutBorder = new Border { Background = yellowBrush, Padding = new Thickness(10, 4, 10, 4), CornerRadius = new CornerRadius(2) };
                    Canvas.SetLeft(voutBorder, 170); Canvas.SetTop(voutBorder, junctionY - 15);
                    voutBorder.Child = new TextBlock { Text = (voutOutputs.Count > i && !string.IsNullOrEmpty(voutOutputs[i].Text)) ? $"{voutOutputs[i].Text}V" : "0V", Foreground = Brushes.Black, FontWeight = FontWeights.Bold, FontSize = 12 };
                    CircuitCanvas.Children.Add(voutBorder);
                }
                else
                {
                    CircuitCanvas.Children.Add(new Line { X1 = x, Y1 = resistorTop + 60, X2 = x, Y2 = resistorTop + 100, Stroke = grayBrush, StrokeThickness = 3 });
                    double groundY = resistorTop + 100;
                    CircuitCanvas.Children.Add(new Line { X1 = 75, Y1 = groundY, X2 = 125, Y2 = groundY, Stroke = grayBrush, StrokeThickness = 3 });
                    CircuitCanvas.Children.Add(new Line { X1 = 85, Y1 = groundY + 10, X2 = 115, Y2 = groundY + 10, Stroke = grayBrush, StrokeThickness = 3 });
                    CircuitCanvas.Children.Add(new Line { X1 = 93, Y1 = groundY + 20, X2 = 107, Y2 = groundY + 20, Stroke = grayBrush, StrokeThickness = 3 });
                }
            }
        }

        private void RenderParallelDiagram()
        {
            int n = parallelResistors.Count;
            CircuitCanvas.Height = 100 + (n * 80);
            CircuitCanvas.Width = 250;
            var grayBrush = new SolidColorBrush(Color.FromRgb(156, 173, 193));
            var accentBrush = (Brush)FindResource("AccentBrush");
            var yellowBrush = (Brush)FindResource("AccentYellowBrush");

            double centerX = 125; double topY = 50; double bottomY = topY + (n - 1) * 80;
            double leftBusX = 60; double rightBusX = 190;

            CircuitCanvas.Children.Add(new Line { X1 = leftBusX - 30, Y1 = (topY + bottomY) / 2, X2 = leftBusX, Y2 = (topY + bottomY) / 2, Stroke = grayBrush, StrokeThickness = 3 });
            CircuitCanvas.Children.Add(new Line { X1 = rightBusX, Y1 = (topY + bottomY) / 2, X2 = rightBusX + 30, Y2 = (topY + bottomY) / 2, Stroke = grayBrush, StrokeThickness = 3 });

            Border rtBorder = new Border { Background = yellowBrush, Padding = new Thickness(8, 3, 8, 3), CornerRadius = new CornerRadius(2) };
            Canvas.SetLeft(rtBorder, rightBusX + 10); Canvas.SetTop(rtBorder, (topY + bottomY) / 2 - 25);
            rtBorder.Child = new TextBlock { Text = !string.IsNullOrEmpty(InputRTotal.Text) ? $"{InputRTotal.Text}Ω" : "0Ω", Foreground = Brushes.Black, FontWeight = FontWeights.Bold, FontSize = 11 };
            CircuitCanvas.Children.Add(rtBorder);
            CircuitCanvas.Children.Add(new TextBlock { Text = "RT", Foreground = grayBrush, FontSize = 10, FontWeight = FontWeights.Bold, Margin = new Thickness(rightBusX + 25, (topY + bottomY) / 2 + 5, 0, 0) });

            if (n > 1)
            {
                CircuitCanvas.Children.Add(new Line { X1 = leftBusX, Y1 = topY, X2 = leftBusX, Y2 = bottomY, Stroke = grayBrush, StrokeThickness = 3 });
                CircuitCanvas.Children.Add(new Line { X1 = rightBusX, Y1 = topY, X2 = rightBusX, Y2 = bottomY, Stroke = grayBrush, StrokeThickness = 3 });
            }

            for (int i = 0; i < n; i++)
            {
                double ry = topY + (i * 80);
                CircuitCanvas.Children.Add(new Line { X1 = leftBusX, Y1 = ry, X2 = centerX - 30, Y2 = ry, Stroke = grayBrush, StrokeThickness = 2 });
                CircuitCanvas.Children.Add(new Line { X1 = centerX + 30, Y1 = ry, X2 = rightBusX, Y2 = ry, Stroke = grayBrush, StrokeThickness = 2 });
                CircuitCanvas.Children.Add(new Ellipse { Width = 6, Height = 6, Fill = grayBrush, Margin = new Thickness(leftBusX - 3, ry - 3, 0, 0) });
                CircuitCanvas.Children.Add(new Ellipse { Width = 6, Height = 6, Fill = grayBrush, Margin = new Thickness(rightBusX - 3, ry - 3, 0, 0) });

                Border rBox = new Border { Width = 60, Height = 40, Background = new SolidColorBrush(Color.FromRgb(38, 49, 66)), BorderBrush = grayBrush, BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(2) };
                Canvas.SetLeft(rBox, centerX - 30); Canvas.SetTop(rBox, ry - 20);
                rBox.Child = new TextBlock { Text = $"R{i + 1}", Foreground = grayBrush, FontWeight = FontWeights.Bold, FontSize = 11, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                CircuitCanvas.Children.Add(rBox);

                Border valBorder = new Border { Background = accentBrush, Padding = new Thickness(5, 2, 5, 2), CornerRadius = new CornerRadius(2) };
                Canvas.SetLeft(valBorder, centerX - 20); Canvas.SetTop(valBorder, ry + 22);
                valBorder.Child = new TextBlock { Text = string.IsNullOrEmpty(parallelResistors[i].Text) ? "0Ω" : $"{parallelResistors[i].Text}Ω", Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 10, TextAlignment = TextAlignment.Center };
                CircuitCanvas.Children.Add(valBorder);
            }
        }

        private void BtnCalc_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            try {
                string content = btn.Content?.ToString() ?? "";
                if (currentMode == AppMode.Divider) {
                    if (btn == BtnCalcVin) CalculateVin();
                    else if (content.StartsWith("Vout")) CalculateVouts();
                    else if (content.StartsWith("R")) CalculateDividerResistor(btn);
                    else CalculateVouts();
                } else if (currentMode == AppMode.Parallel) {
                    if (btn == BtnCalcRTotal) CalculateRTotal();
                    else if (content.StartsWith("R")) CalculateParallelResistor(btn);
                    else CalculateRTotal();
                } else if (currentMode == AppMode.Regulator) {
                    CalculateRegulator(btn);
                } else {
                    CalculateVnode(btn);
                }
            } catch { ShowError(); }
        }

        private void CalculateVouts()
        {
            if (!double.TryParse(InputVin.Text, out double vin)) return;
            
            double totalR = 0;
            foreach (var input in dividerResistors)
            {
                if (double.TryParse(input.Text, out double r)) totalR += r;
                else return; 
            }

            if (totalR <= 0) return;

            for (int i = 0; i < voutOutputs.Count; i++)
            {
                double rBelow = 0;
                for (int j = i + 1; j < dividerResistors.Count; j++)
                {
                    if (double.TryParse(dividerResistors[j].Text, out double r)) rBelow += r;
                }
                voutOutputs[i].Text = (vin * (rBelow / totalR)).ToString("G4");
            }
            UpdateAll();
        }

        private void CalculateVin()
        {
            for (int i = 0; i < voutOutputs.Count; i++)
            {
                if (double.TryParse(voutOutputs[i].Text, out double vout) && vout > 0)
                {
                    double totalR = 0;
                    double rBelow = 0;
                    bool allRValid = true;

                    for (int j = 0; j < dividerResistors.Count; j++)
                    {
                        if (double.TryParse(dividerResistors[j].Text, out double r))
                        {
                            totalR += r;
                            if (j > i) rBelow += r;
                        }
                        else { allRValid = false; break; }
                    }

                    if (allRValid && rBelow > 0)
                    {
                        InputVin.Text = (vout * (totalR / rBelow)).ToString("G4");
                        UpdateAll();
                        return;
                    }
                }
            }
        }

        private void CalculateDividerResistor(Button btn)
        {
            if (!double.TryParse(InputVin.Text, out double vin) || vin <= 0) return;
            int targetIdx = dividerResistors.IndexOf(dividerResistors.FirstOrDefault(r => 
                (r.Parent as Grid)?.Parent is Grid row && row.Children[0] == btn));
            
            if (targetIdx == -1) return;

            double vAbove = (targetIdx == 0) ? vin : 
                           (double.TryParse(voutOutputs[targetIdx - 1].Text, out double va) ? va : -1);
            double vBelow = (targetIdx == dividerResistors.Count - 1) ? 0 : 
                           (double.TryParse(voutOutputs[targetIdx].Text, out double vb) ? vb : -1);

            if (vAbove != -1 && vBelow != -1)
            {
                double current = -1;
                for (int k = 0; k < dividerResistors.Count; k++)
                {
                    if (k == targetIdx) continue;
                    if (double.TryParse(dividerResistors[k].Text, out double rk) && rk > 0)
                    {
                        double vkAbove = (k == 0) ? vin : (double.TryParse(voutOutputs[k - 1].Text, out double vka) ? vka : -1);
                        double vkBelow = (k == dividerResistors.Count - 1) ? 0 : (double.TryParse(voutOutputs[k].Text, out double vkb) ? vkb : -1);
                        if (vkAbove != -1 && vkBelow != -1 && Math.Abs(vkAbove - vkBelow) > 1e-9)
                        {
                            current = (vkAbove - vkBelow) / rk;
                            break;
                        }
                    }
                }

                if (current > 0)
                {
                    dividerResistors[targetIdx].Text = ((vAbove - vBelow) / current).ToString("G4");
                    UpdateAll();
                }
            }
        }

        private void CalculateRTotal()
        {
            double invTotal = 0;
            int validCount = 0;
            foreach (var input in parallelResistors)
            {
                if (double.TryParse(input.Text, out double r) && r != 0)
                {
                    invTotal += 1.0 / r;
                    validCount++;
                }
            }
            if (validCount > 0 && invTotal != 0)
            {
                InputRTotal.Text = (1.0 / invTotal).ToString("G4");
            }
            UpdateAll();
        }

        private void CalculateParallelResistor(Button btn)
        {
            if (!double.TryParse(InputRTotal.Text, out double rt) || rt <= 0) return;
            
            int targetIdx = parallelResistors.IndexOf(parallelResistors.FirstOrDefault(r => 
                (r.Parent as Grid)?.Parent is Grid row && row.Children[0] == btn));
            
            if (targetIdx == -1) return;

            double currentInvSum = 0;
            for (int i = 0; i < parallelResistors.Count; i++)
            {
                if (i == targetIdx) continue;
                if (double.TryParse(parallelResistors[i].Text, out double r) && r > 0) currentInvSum += 1.0 / r;
                else return; 
            }

            double targetInv = (1.0 / rt) - currentInvSum;
            if (targetInv > 1e-12)
            {
                parallelResistors[targetIdx].Text = (1.0 / targetInv).ToString("G4");
                UpdateAll();
            }
        }

        private void CalculateRegulator(Button btn)
        {
            if (btn == BtnCalcRegVref)
            {
                if (double.TryParse(InputRegVout.Text, out double vout) && double.TryParse(InputRegR1.Text, out double r1) && double.TryParse(InputRegR2.Text, out double r2))
                {
                    if (r1 == 0 || (1 + r2 / r1) == 0) return;
                    InputVref.Text = (vout / (1 + r2 / r1)).ToString("G4");
                }
            }
            else
            {
                double vref = double.TryParse(InputVref.Text, out double vr) ? vr : 1.25;
                if (btn == BtnCalcRegVout)
                {
                    if (double.TryParse(InputRegR1.Text, out double r1) && double.TryParse(InputRegR2.Text, out double r2))
                    {
                        if (r1 == 0) return;
                        InputRegVout.Text = (vref * (1 + r2 / r1)).ToString("G4");
                    }
                }
                else if (btn == BtnCalcRegR1)
                {
                    if (double.TryParse(InputRegVout.Text, out double vout) && double.TryParse(InputRegR2.Text, out double r2))
                    {
                        if (vout <= vref) return;
                        InputRegR1.Text = (r2 / (vout / vref - 1)).ToString("G4");
                    }
                }
                else if (btn == BtnCalcRegR2)
                {
                    if (double.TryParse(InputRegVout.Text, out double vout) && double.TryParse(InputRegR1.Text, out double r1))
                    {
                        if (vout <= vref) return;
                        InputRegR2.Text = (r1 * (vout / vref - 1)).ToString("G4");
                    }
                }
            }
            UpdateAll();
        }

        private void RenderRegulatorDiagram()
        {
            CircuitCanvas.Height = 380;
            CircuitCanvas.Width = 380; 
            var grayBrush = new SolidColorBrush(Color.FromRgb(156, 173, 193));
            var accentBrush = (Brush)FindResource("AccentBrush");
            var yellowBrush = (Brush)FindResource("AccentYellowBrush");

            double centerX = 210; // Shifted further right to balance R1 and ADJ distance
            double icY = 70;
            double icH = 65;
            double icW = 110;
            double pinY = icY + icH / 2;

            // IC Body
            Border icBody = new Border { Width = icW, Height = icH, Background = new SolidColorBrush(Color.FromRgb(38, 49, 66)), BorderBrush = grayBrush, BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(4) };
            Canvas.SetLeft(icBody, centerX - icW / 2); Canvas.SetTop(icBody, icY);
            icBody.Child = new TextBlock { Text = "1117 ADJ", Foreground = grayBrush, FontWeight = FontWeights.Bold, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            CircuitCanvas.Children.Add(icBody);

            // VIN (Right)
            CircuitCanvas.Children.Add(new Line { X1 = centerX + icW / 2, Y1 = pinY, X2 = centerX + 80, Y2 = pinY, Stroke = grayBrush, StrokeThickness = 3 });
            CircuitCanvas.Children.Add(new TextBlock { Text = "VIN", Foreground = grayBrush, FontSize = 12, FontWeight = FontWeights.Bold, Margin = new Thickness(centerX + 85, pinY - 10, 0, 0) });

            // VOUT (Left)
            double voutEndX = 40;
            CircuitCanvas.Children.Add(new Line { X1 = voutEndX, Y1 = pinY, X2 = centerX - icW / 2, Y2 = pinY, Stroke = grayBrush, StrokeThickness = 3 });
            CircuitCanvas.Children.Add(new TextBlock { Text = "VOUT", Foreground = yellowBrush, FontSize = 12, FontWeight = FontWeights.Bold, Margin = new Thickness(voutEndX, pinY - 35, 0, 0) });
            
            // VOUT Value Label
            Border voutVal = new Border { Background = yellowBrush, Padding = new Thickness(6, 3, 6, 3), CornerRadius = new CornerRadius(3) };
            Canvas.SetLeft(voutVal, voutEndX); Canvas.SetTop(voutVal, pinY - 18);
            voutVal.Child = new TextBlock { Text = !string.IsNullOrEmpty(InputRegVout.Text) ? $"{InputRegVout.Text}V" : "0V", Foreground = Brushes.Black, FontWeight = FontWeights.Bold, FontSize = 13 };
            CircuitCanvas.Children.Add(voutVal);

            // ADJ (Bottom Pin Wire)
            double adjWireY = pinY + 85;
            CircuitCanvas.Children.Add(new Line { X1 = centerX, Y1 = icY + icH, X2 = centerX, Y2 = adjWireY, Stroke = grayBrush, StrokeThickness = 3 });
            
            // R1 Column (Vertical)
            double r1X = voutEndX + 65; 
            CircuitCanvas.Children.Add(new Line { X1 = r1X, Y1 = pinY, X2 = r1X, Y2 = adjWireY, Stroke = grayBrush, StrokeThickness = 2 });
            CircuitCanvas.Children.Add(new Line { X1 = r1X, Y1 = adjWireY, X2 = centerX, Y2 = adjWireY, Stroke = grayBrush, StrokeThickness = 2 });
            
            // Junction dots
            CircuitCanvas.Children.Add(new Ellipse { Width = 7, Height = 7, Fill = grayBrush, Margin = new Thickness(r1X - 3.5, pinY - 3.5, 0, 0) });
            CircuitCanvas.Children.Add(new Ellipse { Width = 7, Height = 7, Fill = grayBrush, Margin = new Thickness(centerX - 3.5, adjWireY - 3.5, 0, 0) });

            // R1 Box (Balanced size)
            Border r1Box = new Border { Width = 50, Height = 45, Background = new SolidColorBrush(Color.FromRgb(38, 49, 66)), BorderBrush = grayBrush, BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(2) };
            Canvas.SetLeft(r1Box, r1X - 25); Canvas.SetTop(r1Box, pinY + 20);
            r1Box.Child = new TextBlock { Text = "R1", Foreground = grayBrush, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            CircuitCanvas.Children.Add(r1Box);

            // R1 Value Label
            Border r1Val = new Border { Background = accentBrush, Padding = new Thickness(6, 3, 6, 3), CornerRadius = new CornerRadius(3) };
            Canvas.SetLeft(r1Val, r1X - 70); Canvas.SetTop(r1Val, pinY + 25);
            r1Val.Child = new TextBlock { Text = !string.IsNullOrEmpty(InputRegR1.Text) ? $"{InputRegR1.Text}Ω" : "0Ω", Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 12 };
            CircuitCanvas.Children.Add(r1Val);

            // R2 Column (Vertical)
            double r2TopY = adjWireY + 15;
            CircuitCanvas.Children.Add(new Line { X1 = centerX, Y1 = adjWireY, X2 = centerX, Y2 = r2TopY, Stroke = grayBrush, StrokeThickness = 3 });
            
            // R2 Box (Matched width with R1)
            Border r2Box = new Border { Width = 50, Height = 65, Background = new SolidColorBrush(Color.FromRgb(38, 49, 66)), BorderBrush = grayBrush, BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(2) };
            Canvas.SetLeft(r2Box, centerX - 25); Canvas.SetTop(r2Box, r2TopY);
            r2Box.Child = new TextBlock { Text = "R2", Foreground = grayBrush, FontWeight = FontWeights.Bold, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            CircuitCanvas.Children.Add(r2Box);

            // R2 Value Label
            Border r2Val = new Border { Background = accentBrush, Padding = new Thickness(6, 3, 6, 3), CornerRadius = new CornerRadius(3) };
            Canvas.SetLeft(r2Val, centerX + 35); Canvas.SetTop(r2Val, r2TopY + 20);
            r2Val.Child = new TextBlock { Text = !string.IsNullOrEmpty(InputRegR2.Text) ? $"{InputRegR2.Text}Ω" : "0Ω", Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 12 };
            CircuitCanvas.Children.Add(r2Val);

            // Ground
            double gy = r2TopY + 65;
            CircuitCanvas.Children.Add(new Line { X1 = centerX, Y1 = r2TopY + 65, X2 = centerX, Y2 = gy + 20, Stroke = grayBrush, StrokeThickness = 3 });
            gy += 20;
            CircuitCanvas.Children.Add(new Line { X1 = centerX - 25, Y1 = gy, X2 = centerX + 25, Y2 = gy, Stroke = grayBrush, StrokeThickness = 3 });
            CircuitCanvas.Children.Add(new Line { X1 = centerX - 15, Y1 = gy + 10, X2 = centerX + 15, Y2 = gy + 10, Stroke = grayBrush, StrokeThickness = 3 });
            CircuitCanvas.Children.Add(new Line { X1 = centerX - 7, Y1 = gy + 20, X2 = centerX + 7, Y2 = gy + 20, Stroke = grayBrush, StrokeThickness = 3 });
        }

        private void CalculateVnode(Button btn)
        {
            double v1, v2, v3, r1, r2, r3;
            bool hv1 = double.TryParse(InputVnodeV1.Text, out v1);
            bool hv2 = double.TryParse(InputVnodeV2.Text, out v2);
            bool hv3 = double.TryParse(InputVnodeV3.Text, out v3);
            bool hr1 = double.TryParse(InputVnodeR1.Text, out r1);
            bool hr2 = double.TryParse(InputVnodeR2.Text, out r2);
            bool hr3 = double.TryParse(InputVnodeR3.Text, out r3);

            if (btn == BtnCalcVnodeV3)
            {
                if (hv1 && hv2 && hr1 && hr2 && hr3 && r1 > 0 && r2 > 0 && r3 > 0)
                {
                    double g1 = 1 / r1, g2 = 1 / r2, g3 = 1 / r3;
                    v3 = (v1 * g1 + v2 * g2) / (g1 + g2 + g3);
                    InputVnodeV3.Text = v3.ToString("0.###");
                }
            }
            else if (btn == BtnCalcVnodeV1)
            {
                if (hv3 && hv2 && hr1 && hr2 && hr3 && r1 > 0 && r2 > 0 && r3 > 0)
                {
                    double g1 = 1 / r1, g2 = 1 / r2, g3 = 1 / r3;
                    v1 = (v3 * (g1 + g2 + g3) - v2 * g2) / g1;
                    InputVnodeV1.Text = v1.ToString("0.###");
                }
            }
            else if (btn == BtnCalcVnodeV2)
            {
                if (hv3 && hv1 && hr1 && hr2 && hr3 && r1 > 0 && r2 > 0 && r3 > 0)
                {
                    double g1 = 1 / r1, g2 = 1 / r2, g3 = 1 / r3;
                    v2 = (v3 * (g1 + g2 + g3) - v1 * g1) / g2;
                    InputVnodeV2.Text = v2.ToString("0.###");
                }
            }
            else if (btn == BtnCalcVnodeR1)
            {
                if (hv1 && hv2 && hv3 && hr2 && hr3 && r2 > 0 && r3 > 0)
                {
                    double rightSide = v2 / r2 - v3 / r2 - v3 / r3;
                    if (rightSide != 0)
                    {
                        r1 = (v3 - v1) / rightSide;
                        if (r1 > 0) InputVnodeR1.Text = r1.ToString("0.###");
                    }
                }
            }
            else if (btn == BtnCalcVnodeR2)
            {
                if (hv1 && hv2 && hv3 && hr1 && hr3 && r1 > 0 && r3 > 0)
                {
                    double rightSide = v1 / r1 - v3 / r1 - v3 / r3;
                    if (rightSide != 0)
                    {
                        r2 = (v3 - v2) / rightSide;
                        if (r2 > 0) InputVnodeR2.Text = r2.ToString("0.###");
                    }
                }
            }
            else if (btn == BtnCalcVnodeR3)
            {
                if (hv1 && hv2 && hv3 && hr1 && hr2 && r1 > 0 && r2 > 0)
                {
                    double rightSide = v1 / r1 + v2 / r2 - v3 / r1 - v3 / r2;
                    if (rightSide != 0)
                    {
                        r3 = v3 / rightSide;
                        if (r3 > 0) InputVnodeR3.Text = r3.ToString("0.###");
                    }
                }
            }
            UpdateAll();
        }

        private void RenderVnodeDiagram()
        {
            CircuitCanvas.Height = 350;
            CircuitCanvas.Width = 350;
            var grayBrush = new SolidColorBrush(Color.FromRgb(156, 173, 193));
            var accentBrush = (Brush)FindResource("AccentBrush");
            var yellowBrush = (Brush)FindResource("AccentYellowBrush");

            double startX = 60;
            double rEndX = 180;
            double v3X = 240;
            
            double y1 = 100;
            double y2 = 220;
            double midY = (y1 + y2) / 2; // 160
            
            // Wires for R1
            CircuitCanvas.Children.Add(new Line { X1 = startX, Y1 = y1, X2 = rEndX, Y2 = y1, Stroke = grayBrush, StrokeThickness = 3 });
            // Wires for R2
            CircuitCanvas.Children.Add(new Line { X1 = startX, Y1 = y2, X2 = rEndX, Y2 = y2, Stroke = grayBrush, StrokeThickness = 3 });
            
            // Vertical wire joining R1 and R2
            CircuitCanvas.Children.Add(new Line { X1 = rEndX, Y1 = y1, X2 = rEndX, Y2 = y2, Stroke = grayBrush, StrokeThickness = 3 });
            // Wire to V3 node
            CircuitCanvas.Children.Add(new Line { X1 = rEndX, Y1 = midY, X2 = v3X, Y2 = midY, Stroke = grayBrush, StrokeThickness = 3 });
            // Wire down through R3
            double groundY = y2 + 50;
            CircuitCanvas.Children.Add(new Line { X1 = v3X, Y1 = midY, X2 = v3X, Y2 = groundY, Stroke = grayBrush, StrokeThickness = 3 });

            // Junction dots
            CircuitCanvas.Children.Add(new Ellipse { Width = 7, Height = 7, Fill = grayBrush, Margin = new Thickness(rEndX - 3.5, midY - 3.5, 0, 0) });
            CircuitCanvas.Children.Add(new Ellipse { Width = 7, Height = 7, Fill = grayBrush, Margin = new Thickness(v3X - 3.5, midY - 3.5, 0, 0) });

            // Terminal dots for V1, V2
            CircuitCanvas.Children.Add(new Ellipse { Width = 7, Height = 7, Fill = accentBrush, Margin = new Thickness(startX - 3.5, y1 - 3.5, 0, 0) });
            CircuitCanvas.Children.Add(new Ellipse { Width = 7, Height = 7, Fill = accentBrush, Margin = new Thickness(startX - 3.5, y2 - 3.5, 0, 0) });

            // V1 Label & Value
            string v1Str = string.IsNullOrEmpty(InputVnodeV1.Text) ? "0" : InputVnodeV1.Text;
            TextBlock lblV1 = new TextBlock { Text = "V1", Foreground = grayBrush, FontSize = 12, FontWeight = FontWeights.Bold };
            Canvas.SetLeft(lblV1, startX - 35); Canvas.SetTop(lblV1, y1 - 35);
            CircuitCanvas.Children.Add(lblV1);
            Border valV1 = new Border { Background = accentBrush, Padding = new Thickness(6,3,6,3), CornerRadius = new CornerRadius(3) };
            valV1.Child = new TextBlock { Text = v1Str + "V", Foreground = Brushes.White, FontSize = 11, FontWeight = FontWeights.Bold };
            Canvas.SetLeft(valV1, startX - 35); Canvas.SetTop(valV1, y1 - 15);
            CircuitCanvas.Children.Add(valV1);

            // V2 Label & Value
            string v2Str = string.IsNullOrEmpty(InputVnodeV2.Text) ? "0" : InputVnodeV2.Text;
            TextBlock lblV2 = new TextBlock { Text = "V2", Foreground = grayBrush, FontSize = 12, FontWeight = FontWeights.Bold };
            Canvas.SetLeft(lblV2, startX - 35); Canvas.SetTop(lblV2, y2 - 35);
            CircuitCanvas.Children.Add(lblV2);
            Border valV2 = new Border { Background = accentBrush, Padding = new Thickness(6,3,6,3), CornerRadius = new CornerRadius(3) };
            valV2.Child = new TextBlock { Text = v2Str + "V", Foreground = Brushes.White, FontSize = 11, FontWeight = FontWeights.Bold };
            Canvas.SetLeft(valV2, startX - 35); Canvas.SetTop(valV2, y2 - 15);
            CircuitCanvas.Children.Add(valV2);

            // R1 Box
            double r1CenterX = (startX + rEndX) / 2;
            Border r1Box = new Border { Width = 60, Height = 30, Background = new SolidColorBrush(Color.FromRgb(38, 49, 66)), BorderBrush = grayBrush, BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(2) };
            Canvas.SetLeft(r1Box, r1CenterX - 30); Canvas.SetTop(r1Box, y1 - 15);
            r1Box.Child = new TextBlock { Text = "R1", Foreground = grayBrush, FontWeight = FontWeights.Bold, FontSize = 11, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            CircuitCanvas.Children.Add(r1Box);
            
            Border r1Val = new Border { Background = accentBrush, Padding = new Thickness(5,2,5,2), CornerRadius = new CornerRadius(2) };
            Canvas.SetLeft(r1Val, r1CenterX - 20); Canvas.SetTop(r1Val, y1 + 20);
            r1Val.Child = new TextBlock { Text = string.IsNullOrEmpty(InputVnodeR1.Text) ? "0Ω" : $"{InputVnodeR1.Text}Ω", Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 10 };
            CircuitCanvas.Children.Add(r1Val);

            // R2 Box
            double r2CenterX = (startX + rEndX) / 2;
            Border r2Box = new Border { Width = 60, Height = 30, Background = new SolidColorBrush(Color.FromRgb(38, 49, 66)), BorderBrush = grayBrush, BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(2) };
            Canvas.SetLeft(r2Box, r2CenterX - 30); Canvas.SetTop(r2Box, y2 - 15);
            r2Box.Child = new TextBlock { Text = "R2", Foreground = grayBrush, FontWeight = FontWeights.Bold, FontSize = 11, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            CircuitCanvas.Children.Add(r2Box);
            
            Border r2Val = new Border { Background = accentBrush, Padding = new Thickness(5,2,5,2), CornerRadius = new CornerRadius(2) };
            Canvas.SetLeft(r2Val, r2CenterX - 20); Canvas.SetTop(r2Val, y2 + 20);
            r2Val.Child = new TextBlock { Text = string.IsNullOrEmpty(InputVnodeR2.Text) ? "0Ω" : $"{InputVnodeR2.Text}Ω", Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 10 };
            CircuitCanvas.Children.Add(r2Val);

            // R3 Box (Vertical)
            double r3CenterY = (midY + groundY) / 2;
            Border r3Box = new Border { Width = 40, Height = 50, Background = new SolidColorBrush(Color.FromRgb(38, 49, 66)), BorderBrush = grayBrush, BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(2) };
            Canvas.SetLeft(r3Box, v3X - 20); Canvas.SetTop(r3Box, r3CenterY - 25);
            r3Box.Child = new TextBlock { Text = "R3", Foreground = grayBrush, FontWeight = FontWeights.Bold, FontSize = 11, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            CircuitCanvas.Children.Add(r3Box);
            
            Border r3Val = new Border { Background = accentBrush, Padding = new Thickness(5,2,5,2), CornerRadius = new CornerRadius(2) };
            Canvas.SetLeft(r3Val, v3X + 25); Canvas.SetTop(r3Val, r3CenterY - 10);
            r3Val.Child = new TextBlock { Text = string.IsNullOrEmpty(InputVnodeR3.Text) ? "0Ω" : $"{InputVnodeR3.Text}Ω", Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 10 };
            CircuitCanvas.Children.Add(r3Val);

            // Vnode (V3)
            string v3Str = string.IsNullOrEmpty(InputVnodeV3.Text) ? "0" : InputVnodeV3.Text;
            TextBlock lblV3 = new TextBlock { Text = "Vnode", Foreground = grayBrush, FontSize = 12, FontWeight = FontWeights.Bold };
            Canvas.SetLeft(lblV3, v3X + 15); Canvas.SetTop(lblV3, midY - 25);
            CircuitCanvas.Children.Add(lblV3);
            Border valV3 = new Border { Background = yellowBrush, Padding = new Thickness(6,3,6,3), CornerRadius = new CornerRadius(3) };
            valV3.Child = new TextBlock { Text = v3Str + "V", Foreground = Brushes.Black, FontSize = 11, FontWeight = FontWeights.Bold };
            Canvas.SetLeft(valV3, v3X + 15); Canvas.SetTop(valV3, midY - 5);
            CircuitCanvas.Children.Add(valV3);

            // Ground Symbol
            CircuitCanvas.Children.Add(new Line { X1 = v3X - 20, Y1 = groundY, X2 = v3X + 20, Y2 = groundY, Stroke = grayBrush, StrokeThickness = 3 });
            CircuitCanvas.Children.Add(new Line { X1 = v3X - 12, Y1 = groundY + 8, X2 = v3X + 12, Y2 = groundY + 8, Stroke = grayBrush, StrokeThickness = 3 });
            CircuitCanvas.Children.Add(new Line { X1 = v3X - 5, Y1 = groundY + 16, X2 = v3X + 5, Y2 = groundY + 16, Stroke = grayBrush, StrokeThickness = 3 });
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var grid = btn?.Parent as Grid;
            var txt = grid?.Children[0] as TextBox;
            if (txt != null) txt.Text = "";
        }

        private void ShowError() { MessageBox.Show(currentLang == "en" ? "Please enter valid numbers!" : "Vui lòng nhập số hợp lệ!"); }

        private void InitializeColorBandsUI()
        {
            if (ColorBandsGrid == null) return;
            ColorBandsGrid.Children.Clear();

            bool is6Band = Rb6Band?.IsChecked == true;
            bool is5Band = Rb5Band?.IsChecked == true || is6Band;
            int bandsCount = is6Band ? 6 : (is5Band ? 5 : 4);

            // Safety: Ensure all needed indices are valid
            for (int i = 0; i < bandsCount; i++)
            {
                if (selectedColorIndices[i] < 0 || selectedColorIndices[i] >= colorBands.Count)
                    selectedColorIndices[i] = 0; // Default to Black
            }

            int mulIdx = is5Band ? 3 : 2;
            int tolIdx = is5Band ? 4 : 3;
            int tempIdx = 5;

            for (int bandIdx = 0; bandIdx < bandsCount; bandIdx++)
            {
                StackPanel colPanel = new StackPanel { Margin = new Thickness(2) };
                
                TextBlock header = new TextBlock 
                { 
                    Text = currentLang == "en" ? $"Band {bandIdx + 1}" : $"Vòng {bandIdx + 1}", 
                    Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0,0,0,10), FontSize = 12 
                };
                colPanel.Children.Add(header);

                // Add color buttons based on allowed bands
                for (int c = 0; c < colorBands.Count; c++)
                {
                    var colorObj = colorBands[c];
                    
                    bool isValueBand = (bandIdx < mulIdx);
                    if (isValueBand && colorObj.Value == null) continue;

                    bool isMultiplierBand = (bandIdx == mulIdx);
                    if (isMultiplierBand && colorObj.Multiplier == null) continue;

                    bool isToleranceBand = (bandIdx == tolIdx);
                    if (isToleranceBand && colorObj.Tolerance == null) continue;

                    bool isTempCoBand = is6Band && (bandIdx == tempIdx);
                    if (isTempCoBand && colorObj.TempCoef == null) continue;

                    Border btnBorder = new Border
                    {
                        Tag = new int[] { bandIdx, c },
                        Background = new SolidColorBrush(colorObj.Hex),
                        BorderThickness = new Thickness(2),
                        CornerRadius = new CornerRadius(2),
                        Height = 22,
                        Margin = new Thickness(0, 0, 0, 4),
                        Cursor = System.Windows.Input.Cursors.Hand
                    };
                    btnBorder.BorderBrush = (selectedColorIndices[bandIdx] == c) ? Brushes.White : Brushes.Transparent;
                    
                    btnBorder.Child = new TextBlock
                    {
                        Text = colorObj.NameEn,
                        Foreground = new SolidColorBrush(colorObj.FgFormat),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 11,
                        IsHitTestVisible = false
                    };
                    
                    btnBorder.MouseUp += ColorBtn_Click;
                    colPanel.Children.Add(btnBorder);
                }
                
                Grid.SetColumn(colPanel, bandIdx);
                ColorBandsGrid.Children.Add(colPanel);
            }

            CalculateColorCode();
        }

        private void BandToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;
            InitializeColorBandsUI();
            UpdateAll();
        }

        private void ColorBtn_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is int[] data)
            {
                int bandIdx = data[0];
                int colorIdx = data[1];
                selectedColorIndices[bandIdx] = colorIdx;
                InitializeColorBandsUI();
                UpdateAll();
            }
        }

        private void CalculateColorCode()
        {
            bool is6Band = Rb6Band?.IsChecked == true;
            bool is5Band = Rb5Band?.IsChecked == true || is6Band;
            if (TxtColorResult == null) return;

            int val = 0;
            double mult = 1;
            double tol = 0;

            if (is5Band)
            {
                val = (colorBands[selectedColorIndices[0]].Value ?? 0) * 100 +
                      (colorBands[selectedColorIndices[1]].Value ?? 0) * 10 +
                      (colorBands[selectedColorIndices[2]].Value ?? 0);
                mult = colorBands[selectedColorIndices[3]].Multiplier ?? 1;
                tol = colorBands[selectedColorIndices[4]].Tolerance ?? 0;
            }
            else
            {
                val = (colorBands[selectedColorIndices[0]].Value ?? 0) * 10 +
                      (colorBands[selectedColorIndices[1]].Value ?? 0);
                mult = colorBands[selectedColorIndices[2]].Multiplier ?? 1;
                tol = colorBands[selectedColorIndices[3]].Tolerance ?? 0;
            }

            double resistance = val * mult;
            TxtColorResult.Text = FormatOhms(resistance);
            TxtColorTolerance.Text = currentLang == "en" ? $"Tolerance: ±{tol}%" : $"Sai số: ±{tol}%";

            if (is6Band)
            {
                int tempco = colorBands[selectedColorIndices[5]].TempCoef ?? 0;
                TxtColorTempCo.Text = $"{tempco} ppm/K";
                TxtColorTempCo.Visibility = Visibility.Visible;
            }
            else
            {
                if (TxtColorTempCo != null) TxtColorTempCo.Visibility = Visibility.Collapsed;
            }
        }

        private string FormatOhms(double ohms)
        {
            if (ohms >= 1000000) return (ohms / 1000000).ToString("0.##") + " MΩ";
            if (ohms >= 1000) return (ohms / 1000).ToString("0.##") + " kΩ";
            return ohms.ToString("0.##") + " Ω";
        }

        private void RenderColorCodeDiagram()
        {
            if (CircuitCanvas == null) return;
            CircuitCanvas.Height = 150;
            CircuitCanvas.Width = 350;

            bool is6Band = Rb6Band?.IsChecked == true;
            bool is5Band = Rb5Band?.IsChecked == true || is6Band;
            int numBands = is6Band ? 6 : (is5Band ? 5 : 4);
            
            // Draw Wire
            var grayBrush = new SolidColorBrush(Color.FromRgb(156, 173, 193));
            CircuitCanvas.Children.Add(new Line { X1 = 20, Y1 = 60, X2 = 330, Y2 = 60, Stroke = grayBrush, StrokeThickness = 3 });

            // Draw Resistor Body
            Color bodyColor = is5Band ? Color.FromRgb(158, 198, 214) : Color.FromRgb(209, 180, 140);
            Rectangle rBody = new Rectangle { Width = 180, Height = 50, Fill = new SolidColorBrush(bodyColor), RadiusX = 10, RadiusY = 10 };
            Canvas.SetLeft(rBody, 85);
            Canvas.SetTop(rBody, 35);
            CircuitCanvas.Children.Add(rBody);

            // Draw Bands
            double startX = 105;
            double spacing = is6Band ? 20 : (is5Band ? 25 : 35);
            
            for (int i = 0; i < numBands; i++)
            {
                double xOffset = startX + (i * spacing);
                if (!is6Band && i == numBands - 1) xOffset += 15;
                if (is6Band && i >= 4) xOffset += 15;

                Color bandCol = colorBands[selectedColorIndices[i]].Hex;
                Rectangle band = new Rectangle { Width = 12, Height = 50, Fill = new SolidColorBrush(bandCol) };
                Canvas.SetLeft(band, xOffset);
                Canvas.SetTop(band, 35);
                CircuitCanvas.Children.Add(band);
            }
        }

        private void InputSmdCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateSmdCode();
            UpdateAll();
        }

        private void CalculateSmdCode()
        {
            if (InputSmdCode == null || TxtSmdResult == null) return;
            string code = InputSmdCode.Text.ToUpper().Trim();
            if (string.IsNullOrEmpty(code))
            {
                TxtSmdResult.Text = "0 Ω";
                return;
            }

            try
            {
                double value = 0;
                if (code.Contains("R"))
                {
                    string replaced = code.Replace("R", ".");
                    if (double.TryParse(replaced, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
                    {
                        TxtSmdResult.Text = FormatOhms(value);
                        return;
                    }
                }

                if (code.Length == 3)
                {
                    int digits = int.Parse(code.Substring(0, 2));
                    int multiplier = int.Parse(code.Substring(2, 1));
                    value = digits * Math.Pow(10, multiplier);
                }
                else if (code.Length == 4)
                {
                    int digits = int.Parse(code.Substring(0, 3));
                    int multiplier = int.Parse(code.Substring(3, 1));
                    value = digits * Math.Pow(10, multiplier);
                }
                else
                {
                    TxtSmdResult.Text = "---";
                    return;
                }

                TxtSmdResult.Text = FormatOhms(value);
            }
            catch
            {
                TxtSmdResult.Text = "ERR";
            }
        }

        private void RenderSmdDiagram()
        {
            if (CircuitCanvas == null) return;
            CircuitCanvas.Height = 200;
            CircuitCanvas.Width = 350;

            var grayBrush = new SolidColorBrush(Color.FromRgb(156, 173, 193));
            var bgBrush = new SolidColorBrush(Color.FromRgb(28, 36, 47));
            
            // SMD Body (Black rectangle)
            Border body = new Border
            {
                Width = 200,
                Height = 100,
                Background = new SolidColorBrush(Color.FromRgb(20, 20, 20)),
                CornerRadius = new CornerRadius(5),
                BorderBrush = grayBrush,
                BorderThickness = new Thickness(1)
            };
            Canvas.SetLeft(body, 75);
            Canvas.SetTop(body, 50);
            
            // Terminals (Silver ends)
            Rectangle leftTerm = new Rectangle { Width = 30, Height = 100, Fill = grayBrush };
            Canvas.SetLeft(leftTerm, 75); Canvas.SetTop(leftTerm, 50);
            Rectangle rightTerm = new Rectangle { Width = 30, Height = 100, Fill = grayBrush };
            Canvas.SetLeft(rightTerm, 245); Canvas.SetTop(rightTerm, 50);
            
            CircuitCanvas.Children.Add(body);
            CircuitCanvas.Children.Add(leftTerm);
            CircuitCanvas.Children.Add(rightTerm);

            // Text on SMD
            string codeText = string.IsNullOrEmpty(InputSmdCode.Text) ? "000" : InputSmdCode.Text.ToUpper();
            TextBlock txt = new TextBlock
            {
                Text = codeText,
                Foreground = grayBrush,
                FontSize = 36,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            body.Child = txt;

            // Wires
            CircuitCanvas.Children.Add(new Line { X1 = 20, Y1 = 100, X2 = 75, Y2 = 100, Stroke = grayBrush, StrokeThickness = 4 });
            CircuitCanvas.Children.Add(new Line { X1 = 275, Y1 = 100, X2 = 330, Y2 = 100, Stroke = grayBrush, StrokeThickness = 4 });
        }
    }
}
