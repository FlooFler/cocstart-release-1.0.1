using NAudio.Wave;
using System.Reflection;
using Label = System.Windows.Forms.Label;
using Microsoft.Xna.Framework;
using f_x;
using System.Text.Json;
using MaterialSkin;
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;

namespace cocstart
{

    public partial class FormMain : Form
    {
        private TrackBar volumeSlider;

        private enum AppState { MainMenu, Settings, Themes, Stats }
        private AppState currentState = AppState.MainMenu;

        private List<Control> mainMenuControls = new List<Control>();

        public static Dictionary<int, Level> levels;

        private Size originalPlayButtonSize;
        private Size originalFreeModeButtonSize;
        private Size originalThemeButtonSize;
        private Size originalStatsButtonSize;
        private Size originalQuitSize;
        private Size originalGearSize;
        private Size originalToggleMusicSize;

        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;
        private bool isMusicMuted = false;

        private Panel overlayPanel;
        private Panel settingsPanel;
        private Panel themesPanel;
        private Panel statsPanel;
        private Panel levelPanel;

        private Panel infoPanel;
        private FlowLayoutPanel infoContentPanel;

        private Color Graph_color = Color.FromArgb(5, 100, 150);
        private Color Ball_color = Color.FromArgb(22, 225, 200);

        private string filePath = "game_stats.json";
        private GameStats stats = new GameStats();
        private Stopwatch playTimer = new Stopwatch();


        public FormMain()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.WindowState = FormWindowState.Normal;
            LoadStats();
            playTimer.Start();

            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;

            togglemusicon.Hide();
            this.FormClosing += Form1_FormClosing;

            title.Font = new Font("Bahnschrift", (int)(this.ClientSize.Height * 0.07), FontStyle.Bold);
            playbutton.Font = new Font("Microsoft JhengHei", (int)(this.ClientSize.Height * 0.04));
            playbutton.Size = new Size((int)(this.ClientSize.Width * 0.3), (int)(this.ClientSize.Height * 0.1));
            freemodebutton.Font = new Font("Microsoft JhengHei", (int)(this.ClientSize.Height * 0.03));
            freemodebutton.Size = new Size((int)(this.ClientSize.Width * 0.25), (int)(this.ClientSize.Height * 0.075));
            themebutton.Font = new Font("Microsoft JhengHei", (int)(this.ClientSize.Height * 0.03));
            themebutton.Size = new Size((int)(this.ClientSize.Width * 0.25), (int)(this.ClientSize.Height * 0.075));
            statsbutton.Font = new Font("Microsoft JhengHei", (int)(this.ClientSize.Height * 0.03));
            statsbutton.Size = new Size((int)(this.ClientSize.Width * 0.25), (int)(this.ClientSize.Height * 0.075));

            originalPlayButtonSize = playbutton.Size;
            originalFreeModeButtonSize = freemodebutton.Size;
            originalThemeButtonSize = themebutton.Size;
            originalStatsButtonSize = statsbutton.Size;
            originalQuitSize = quit.Size;
            originalGearSize = gear.Size;
            originalToggleMusicSize = togglemusicoff.Size;

            playbutton.Cursor = Cursors.Hand;
            freemodebutton.Cursor = Cursors.Hand;
            themebutton.Cursor = Cursors.Hand;
            statsbutton.Cursor = Cursors.Hand;
            quit.Cursor = Cursors.Hand;
            gear.Cursor = Cursors.Hand;
            togglemusicoff.Cursor = Cursors.Hand;
            togglemusicon.Cursor = Cursors.Hand;

            PositionControls();

            SetupHoverEffects();

            InitializeMenuSystem();

            InitializeMusicPlayer();

            InitializeInfoMenu();

            this.Resize += Form1_Resize;

            levels = new Dictionary<int, Level>();
            levels[0] = new Level
            {
                free_mode = true,
                Graph_color = Graph_color,
                Ball_color = Ball_color,
                stats = stats

            };
            levels[1] = new Level
            {
                level_id = 1,
                GenPositions = new List<Vector2> { new Vector2(10, 10) },
                StarPositions = new List<Vector2> { new Vector2(5, 5) },
                Graph_color = Graph_color,
                Ball_color = Ball_color,
                stats = stats
            };

            levels[2] = new Level
            {
                level_id = 2,
                GenPositions = new List<Vector2> { new Vector2(-3, 11) },
                StarPositions = new List<Vector2> { new Vector2(7, 6.5f), new Vector2(11, 4), new Vector2(3, 4) },
                Graph_color = Graph_color,
                Ball_color = Ball_color,
                stats = stats
            };

            levels[3] = new Level
            {
                level_id = 3,
                GenPositions = new List<Vector2> { new Vector2(-6, 8) },
                StarPositions = new List<Vector2> { new Vector2(4, 2f), new Vector2(-9, -1), new Vector2(-2, -5) },
                Graph_color = Graph_color,
                Ball_color = Ball_color,
                stats = stats
            };

            levels[4] = new Level
            {
                level_id = 4,
                GenPositions = new List<Vector2> { new Vector2(7, 11) },
                StarPositions = new List<Vector2> { new Vector2(-3, -3.5f), new Vector2(6, -2), new Vector2(0, 0), new Vector2(-3, 5) },
                Graph_color = Graph_color,
                Ball_color = Ball_color,
                stats = stats
            };

            levels[5] = new Level
            {
                level_id = 5,
                GenPositions = new List<Vector2> { new Vector2(-3, 14) },
                StarPositions = new List<Vector2> { new Vector2(0, -7), new Vector2(11, 5), new Vector2(7, -12) },
                Graph_color = Graph_color,
                Ball_color = Ball_color,
                stats = stats
            };

            levels[6] = new Level
            {
                level_id = 6,
                GenPositions = new List<Vector2> { new Vector2(7, 5) },
                StarPositions = new List<Vector2> { new Vector2(2, 1.5f), new Vector2(0, -4), new Vector2(-6, -3) },
                Graph_color = Graph_color,
                Ball_color = Ball_color,
                stats = stats
            };

            levels[7] = new Level
            {
                level_id = 7,
                GenPositions = new List<Vector2> { new Vector2(1, 4) },
                StarPositions = new List<Vector2> { new Vector2(-3, 0), new Vector2(-2, -1.5f), new Vector2(1, -3) },
                Graph_color = Graph_color,
                Ball_color = Ball_color,
                stats = stats
            };

