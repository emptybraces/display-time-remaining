using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace display_time_remaining
{
	public partial class MainWindow : Window
	{
		DispatcherTimer _timer;
		int _intervalMS = 40;
		string _displayFmt = @"hh\:mm\:ss\:ff";
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

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			xNameGridSettings.Visibility = Visibility.Hidden;
			xNameStackPanelMain.Visibility = Visibility.Visible;
			_destTime = xNameTimePickerTargetTime.Value.Value.TimeOfDay;
			_displayFmt = xNameTextBoxDisplayformat.Text;
			_intervalMS = xNameIntegerUpDownInterval.Value.Value;
			_timer = CreateTimer();
			_timer.Start();
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
				xNameTextBlockTimerMain.Text = diff.ToString(_displayFmt);
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
			xNameTextBlockElpasedTime.Text = diff.ToString(_displayFmt);
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
