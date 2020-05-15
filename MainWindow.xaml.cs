using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace display_time_remaining
{
	public partial class MainWindow : Window
	{
		DispatcherTimer timer;
		int intervalMS = 40;
		string displayFmt = @"hh\:mm\:ss\:ff";
		TimeSpan destTime;

		public MainWindow()
		{
			InitializeComponent();
			SetTrans(true);
			this.PreviewKeyDown += new KeyEventHandler((sender, e) => {
				if (e.Key == Key.Escape)
					Close();
			});
		}

		DispatcherTimer CreateTimer()
		{
			var t = new DispatcherTimer(DispatcherPriority.SystemIdle);
			t.Interval = TimeSpan.FromMilliseconds(intervalMS);
			TimeSpan now = DateTime.Now.TimeOfDay;
			if (destTime < now) {
				destTime = destTime.Add(TimeSpan.FromDays(1));
			}
			var time_of_day = DateTime.Now.TimeOfDay;
			var length_minutes = (float)destTime.TotalMinutes - time_of_day.TotalMinutes;
			var last_minutes = 0;
			t.Tick += (sender, e) => {
				var diff = destTime - DateTime.Now.TimeOfDay;
				if (last_minutes != diff.TotalMinutes) {
					last_minutes = diff.Minutes;
					firstOffset.Offset = 1 - diff.TotalMinutes / length_minutes;
				}
				textBlock.Text = diff.ToString(displayFmt);
			};
			return t;
		}

		private void Window_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			SetTrans(false);
		}

		private void Window_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			SetTrans(true);
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

		private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			this.DragMove();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			xNameGridSettings.Visibility = Visibility.Hidden;
			xNameStackPanelMain.Visibility = Visibility.Visible;
			destTime = xNameTimePickerTargetTime.Value.Value.TimeOfDay;
			displayFmt = xNameTextBoxDisplayformat.Text;
			intervalMS = xNameIntegerUpDownInterval.Value.Value;
			timer = CreateTimer();
			timer.Start();
			ResizeMode = ResizeMode.CanResizeWithGrip;
		}
	}
}