            levels[8] = new Level
            {
                level_id = 8,
                GenPositions = new List<Vector2> { new Vector2(0, 5) },
                StarPositions = new List<Vector2> { new Vector2(-0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(-1.5f, 0.2f), new Vector2(1.5f, 0.2f), new Vector2(-2.5f, 0.2f) },
                Polygons = new List<List<Vector2>> { new List<Vector2> { new Vector2(0, 4), new Vector2(3, 1), new Vector2(-3, 1) } },
                Graph_color = Graph_color,
                Ball_color = Ball_color,
                stats = stats
            };
            levels[9] = new Level
            {
                level_id = 9,
                GenPositions = new List<Vector2> { new Vector2(1, 2) },
                StarPositions = new List<Vector2> { new Vector2(-3, -1.5f), new Vector2(-5, -4), new Vector2(-2, -8) },
                Graph_color = Graph_color,
                Ball_color = Ball_color,
                stats = stats
            };

            levels[10] = new Level
            {
                level_id = 10,
                GenPositions = new List<Vector2> { new Vector2(-12, 8), new Vector2(12, 8) },
                StarPositions = new List<Vector2> { new Vector2(-13, -16), new Vector2(-9, -6), new Vector2(-5, -16),
                                        new Vector2(12, -16), new Vector2(8, -6), new Vector2(4, -16),
                                        new Vector2(-7f, -10), new Vector2(6, -10)},
                Graph_color = Graph_color,
                Ball_color = Ball_color,
                stats = stats
            };

            levels[11] = new Level
            {
                level_id = 11,
                GenPositions = new List<Vector2> { new Vector2(0, 10) },
                StarPositions = new List<Vector2> { new Vector2(3.5f, -3), new Vector2(-5.5f, -4), new Vector2(-5.5f, 3) },
                Lines = new List<List<Vector2>> { new List<Vector2> { new Vector2(-1, 8), new Vector2(1, 5) } },
                Graph_color = Graph_color,
                Ball_color = Ball_color,
                stats = stats
            };

            levels[12] = new Level
            {
                level_id = 12,
                GenPositions = new List<Vector2> { new Vector2(-8, 10) },
                StarPositions = new List<Vector2> { new Vector2(1, 1), new Vector2(4, -4), new Vector2(-6, -6) },
                Lines = new List<List<Vector2>> { new List<Vector2> { new Vector2(-3, 2), new Vector2(-3, -4) } },
                Graph_color = Graph_color,
                Ball_color = Ball_color,
                stats = stats
            };

            levels[13] = new Level
            {
                level_id = 13,
                GenPositions = new List<Vector2> { new Vector2(0, 10) },
                StarPositions = new List<Vector2> { new Vector2(-3, -1), new Vector2(3, -1), new Vector2(0, -5) },
                Lines = new List<List<Vector2>> { new List<Vector2> { new Vector2(-3, 4), new Vector2(3, 2) } },
                Graph_color = Graph_color,
                Ball_color = Ball_color,
                stats = stats
            };

            levels[14] = new Level
            {
                level_id = 14,
                GenPositions = new List<Vector2> { new Vector2(-2, 12) },
                StarPositions = new List<Vector2> { new Vector2(3, -4), new Vector2(-4, -6), new Vector2(0, -5) },
                Lines = new List<List<Vector2>> { new List<Vector2> { new Vector2(0, 8), new Vector2(0, -7) } },
                Polygons = new List<List<Vector2>> { new List<Vector2> { new Vector2(-6, 6), new Vector2(-6, 0), new Vector2(0, 0), new Vector2(0, 8) } },
                Graph_color = Graph_color,
                Ball_color = Ball_color,
                stats = stats
            };

            levels[15] = new Level
            {
                level_id = 15,
                GenPositions = new List<Vector2> { new Vector2(12, 17) },
                StarPositions = new List<Vector2> { new Vector2(11.5f, 10), new Vector2(11, -5), new Vector2(-1, -6), new Vector2(-5, 9), new Vector2(-4, 5) },
                Lines = new List<List<Vector2>> { new List<Vector2> { new Vector2(0, 20), new Vector2(0, -6) },
                                                new List<Vector2> { new Vector2(0, 6), new Vector2(-6, 4) },
                                                new List<Vector2> { new Vector2(-1, -1), new Vector2(-1, -6) },
                                                new List<Vector2> { new Vector2(-8, 10), new Vector2(-2, 8) } }, 
                Graph_color = Graph_color,
                Ball_color = Ball_color,
                stats = stats
            };
        }

