using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace display_time_remaining
{
	public partial class MainWindow : Window
	{
		DispatcherTimer _timer;
		int _intervalMS;
		TimeSpan _destTime;
		TimeSpan _startTime;
		bool IsSetting => xNameGridSettings.Visibility == Visibility.Visible;

		public MainWindow()
		{
			InitializeComponent();
			this.PreviewKeyDown += new KeyEventHandler((sender, e) => {
				if (e.Key == Key.Escape)
					Close();
			});
			this.Loaded += Init;
			this.MouseEnter += OnEnter;
			this.MouseLeave += OnLeave;
			this.MouseLeftButtonDown += (sender, e) => this.DragMove();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			xNameTextBoxDisplayformat.Text = Properties.Settings.Default.FormatTime;
			xNameTimePickerTargetTime.Value = new DateTime() + Properties.Settings.Default.DestTime;
			xNameIntegerUpDownInterval.Value = Properties.Settings.Default.Interval;
			var c = Properties.Settings.Default.ColorBackground;
			((SolidColorBrush)Background).Color = Color.FromArgb(c.A, c.R, c.G, c.B);
			c = Properties.Settings.Default.ColorText;
			((SolidColorBrush)xNameTextBlockTimerMain.Foreground).Color = Color.FromArgb(c.A, c.R, c.G, c.B);


		}

		private void Window_Closed(object sender, EventArgs e)
		{
			Properties.Settings.Default.FormatTime = xNameTextBoxDisplayformat.Text;
			Properties.Settings.Default.DestTime = xNameTimePickerTargetTime.Value.Value.TimeOfDay;
			Properties.Settings.Default.Interval = xNameIntegerUpDownInterval.Value.Value;
			var c = ((SolidColorBrush)Background).Color;
			Properties.Settings.Default.ColorBackground = System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
			c = ((SolidColorBrush)xNameTextBlockTimerMain.Foreground).Color;
			Properties.Settings.Default.ColorText = System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
			Properties.Settings.Default.Save();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			xNameGridSettings.Visibility = Visibility.Hidden;
			xNameStackPanelMain.Visibility = Visibility.Visible;
			_destTime = xNameTimePickerTargetTime.Value.Value.TimeOfDay;
			_intervalMS = xNameIntegerUpDownInterval.Value.Value;
			_timer = CreateTimer();
			_timer.Start();
			if (Background is SolidColorBrush sb) {
				var c = sb.Color;
				c.A = Math.Max(c.A, (byte)2);
				sb.Color = c;
			}

			ResizeMode = ResizeMode.CanResizeWithGrip;
		}

		DispatcherTimer CreateTimer()
		{
			var t = new DispatcherTimer(DispatcherPriority.SystemIdle);
			t.Interval = TimeSpan.FromMilliseconds(_intervalMS);
			TimeSpan now = DateTime.Now.TimeOfDay;
			_startTime = DateTime.Now.TimeOfDay;
			if (_destTime < now) {
				_destTime = _destTime.Add(TimeSpan.FromDays(1));
			}
			var time_of_day = DateTime.Now.TimeOfDay;
			t.Tick += (sender, e) => {
				var diff = _destTime - DateTime.Now.TimeOfDay;
				if (diff < TimeSpan.Zero) {
					diff = TimeSpan.Zero;
					_timer.Stop();
					xNameTextBlockTimerMain.Text = "FINISH";
					return;
				}
				xNameTextBlockTimerMain.Text = diff.ToString(xNameTextBoxDisplayformat.Text);
			};
			return t;
		}

		void Init(object sender, RoutedEventArgs e)
		{
			SetTrans(false);
			xNameGridInfo.Visibility = Visibility.Hidden;
			xNameTextBlockHelp.Text = @"Esc: Close window
S: Show settings
P: Pause
			";
		}

		void OnEnter(object sender, RoutedEventArgs e)
		{
			if (IsSetting)
				return;
			SetTrans(false);
			xNameGridInfo.Visibility = Visibility.Visible;
			xNameStackPanelMain.Visibility = Visibility.Hidden;
			_timer.Tick += OnTickInfo;
		}

		void OnLeave(object sender, RoutedEventArgs e)
		{
			if (IsSetting)
				return;
			SetTrans(true);
			xNameGridInfo.Visibility = Visibility.Hidden;
			xNameStackPanelMain.Visibility = Visibility.Visible;
			_timer.Tick -= OnTickInfo;
		}

		void OnTickInfo(object sender, EventArgs e)
		{
			var diff = _startTime - DateTime.Now.TimeOfDay;
			xNameTextBlockElpasedTime.Text = diff.ToString(xNameTextBoxDisplayformat.Text);
			xNameTextBlockRemainingTime.Text = xNameTextBlockTimerMain.Text;
		}

		void SetTrans(bool transparency)
		{
			if (transparency) {
				this.Opacity = 0.5;
			}
			else {
				this.Opacity = 1;
			}
		}
	}
}
