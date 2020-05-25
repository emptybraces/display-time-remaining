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
		TimeSpan _startTimePause;
		TimeSpan _pauseTimeTotal;
		bool IsSetting => xNameGridSettings.Visibility == Visibility.Visible;
		bool IsPaused;

		public MainWindow()
		{
			InitializeComponent();
			xNameGridPause.Visibility = Visibility.Hidden;
			xNameViewboxPause.Visibility = Visibility.Hidden;
			this.PreviewKeyDown += new KeyEventHandler((sender, e) => {
				if (e.Key == Key.Escape)
					Close();
				else if (e.Key == Key.S && !IsSetting) {
					_timer.Tick -= OnTickMain;
					_timer.Tick -= OnTickInfo;
					_timer.Tick -= OnTickPause;
					xNameGridSettings.Visibility = Visibility.Visible;
					xNameStackPanelMain.Visibility = Visibility.Visible;
					xNameGridInfo.Visibility = Visibility.Hidden;
					xNameGridPause.Visibility = Visibility.Hidden;
					xNameViewboxPause.Visibility = Visibility.Hidden;
				}
				else if (e.Key == Key.P && !IsSetting) {
					IsPaused = !IsPaused;
					if (!IsPaused) {
						xNameGridPause.Visibility = Visibility.Hidden;
						xNameStackPanelMain.Visibility = Visibility.Visible;
						xNameViewboxPause.Visibility = Visibility.Hidden;
						_timer.Tick -= OnTickPause;
						_pauseTimeTotal += DateTime.Now.TimeOfDay - _startTimePause;
					}
					else {
						xNameGridPause.Visibility = Visibility.Visible;
						xNameStackPanelMain.Visibility = Visibility.Hidden;
						xNameViewboxPause.Visibility = Visibility.Visible;
						_startTimePause = DateTime.Now.TimeOfDay;
						_timer.Tick += OnTickPause;
					}
				}
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
			// リセット
			OnTickMain(null, null);
			OnTickInfo(null, null);
			_pauseTimeTotal = TimeSpan.Zero;
			_startTimePause = DateTime.Now.TimeOfDay;
			OnTickPause(null, null);
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
			t.Tick += OnTickMain;
			return t;
		}

		void Init(object sender, RoutedEventArgs e)
		{
			SetTrans(false);
			xNameGridInfo.Visibility = Visibility.Hidden;
			xNameTextBlockHelp.Text = @"'Esc': Close window
'S': Show settings
'P': Pause
			";
		}

		void OnEnter(object sender, RoutedEventArgs e)
		{
			if (IsSetting)
				return;
			SetTrans(false);
			xNameGridInfo.Visibility = Visibility.Visible;
			xNameGridPause.Visibility = Visibility.Hidden;
			xNameStackPanelMain.Visibility = Visibility.Hidden;
			_timer.Tick += OnTickInfo;
		}

		void OnLeave(object sender, RoutedEventArgs e)
		{
			if (IsSetting)
				return;
			SetTrans(true);
			xNameGridInfo.Visibility = Visibility.Hidden;
			if (IsPaused)
				xNameGridPause.Visibility = Visibility.Visible;
			else
				xNameStackPanelMain.Visibility = Visibility.Visible;
			_timer.Tick -= OnTickInfo;
		}

		void OnTickMain(object sender, EventArgs e)
		{
			var diff = _destTime - DateTime.Now.TimeOfDay;
			if (diff < TimeSpan.Zero) {
				_timer.Stop();
				xNameTextBlockTimerMain.Text = "FINISH";
				return;
			}
			xNameTextBlockTimerMain.Text = diff.ToString(xNameTextBoxDisplayformat.Text);
		}

		void OnTickInfo(object sender, EventArgs e)
		{
			var diff = _startTime - DateTime.Now.TimeOfDay;
			xNameTextBlockElapsedTime.Text = diff.ToString(xNameTextBoxDisplayformat.Text);
			xNameTextBlockRemainingTime.Text = xNameTextBlockTimerMain.Text;
		}

		void OnTickPause(object sender, EventArgs e)
		{
			var diff = DateTime.Now.TimeOfDay - _startTimePause + _pauseTimeTotal;
			xNameTextBlockTimerPause.Text = diff.ToString(xNameTextBoxDisplayformat.Text);
			xNameTextBlockRemainingTime2.Text = xNameTextBlockTimerMain.Text;
			xNameTextBlockPauseTime.Text = xNameTextBlockTimerPause.Text;
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