        static bool CheckSystemRequirements()
        {
            // Check .NET version (approximate)
            Version requiredDotNet = new Version(6, 0);
            Version currentDotNet = Environment.Version;
            if (currentDotNet < requiredDotNet)
            {
                MessageBox.Show($"This application requires .NET {requiredDotNet} or higher.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Check Windows version (Windows 10 = 10.0)
            Version win10 = new Version(10, 0);
            Version osVersion = Environment.OSVersion.Version;
            if (osVersion < win10)
            {
                MessageBox.Show("This application requires Windows 10 or higher.", "Unsupported OS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Check screen resolution
            if (Screen.PrimaryScreen.Bounds.Width < 1280 || Screen.PrimaryScreen.Bounds.Height < 720)
            {
                MessageBox.Show("Screen resolution must be at least 1280x720.", "Display Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Check available RAM (in MB)
            var memStatus = new ComputerInfo();
            ulong totalRamMB = memStatus.TotalPhysicalMemory / (1024 * 1024);
            if (totalRamMB < 4096)
            {
                MessageBox.Show("At least 4 GB of RAM is required.", "Insufficient Memory", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (infoPanel != null && infoPanel.Visible)
                {
                    infoPanel.Visible = false;
                }
                else if (overlayPanel.Visible)
                {
                    overlayPanel.Visible = false;
                    settingsPanel.Visible = false;
                    themesPanel.Visible = false;
                    statsPanel.Visible = false;
                    levelPanel.Visible = false;
                }
            }
        }

        private void InitializeMenuSystem()
        {
            CreateSettingsMenu();
            CreateThemesMenu();
            CreateStatsMenu();
            CreateLevelsMenu();

            mainMenuControls.AddRange(new Control[] {
                playbutton, freemodebutton, themebutton, statsbutton,
                quit, gear, togglemusicoff, togglemusicon, info,
                pictureBox5, pictureBox6, pictureBox7, pictureBox8
            });
        }

        private void InitializeInfoMenu()
        {
            infoPanel = new Panel();
            infoPanel.Dock = DockStyle.Fill;
            infoPanel.BackColor = Color.FromArgb(27, 34, 44);
            infoPanel.Visible = false;
            infoPanel.Paint += (s, e) =>
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(240, 255, 255)), 0, 0, infoPanel.Width, 40);
                using (var textBrush = new SolidBrush(Color.FromArgb(27, 34, 44)))
                {
                    e.Graphics.DrawString("Information",
                                        new Font("Arial", 12, FontStyle.Bold),
                                        textBrush,
                                        15, 10);
                }
            };
            this.Controls.Add(infoPanel);
            infoPanel.BringToFront();

            infoContentPanel = new FlowLayoutPanel();
            infoContentPanel.AutoScroll = true;
            infoContentPanel.FlowDirection = FlowDirection.TopDown;
            infoContentPanel.WrapContents = false;
            infoContentPanel.Location = new Point(10, 50);
            infoContentPanel.Size = new Size(infoPanel.Width - 20, infoPanel.Height - 60);
            infoContentPanel.BackColor = Color.FromArgb(27, 34, 44);
            infoPanel.Controls.Add(infoContentPanel);

            infoPanel.Resize += (s, e) =>
            {
                infoContentPanel.Size = new Size(infoPanel.Width - 20, infoPanel.Height - 60);
            };

            AddInfoContent();
        }

        private void AddInfoTitle(string text)
        {
            var titleLabel = new Label();
            titleLabel.Text = text;
            titleLabel.Font = new Font("Arial", 14, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(240, 255, 255);
            titleLabel.AutoSize = true;
            titleLabel.Margin = new Padding(10, 20, 10, 5);
            infoContentPanel.Controls.Add(titleLabel);
        }

        private void AddInfoContent()
        {

            AddInfoTitle("How to play:");
            AddInfoSection("",
        @"Write functions in the Enter Function box.
Click Start to generate the falling balls.
You have to write functions so that all the stars are collected after starting the ball generator .
The score you get is higher if you use less functions. Try to use as little as possible.");
            AddInfoSection("", "");

            AddInfoTitle("Controls:");
            AddInfoSection("",
        @"Esc – Exit Menus
Enter – Alternative to ”ADD FUNCTION” button
Left Click (hold) - Pan the camera
Scroll Wheel – Zoom in/out
You can edit a function by selecting it from the list, making the changes you want, then clicking ”ADD FUNCTION” or pressing Enter");
            AddInfoSection("", "");

            AddInfoTitle("Free Mode:");
            AddInfoSection("",
        @"Free mode is an editor, where you can experiment with all of the elements included in the game
Select an object by clicking on one of the buttons at the top of the screen, place them with right click. You can edit their size using the slider that appears after placing an object.
Polygons are built by placing vertices in order, filling in the area in between when you click ”FINISH POLYGON”. The lines work the same way, joining the points you place with right click after clicking ”FINISH LINE”");
            AddInfoSection("", "");

            AddInfoTitle("General knowledge (use x and y as variables):");

            AddInfoSection("Set bounds for variables:",
        @"After writing the mathematical expression of a function, add the following:
” {x<_, x>_, ...}”
Example: pow(x,2) {x<2, x>1}");

            AddInfoSection("",
        @"It works for x and y, as long as you put the variable first.
Example: 6*x + 2 {2<x} (WRONG)
Use the comparison signs ONE at a time.
Example: pow(x,2) {2<x<9} (WRONG)");


            AddInfoSection("Using x and y in the same function:",
        @"It helps create shapes that cannot be made with just 1 variable.
Example: pow(x,2) + pow(y,2) – 3 (creates a circle with radius sqrt(3) in the middle of the graph)");

            AddInfoTitle("Basic functions (use x and y as variables):");

            AddInfoSection("Power function",
        @"Writing: pow(_,_)
You raise the variable to the power of a constant
Example: pow(x,2)");

            AddInfoSection("Exponential function",
        @"Writing: pow(_,_)
It has the same notation as the power function, except that the constant comes first and the variable second
Example: pow(3,x)");

            AddInfoSection("Square root function",
        @"Writing: sqrt(_)
Takes the square root of a variable/constant/expression. Only works for positive values
Example: sqrt(x)");

            AddInfoSection("Logarithmic function",
        @"Writing: ln(_) (log in base e), log10(_) (log in base 10)
Takes the natural logarithm (or the base 10 logarithm) of a variable/constant/expression. Only works for positive values
Example: ln(x)");
            
            AddInfoSection("Trigonometric functions",
        @"Writing: sin(_), cos(_), tan(_), asin(_), acos(_), atan(_)
All basic trigonometric functions
Example: sin(x)");

            AddInfoSection("Minimum and maximum functions",
        @"Writing: min(_,_), max(_,_)
Takes the minimum/maximum value of 2 variables/expressions
Example: min(x,2*x)");

            AddInfoSection("Absolute value",
        @"Writing: abs(_)
Takes the absolute (positive) value of a variable/constant/expression
Example: abs(x)");
        }

        private void AddInfoSection(string header, string content)
        {
            Color textColor = Color.FromArgb(240, 255, 255);

            var sectionHeader = new Label();
            sectionHeader.Text = header;
            sectionHeader.Font = new Font("Arial", 14, FontStyle.Bold);
            sectionHeader.ForeColor = Color.FromArgb(240, 255, 255);
            sectionHeader.AutoSize = true;
            sectionHeader.Margin = new Padding(10, 20, 10, 5);
            infoContentPanel.Controls.Add(sectionHeader);

            var sectionContent = new Label();
            sectionContent.Text = content;
            sectionContent.Font = new Font("Arial", 11);
            sectionContent.ForeColor = Color.FromArgb(240, 255, 255);
            sectionContent.AutoSize = true;
            sectionContent.Margin = new Padding(20, 0, 10, 10);
            infoContentPanel.Controls.Add(sectionContent);
        }

        private void AddInfoImage(Image image)
        {
            var pictureBox = new PictureBox();
            pictureBox.Image = image;
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.Width = infoContentPanel.Width - 40;
            pictureBox.Height = (int)(image.Height * ((float)pictureBox.Width / image.Width));
            pictureBox.Margin = new Padding(20, 10, 20, 20);
            infoContentPanel.Controls.Add(pictureBox);
        }
        private void CreateSettingsMenu()
        {
            if (overlayPanel == null)
            {
                overlayPanel = new Panel();
                overlayPanel.Dock = DockStyle.Fill;
                overlayPanel.BackColor = Color.Black;
                overlayPanel.Visible = false;
                overlayPanel.Paint += (sender, e) =>
                {
                    using (var brush = new SolidBrush(Color.FromArgb(180, 27, 34, 44)))
                    {
                        e.Graphics.FillRectangle(brush, overlayPanel.ClientRectangle);
                    }
                };
                this.Controls.Add(overlayPanel);
                overlayPanel.BringToFront();
            }

            settingsPanel = new Panel();
            settingsPanel.Size = new Size(400, 350);
            settingsPanel.BackColor = Color.FromArgb(235, 40, 50, 60);
            settingsPanel.Paint += (sender, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(240, 255, 255), 2))
                {
                    e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, settingsPanel.Width - 1, settingsPanel.Height - 1));
                }
            };
            settingsPanel.Visible = false;
            this.Controls.Add(settingsPanel);
            settingsPanel.BringToFront();

            var titleLabel = new Label();
            titleLabel.Text = "SETTINGS";
            titleLabel.Font = new Font("Arial", 24, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(240, 255, 255);
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(
                (settingsPanel.Width - titleLabel.Width) / 2 - 50,
                30
            );
            settingsPanel.Controls.Add(titleLabel);

            var musicLabel = new Label();
            musicLabel.Text = "Music Volume";
            musicLabel.Font = new Font("Arial", 18, FontStyle.Bold);
            musicLabel.ForeColor = Color.FromArgb(240, 255, 255);
            musicLabel.AutoSize = true;
            musicLabel.Location = new Point(
                (settingsPanel.Width - musicLabel.Width) / 2 - 50,
                100
            );
            settingsPanel.Controls.Add(musicLabel);

            volumeSlider = new TrackBar();
            volumeSlider.Minimum = 0;
            volumeSlider.Maximum = 100;
            volumeSlider.Value = (int)((outputDevice?.Volume ?? 0.5f) * 100);
            volumeSlider.Size = new Size(300, 50);
            volumeSlider.BackColor = Color.FromArgb(240, 255, 255);
            volumeSlider.Location = new Point(
                (settingsPanel.Width - volumeSlider.Width) / 2,
                140
            );
            volumeSlider.ValueChanged += (s, e) =>
            {
                if (outputDevice != null)
                    outputDevice.Volume = volumeSlider.Value / 100f;
            };
            settingsPanel.Controls.Add(volumeSlider);

            var volumeLabel = new Label();
            volumeLabel.Text = $"{volumeSlider.Value}%";
            volumeLabel.ForeColor = Color.FromArgb(240, 255, 255);
            volumeLabel.BackColor = Color.Transparent;
            volumeLabel.AutoSize = true;
            volumeLabel.Location = new Point(
                volumeSlider.Right + 10,
                volumeSlider.Top + (volumeSlider.Height - volumeLabel.Height) / 2
            );
            settingsPanel.Controls.Add(volumeLabel);
            volumeSlider.ValueChanged += (s, e) => volumeLabel.Text = $"{volumeSlider.Value}%";
        }


        private void CreateThemesMenu()
        {
            themesPanel = new Panel();
            themesPanel.Size = new Size(600, 500);
            themesPanel.BackColor = Color.FromArgb(235, 40, 50, 60);
            themesPanel.Visible = false;
            this.Controls.Add(themesPanel);

            var titleLabel = new Label();
            titleLabel.Text = "THEMES";
            titleLabel.Font = new Font("Arial", 24, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(240, 255, 255);
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(
                (themesPanel.Width - titleLabel.Width) / 2 - 40,
                30
            );
            themesPanel.Controls.Add(titleLabel);

            var themes = new List<(string name, Color circleColor, Color lineColor)>
                          {
                    ("Classic", Color.FromArgb(50, 100, 150), Color.FromArgb(225, 225, 200)),
                    ("Dark", Color.FromArgb(30, 30, 30), Color.FromArgb(180, 180, 180)),
                    ("Ocean", Color.FromArgb(0, 85, 128), Color.FromArgb(153, 196, 210)),
                    ("Forest", Color.FromArgb(24, 119, 24), Color.FromArgb(132, 231, 132)),
                    ("Sunset", Color.FromArgb(235, 59, 0), Color.FromArgb(235, 195, 0)),
                    ("Royal", Color.FromArgb(55, 0, 110), Color.FromArgb(218, 110, 218))
                         };

            int buttonWidth = 150;
            int buttonHeight = 40;
            int previewSize = 80;
            int horizontalSpacing = 30;
            int verticalSpacing = 80;
            int themesPerRow = 3;

            int startX = (themesPanel.Width - (themesPerRow * buttonWidth + (themesPerRow - 1) * horizontalSpacing)) / 2;
            int startY = 100;

            Button selectedButton = null;
            Color panelBgColor = Color.FromArgb(240, 255, 255);

            for (int i = 0; i < themes.Count; i++)
            {
                var theme = themes[i];
                int row = i / themesPerRow;
                int col = i % themesPerRow;
                int xPos = startX + col * (buttonWidth + horizontalSpacing);
                int yPos = startY + row * (previewSize + buttonHeight + verticalSpacing);

                var previewPanel = new Panel();
                previewPanel.Size = new Size(previewSize, previewSize);
                previewPanel.Location = new Point(xPos + (buttonWidth - previewSize) / 2, yPos);
                previewPanel.BackColor = panelBgColor;
                previewPanel.Paint += (sender, e) =>
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillEllipse(new SolidBrush(theme.circleColor),
                        previewSize / 4, previewSize / 4, previewSize / 2, previewSize / 2);

                    using (var pen = new Pen(theme.lineColor, 4))
                    {
                        e.Graphics.DrawLine(pen, 10, previewSize - 10, previewSize - 10, 10);
                    }
                };
                themesPanel.Controls.Add(previewPanel);

                var themeButton = new Button();
                themeButton.Text = theme.name;
                themeButton.Tag = i;
                themeButton.Font = new Font("Arial", 10, FontStyle.Bold);
                themeButton.ForeColor = Color.FromArgb(27, 34, 44);
                themeButton.BackColor = panelBgColor;
                themeButton.FlatStyle = FlatStyle.Flat;
                themeButton.FlatAppearance.BorderSize = 2;
                themeButton.FlatAppearance.BorderColor = Color.FromArgb(36, 42, 55);
                themeButton.Size = new Size(buttonWidth, buttonHeight);
                themeButton.Location = new Point(xPos, yPos + previewSize + 10);

                int index = i;
                themeButton.Click += (sender, e) =>
                {
                    if (selectedButton != null)
                    {
                        selectedButton.BackColor = panelBgColor;
                        selectedButton.ForeColor = Color.FromArgb(27, 34, 44);
                    }

                    selectedButton = (Button)sender;
                    selectedButton.BackColor = Color.FromArgb(70, 130, 180);
                    selectedButton.ForeColor = Color.White;
                    ApplyTheme(index, themes);
                };

                themeButton.MouseEnter += (sender, e) =>
                {
                    if (sender != selectedButton)
                    {
                        themeButton.BackColor = Color.White;
                    }
                };

                themeButton.MouseLeave += (sender, e) =>
                {
                    if (sender != selectedButton)
                    {
                        themeButton.BackColor = panelBgColor;
                    }
                };

                themesPanel.Controls.Add(themeButton);
            }
        }

        private void ApplyTheme(int themeIndex, List<(string name, Color circleColor, Color lineColor)> themes)
        {
            var selectedTheme = themes[themeIndex];

            foreach (var levelPair in levels)
            {
                levelPair.Value.Graph_color = selectedTheme.lineColor;
                levelPair.Value.Ball_color = selectedTheme.circleColor;
            }
        }

        private void CreateStatsMenu()
        {
            statsPanel = new Panel();
            statsPanel.Size = new Size(500, 400);
            statsPanel.BackColor = Color.FromArgb(235, 40, 50, 60);
            statsPanel.Visible = false;
            this.Controls.Add(statsPanel);

            var titleLabel = new Label();
            titleLabel.Text = "STATISTICS";
            titleLabel.Font = new Font("Arial", 24, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(240, 255, 255);
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(
                (statsPanel.Width - titleLabel.Width) / 2 - 55,
                30
            );
            statsPanel.Controls.Add(titleLabel);

            int startY = 100;
            int spacing = 60;
            int leftLabelWidth = 200;
            int rightLabelWidth = 100;

            var starsLabel = new Label();
            starsLabel.Text = "Stars";
            starsLabel.Font = new Font("Arial", 16, FontStyle.Bold);
            starsLabel.ForeColor = Color.FromArgb(240, 255, 255);
            starsLabel.AutoSize = false;
            starsLabel.Size = new Size(leftLabelWidth, 30);
            starsLabel.Location = new Point(50, startY);
            statsPanel.Controls.Add(starsLabel);

            var starsValue = new Label();
            starsValue.Text = "0";
            starsValue.Font = new Font("Arial", 16, FontStyle.Regular);
            starsValue.ForeColor = Color.FromArgb(240, 255, 255);
            starsValue.AutoSize = false;
            starsValue.Size = new Size(rightLabelWidth, 30);
            starsValue.Location = new Point(statsPanel.Width - rightLabelWidth - 50, startY);
            statsPanel.Controls.Add(starsValue);

            var attemptsLabel = new Label();
            attemptsLabel.Text = "Attempts";
            attemptsLabel.Font = new Font("Arial", 16, FontStyle.Bold);
            attemptsLabel.ForeColor = Color.FromArgb(240, 255, 255);
            attemptsLabel.AutoSize = false;
            attemptsLabel.Size = new Size(leftLabelWidth, 30);
            attemptsLabel.Location = new Point(50, startY + spacing);
            statsPanel.Controls.Add(attemptsLabel);

            var attemptsValue = new Label();
            attemptsValue.Text = "0";
            attemptsValue.Font = new Font("Arial", 16, FontStyle.Regular);
            attemptsValue.ForeColor = Color.FromArgb(240, 255, 255);
            attemptsValue.AutoSize = false;
            attemptsValue.Size = new Size(rightLabelWidth, 30);
            attemptsValue.Location = new Point(statsPanel.Width - rightLabelWidth - 50, startY + spacing);
            statsPanel.Controls.Add(attemptsValue);

            var ballsLabel = new Label();
            ballsLabel.Text = "Balls";
            ballsLabel.Font = new Font("Arial", 16, FontStyle.Bold);
            ballsLabel.ForeColor = Color.FromArgb(240, 255, 255);
            ballsLabel.AutoSize = false;
            ballsLabel.Size = new Size(leftLabelWidth, 30);
            ballsLabel.Location = new Point(50, startY + spacing * 2);
            statsPanel.Controls.Add(ballsLabel);

            var ballsValue = new Label();
            ballsValue.Text = "0";
            ballsValue.Font = new Font("Arial", 16, FontStyle.Regular);
            ballsValue.ForeColor = Color.FromArgb(240, 255, 255);
            ballsValue.AutoSize = false;
            ballsValue.Size = new Size(rightLabelWidth, 30);
            ballsValue.Location = new Point(statsPanel.Width - rightLabelWidth - 50, startY + spacing * 2);
            statsPanel.Controls.Add(ballsValue);
        }

        private void CreateLevelsMenu()
        {
            levelPanel = new Panel();
            levelPanel.Size = new Size(800, 500);
            levelPanel.BackColor = Color.FromArgb(235, 40, 50, 60);
            levelPanel.Visible = false;
            this.Controls.Add(levelPanel);

            var titleLabel = new Label();
            titleLabel.Text = "LEVELS";
            titleLabel.Font = new Font("Arial", 24, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(240, 255, 255);
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(
                (levelPanel.Width - titleLabel.Width) / 2 - 33,
                30
            );
            levelPanel.Controls.Add(titleLabel);

            List<Button> levelButtons = new List<Button>();

            int buttonWidth = 120;
            int buttonHeight = 60;
            int horizontalSpacing = 20;
            int verticalSpacing = 20;
            int startX = (levelPanel.Width - (5 * buttonWidth + 4 * horizontalSpacing)) / 2;
            int startY = 100;

            for (int i = 1; i <= 15; i++)
            {
                bool isCompleted = stats.LevelsCompleted.Contains(i);

                Button levelButton = new Button();
                levelButton.Text = $"Level {i}";
                levelButton.Tag = i;
                levelButton.Font = new Font("Arial", 12, FontStyle.Bold);
                levelButton.FlatStyle = FlatStyle.Flat;
                levelButton.FlatAppearance.BorderSize = 2;
                levelButton.FlatAppearance.BorderColor = Color.FromArgb(36, 42, 55);
                levelButton.Size = new Size(buttonWidth, buttonHeight);

                UpdateButtonColors(levelButton, isCompleted, false);
                
                int row = (i - 1) / 5;
                int col = (i - 1) % 5;
                levelButton.Location = new Point(
                    startX + col * (buttonWidth + horizontalSpacing),
                    startY + row * (buttonHeight + verticalSpacing)
                );

                levelButton.Click += LevelButton_Click;
                levelPanel.Controls.Add(levelButton);
            }
        }
        private void UpdateButtonColors(Button button, bool isCompleted, bool isHovering)
        {
            if (isHovering)
            {
                button.BackColor = Color.White;
                button.ForeColor = Color.FromArgb(27, 34, 44);
            }
            else
            {
                button.BackColor = Color.FromArgb(240, 255, 255);
                button.ForeColor = Color.FromArgb(27, 34, 44);
            }
            if (isCompleted)
            {
                button.Font = new Font("Arial", 12, FontStyle.Bold);
                button.BackColor = Color.Green;
            }
            else
            {
                button.FlatAppearance.BorderColor = Color.FromArgb(36, 42, 55);
                button.Font = new Font("Arial", 12, FontStyle.Bold);
            }
        }
        private void LevelButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            int levelNumber = (int)clickedButton.Tag;

            overlayPanel.Visible = false;
            levelPanel.Visible = false;

            OpenGameWithLevel(levelNumber);
        }

        private void InitializeMusicPlayer()
        {
            try
            {
                outputDevice = new WaveOutEvent();

                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "cocstart.cocmusic.wav";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        throw new Exception("Could not find embedded music resource");

                    string tempFilePath = Path.Combine(Path.GetTempPath(), "cocmusic_temp.wav");
                    using (var fileStream = File.Create(tempFilePath))
                    {
                        stream.CopyTo(fileStream);
                    }

                    audioFile = new AudioFileReader(tempFilePath);
                    outputDevice.Init(audioFile);

                    outputDevice.Volume = 0.5f;

                    if (volumeSlider != null)
                    {
                        volumeSlider.Value = (int)(outputDevice.Volume * 100);
                    }

                    outputDevice.PlaybackStopped += (s, e) =>
                    {
                        try { File.Delete(tempFilePath); } catch { }
                    };
                }

                outputDevice.PlaybackStopped += (s, e) =>
                {
                    if (!isMusicMuted)
                    {
                        audioFile.Position = 0;
                        outputDevice.Play();
                    }
                };

                outputDevice.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Couldn't initialize audio: {ex.Message}");
            }
        }

        private void SetupHoverEffects()
        {
            const float scaleFactor = 1.1f;

            playbutton.MouseEnter += (sender, e) =>
            {
                playbutton.Size = new Size((int)(originalPlayButtonSize.Width * scaleFactor),
                                           (int)(originalPlayButtonSize.Height * scaleFactor));
                playbutton.Font = new Font(playbutton.Font.FontFamily,
                                           playbutton.Font.Size * scaleFactor,
                                           playbutton.Font.Style);
                playbutton.Location = new Point(
                    (this.ClientSize.Width - playbutton.Width) / 2,
                    (int)(this.ClientSize.Height * 0.4)
                );
            };

            playbutton.MouseLeave += (sender, e) =>
            {
                playbutton.Size = originalPlayButtonSize;
                playbutton.Font = new Font(playbutton.Font.FontFamily,
                                           playbutton.Font.Size / scaleFactor,
                                           playbutton.Font.Style);
                playbutton.Location = new Point(
                    (this.ClientSize.Width - playbutton.Width) / 2,
                    (int)(this.ClientSize.Height * 0.4)
                );
            };

            freemodebutton.MouseEnter += (sender, e) =>
            {
                freemodebutton.Size = new Size((int)(originalFreeModeButtonSize.Width * scaleFactor),
                                               (int)(originalFreeModeButtonSize.Height * scaleFactor));
                freemodebutton.Font = new Font(freemodebutton.Font.FontFamily,
                                               freemodebutton.Font.Size * scaleFactor,
                                               freemodebutton.Font.Style);
                freemodebutton.Location = new Point(
                    (this.ClientSize.Width - freemodebutton.Width) / 2,
                    (int)(this.ClientSize.Height * 0.6)
                );
            };

            freemodebutton.MouseLeave += (sender, e) =>
            {
                freemodebutton.Size = originalFreeModeButtonSize;
                freemodebutton.Font = new Font(freemodebutton.Font.FontFamily,
                                               freemodebutton.Font.Size / scaleFactor,
                                               freemodebutton.Font.Style);
                freemodebutton.Location = new Point(
                    (this.ClientSize.Width - freemodebutton.Width) / 2,
                    (int)(this.ClientSize.Height * 0.6)
                );
            };

            themebutton.MouseEnter += (sender, e) =>
            {
                themebutton.Size = new Size((int)(originalThemeButtonSize.Width * scaleFactor),
                                             (int)(originalThemeButtonSize.Height * scaleFactor));
                themebutton.Font = new Font(themebutton.Font.FontFamily,
                                             themebutton.Font.Size * scaleFactor,
                                             themebutton.Font.Style);
                themebutton.Location = new Point(
                    (this.ClientSize.Width - themebutton.Width) / 2,
                    (int)(this.ClientSize.Height * 0.7)
                );
            };

            themebutton.MouseLeave += (sender, e) =>
            {
                themebutton.Size = originalThemeButtonSize;
                themebutton.Font = new Font(themebutton.Font.FontFamily,
                                             themebutton.Font.Size / scaleFactor,
                                             themebutton.Font.Style);
                themebutton.Location = new Point(
                    (this.ClientSize.Width - themebutton.Width) / 2,
                    (int)(this.ClientSize.Height * 0.7)
                );
            };

            statsbutton.MouseEnter += (sender, e) =>
            {
                statsbutton.Size = new Size((int)(originalThemeButtonSize.Width * scaleFactor),
                                              (int)(originalThemeButtonSize.Height * scaleFactor));
                statsbutton.Font = new Font(statsbutton.Font.FontFamily,
                                              statsbutton.Font.Size * scaleFactor,
                                              statsbutton.Font.Style);
                statsbutton.Location = new Point(
                    (this.ClientSize.Width - statsbutton.Width) / 2,
                    (int)(this.ClientSize.Height * 0.8)
                );
            };

            statsbutton.MouseLeave += (sender, e) =>
            {
                statsbutton.Size = originalThemeButtonSize;
                statsbutton.Font = new Font(statsbutton.Font.FontFamily,
                                              statsbutton.Font.Size / scaleFactor,
                                              statsbutton.Font.Style);
                statsbutton.Location = new Point(
                    (this.ClientSize.Width - statsbutton.Width) / 2,
                    (int)(this.ClientSize.Height * 0.8)
                );
            };

            quit.MouseEnter += (sender, e) =>
            {
                quit.Size = new Size((int)(originalQuitSize.Width * scaleFactor),
                                    (int)(originalQuitSize.Height * scaleFactor));
                quit.Font = new Font(quit.Font.FontFamily,
                                    quit.Font.Size * scaleFactor,
                                    quit.Font.Style);
                quit.Location = new Point(
                    pictureBox7.Location.X + (pictureBox7.Width - quit.Width) / 2,
                    pictureBox7.Location.Y + (pictureBox7.Height - quit.Height) / 2
                );
            };
            quit.MouseLeave += (sender, e) =>
            {
                quit.Size = originalQuitSize;
                quit.Font = new Font(quit.Font.FontFamily,
                                    quit.Font.Size / scaleFactor,
                                    quit.Font.Style);
                quit.Location = new Point(
                    pictureBox7.Location.X + (pictureBox7.Width - quit.Width) / 2,
                    pictureBox7.Location.Y + (pictureBox7.Height - quit.Height) / 2
                );
            };

            gear.MouseEnter += (sender, e) =>
            {
                gear.Size = new Size((int)(originalGearSize.Width * scaleFactor),
                                    (int)(originalGearSize.Height * scaleFactor));
                gear.Location = new Point(
                    pictureBox5.Location.X + (pictureBox5.Width - gear.Width) / 2,
                    pictureBox5.Location.Y + (pictureBox5.Height - gear.Height) / 2
                );
            };
            gear.MouseLeave += (sender, e) =>
            {
                gear.Size = originalGearSize;
                gear.Location = new Point(
                    pictureBox5.Location.X + (pictureBox5.Width - gear.Width) / 2,
                    pictureBox5.Location.Y + (pictureBox5.Height - gear.Height) / 2
                );
            };

            togglemusicoff.MouseEnter += (sender, e) =>
            {
                togglemusicoff.Size = new Size((int)(originalToggleMusicSize.Width * scaleFactor),
                                          (int)(originalToggleMusicSize.Height * scaleFactor));
                togglemusicoff.Location = new Point(
                    pictureBox6.Location.X + (pictureBox6.Width - togglemusicoff.Width) / 2,
                    pictureBox6.Location.Y + (pictureBox6.Height - togglemusicoff.Height) / 2
                );
            };
            togglemusicoff.MouseLeave += (sender, e) =>
            {
                togglemusicoff.Size = originalToggleMusicSize;
                togglemusicoff.Location = new Point(
                    pictureBox6.Location.X + (pictureBox6.Width - togglemusicoff.Width) / 2,
                    pictureBox6.Location.Y + (pictureBox6.Height - togglemusicoff.Height) / 2
                );
            };

            togglemusicon.MouseEnter += (sender, e) =>
            {
                togglemusicon.Size = new Size((int)(originalToggleMusicSize.Width * scaleFactor),
                                          (int)(originalToggleMusicSize.Height * scaleFactor));
                togglemusicon.Location = new Point(
                    pictureBox6.Location.X + (pictureBox6.Width - togglemusicon.Width) / 2,
                    pictureBox6.Location.Y + (pictureBox6.Height - togglemusicon.Height) / 2
                );
            };
            togglemusicon.MouseLeave += (sender, e) =>
            {
                togglemusicon.Size = originalToggleMusicSize;
                togglemusicon.Location = new Point(
                    pictureBox6.Location.X + (pictureBox6.Width - togglemusicon.Width) / 2,
                    pictureBox6.Location.Y + (pictureBox6.Height - togglemusicon.Height) / 2
                );
            };

            info.MouseEnter += (sender, e) =>
            {
                info.Size = new Size((int)(originalToggleMusicSize.Width * scaleFactor),
                                          (int)(originalToggleMusicSize.Height * scaleFactor));
                info.Location = new Point(
                    pictureBox8.Location.X + (pictureBox8.Width - info.Width) / 2,
                    pictureBox8.Location.Y + (pictureBox8.Height - info.Height) / 2
                );
            };
            info.MouseLeave += (sender, e) =>
            {
                info.Size = originalToggleMusicSize;
                info.Location = new Point(
                    pictureBox8.Location.X + (pictureBox8.Width - info.Width) / 2,
                    pictureBox8.Location.Y + (pictureBox8.Height - info.Height) / 2
                );
            };
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            PositionControls();
        }

        private void PositionControls()
        {
            title.Font = new Font("Bahnschrift", (int)(this.ClientSize.Height * 0.07), FontStyle.Bold);
            playbutton.Font = new Font("Microsoft JhengHei", (int)(this.ClientSize.Height * 0.04));
            playbutton.Size = new Size((int)(this.ClientSize.Width * 0.3), (int)(this.ClientSize.Height * 0.1));
            freemodebutton.Font = new Font("Microsoft JhengHei", (int)(this.ClientSize.Height * 0.03));
            freemodebutton.Size = new Size((int)(this.ClientSize.Width * 0.25), (int)(this.ClientSize.Height * 0.075));
            themebutton.Font = new Font("Microsoft JhengHei", (int)(this.ClientSize.Height * 0.03));
            themebutton.Size = new Size((int)(this.ClientSize.Width * 0.25), (int)(this.ClientSize.Height * 0.075));
            statsbutton.Font = new Font("Microsoft JhengHei", (int)(this.ClientSize.Height * 0.03));
            statsbutton.Size = new Size((int)(this.ClientSize.Width * 0.25), (int)(this.ClientSize.Height * 0.075));

            originalPlayButtonSize = playbutton.Size;
            originalFreeModeButtonSize = freemodebutton.Size;
            originalThemeButtonSize = themebutton.Size;
            originalStatsButtonSize = statsbutton.Size;

            title.Location = new Point(
                (this.ClientSize.Width - title.Width) / 2,
                (int)(this.ClientSize.Height * 0.15)
            );

            playbutton.Location = new Point(
                (this.ClientSize.Width - playbutton.Width) / 2,
                (int)(this.ClientSize.Height * 0.4)
            );

            freemodebutton.Location = new Point(
                (this.ClientSize.Width - freemodebutton.Width) / 2,
                (int)(this.ClientSize.Height * 0.6)
            );

            themebutton.Location = new Point(
                (this.ClientSize.Width - themebutton.Width) / 2,
                (int)(this.ClientSize.Height * 0.7)
            );

            statsbutton.Location = new Point(
                (this.ClientSize.Width - statsbutton.Width) / 2,
                (int)(this.ClientSize.Height * 0.8)
            );


            Color textColor = Color.FromArgb(27, 34, 44);
            playbutton.ForeColor = textColor;
            freemodebutton.ForeColor = textColor;
            themebutton.ForeColor = textColor;
            statsbutton.ForeColor = textColor;

            pictureBox5.Location = new Point(10, this.ClientSize.Height - pictureBox5.Height - 10);
            pictureBox6.Location = new Point(this.ClientSize.Width - pictureBox6.Width - 10, this.ClientSize.Height - pictureBox6.Height - 10);
            pictureBox7.Location = new Point(this.ClientSize.Width - pictureBox7.Width - 10, 10);
            pictureBox8.Location = new Point(10, 10);

            quit.Location = new Point(
                pictureBox7.Location.X + (pictureBox7.Width - quit.Width) / 2,
                pictureBox7.Location.Y + (pictureBox7.Height - quit.Height) / 2
            );
            quit.ForeColor = Color.FromArgb(27, 34, 44);
            gear.Location = new Point(
                pictureBox5.Location.X + (pictureBox5.Width - quit.Width) / 2,
                pictureBox5.Location.Y + (pictureBox5.Height - quit.Height) / 2
            );
            togglemusicoff.Location = new Point(
                pictureBox6.Location.X + (pictureBox6.Width - togglemusicoff.Width) / 2,
                pictureBox6.Location.Y + (pictureBox6.Height - togglemusicoff.Height) / 2
            );
            togglemusicon.Location = new Point(
                pictureBox6.Location.X + (pictureBox6.Width - togglemusicon.Width) / 2,
                pictureBox6.Location.Y + (pictureBox6.Height - togglemusicon.Height) / 2
            );
            info.Location = new Point(
                pictureBox8.Location.X + (pictureBox8.Width - info.Width) / 2,
                pictureBox8.Location.Y + (pictureBox8.Height - info.Height) / 2
            );
            info.ForeColor = Color.FromArgb(27, 34, 44);
        }

        private void quit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape && currentState != AppState.MainMenu)
            {
                //ShowMainMenu();
            }
        }

        private void gear_Click(object sender, EventArgs e)
        {
            if (overlayPanel == null)
            {
                overlayPanel = new Panel();
                overlayPanel.Dock = DockStyle.Fill;
                overlayPanel.BackColor = Color.Black;
                overlayPanel.Visible = false;
                this.Controls.Add(overlayPanel);
                overlayPanel.BringToFront();
            }

            settingsPanel.Location = new Point(
                (this.ClientSize.Width - settingsPanel.Width) / 2,
                (this.ClientSize.Height - settingsPanel.Height) / 2
            );

            overlayPanel.Visible = true;
            settingsPanel.Visible = true;

            overlayPanel.BringToFront();
            settingsPanel.BringToFront();

        }

        private void togglemusic_Click(object sender, EventArgs e)
        {
            outputDevice.Volume = 0;
            isMusicMuted = true;
            togglemusicoff.Hide();
            togglemusicon.Show();
        }

        private void togglemusicon_Click(object sender, EventArgs e)
        {
            float volume = volumeSlider != null ? volumeSlider.Value / 100f : 0.5f;

            outputDevice.Volume = volume;
            isMusicMuted = false;

            if (outputDevice.PlaybackState != PlaybackState.Playing)
            {
                audioFile.Position = 0;
                outputDevice.Play();
            }

            togglemusicon.Hide();
            togglemusicoff.Show();
        }

        private void themebutton_Click(object sender, EventArgs e)
        {
            if (overlayPanel == null)
            {
                overlayPanel = new Panel();
                overlayPanel.Dock = DockStyle.Fill;
                overlayPanel.BackColor = Color.Black;
                overlayPanel.Visible = false;
                this.Controls.Add(overlayPanel);
                overlayPanel.BringToFront();
            }

            themesPanel.Location = new Point(
                (this.ClientSize.Width - themesPanel.Width) / 2,
                (this.ClientSize.Height - themesPanel.Height) / 2
            );

            overlayPanel.Visible = true;
            themesPanel.Visible = true;

            overlayPanel.BringToFront();
            themesPanel.BringToFront();
        }

        private void statsbutton_Click(object sender, EventArgs e)
        {
            if (overlayPanel == null)
            {
                overlayPanel = new Panel();
                overlayPanel.Dock = DockStyle.Fill;
                overlayPanel.BackColor = Color.FromArgb(180, 0, 0, 0);
                overlayPanel.Visible = false;
                this.Controls.Add(overlayPanel);
            }

            if (statsPanel == null)
            {
                statsPanel = new Panel();
                statsPanel.Size = new Size(300, 200);
                statsPanel.BackColor = Color.DarkSlateGray;
                statsPanel.BorderStyle = BorderStyle.FixedSingle;
                statsPanel.Visible = false;

                overlayPanel.Controls.Add(statsPanel);
            }

            statsPanel.Controls.Clear();

            Label titleLabel = new Label();
            titleLabel.Text = "Game Statistics";
            titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(10, 10);
            statsPanel.Controls.Add(titleLabel);

            Label levelsLabel = new Label();
            levelsLabel.Text = $"Levels completed: {string.Join(", ", stats.LevelsCompleted)}";
            levelsLabel.ForeColor = Color.White;
            levelsLabel.AutoSize = true;
            levelsLabel.Location = new Point(10, 50);
            statsPanel.Controls.Add(levelsLabel);

            Label starsLabel = new Label();
            starsLabel.Text = $"Stars collected: {stats.StarsCollected}";
            starsLabel.ForeColor = Color.White;
            starsLabel.AutoSize = true;
            starsLabel.Location = new Point(10, 80);
            statsPanel.Controls.Add(starsLabel);

            Label timeLabel = new Label();
            timeLabel.Text = $"Play time: {stats.TotalPlayTimeSeconds:F1} seconds";
            timeLabel.ForeColor = Color.White;
            timeLabel.AutoSize = true;
            timeLabel.Location = new Point(10, 110);
            statsPanel.Controls.Add(timeLabel);

            Button closeButton = new Button();
            closeButton.Text = "Delete save";
            closeButton.Location = new Point(10, 150);
            closeButton.Size = new Size(80, 30);
            closeButton.ForeColor = Color.FromArgb(27, 34, 44);
            closeButton.BackColor = Color.FromArgb(240, 255, 255);
            closeButton.Click += (s, args) =>
            {
                DeleteStats();
                Application.Restart();

            };
            statsPanel.Controls.Add(closeButton);

            statsPanel.Location = new Point(
                (this.ClientSize.Width - statsPanel.Width) / 2,
                (this.ClientSize.Height - statsPanel.Height) / 2
            );

            overlayPanel.Visible = true;
            statsPanel.Visible = true;

            overlayPanel.BringToFront();
            statsPanel.BringToFront();
        }


        private void playbutton_Click(object sender, EventArgs e)
        {
            if (overlayPanel == null)
            {
                overlayPanel = new Panel();
                overlayPanel.Dock = DockStyle.Fill;
                overlayPanel.BackColor = Color.Black;
                overlayPanel.Visible = false;
                this.Controls.Add(overlayPanel);
                overlayPanel.BringToFront();
            }

            levelPanel.Location = new Point(
                (this.ClientSize.Width - levelPanel.Width) / 2,
                (this.ClientSize.Height - levelPanel.Height) / 2
            );

            overlayPanel.Visible = true;
            levelPanel.Visible = true;

            overlayPanel.BringToFront();
            levelPanel.BringToFront();
        }

        private void info_Click(object sender, EventArgs e)
        {
            if (infoPanel == null)
            {
                InitializeInfoMenu();
            }

            infoPanel.Location = new Point(
                (this.ClientSize.Width - infoPanel.Width) / 2,
                (this.ClientSize.Height - infoPanel.Height) / 2
            );

            infoPanel.Visible = true;
            infoPanel.BringToFront();
        }

        public void OpenGameWithLevel(int levelIndex)
        {
            if (!levels.ContainsKey(levelIndex)) return;

            var level = levels[levelIndex];
            Game form1 = new Game(level);

            playTimer.Restart();

            this.Hide();
            form1.FormClosed += (s, e) =>
            {
                playTimer.Stop();

                if (form1.LevelCompleted)
                {
                    stats.LevelsCompleted.Add(levelIndex);
                }
                stats.StarsCollected += form1.StarsCollected; 

                SaveStats();
                this.Show();
            };
            form1.Show();
        }


        private void freemodebutton_Click(object sender, EventArgs e)
        {
            OpenGameWithLevel(0);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            playTimer.Stop();
            stats.TotalPlayTimeSeconds += (float)playTimer.Elapsed.TotalSeconds;
            SaveStats();
        }

        private void LoadStats()
        {
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    stats = JsonSerializer.Deserialize<GameStats>(json) ?? new GameStats();
                }
                catch
                {
                    stats = new GameStats(); // fallback
                }
            }
        }

        private void SaveStats()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(stats, options);
            File.WriteAllText(filePath, json);
        }

        private void DeleteStats()
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to delete save file:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            stats = new GameStats(); 
        }


    }
}

