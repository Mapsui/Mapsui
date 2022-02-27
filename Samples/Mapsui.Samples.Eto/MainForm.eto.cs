
namespace Mapsui.Samples.Eto
{
	using global::Eto.Drawing;
	using global::Eto.Forms;

	public partial class MainForm : Form
	{
		private void InitializeComponent()
		{
			this.Title = "My Eto Form";
			this.MinimumSize = new Size(200, 150);
			this.Size = this.MinimumSize * 4;
			this.Padding = 10;

			this.Content = new StackLayout
			{
				Items =
				{
					"Hello World!",
				},
			};

			var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += (sender, e) => Application.Instance.Quit();

			var aboutCommand = new Command { MenuText = "About..." };
			aboutCommand.Executed += (sender, e) => new AboutDialog().ShowDialog(this);

			// create menu
			this.Menu = new MenuBar
			{
				Items =
				{
					// File submenu0
				},
				ApplicationItems =
				{
					// application (OS X) or file menu (others)
					new ButtonMenuItem { Text = "&Preferences..." },
				},
				QuitItem = quitCommand,
				AboutItem = aboutCommand,
			};
		}
	}
}
