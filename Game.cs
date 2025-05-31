using System.Data;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision.Shapes;
using Microsoft.Xna.Framework;
using MaterialSkin;
using MaterialSkin.Controls;
using System.Text.RegularExpressions;
using DynamicExpresso;
using System.Reflection;
using cocstart;
using System.Globalization;
using FarseerPhysics.Common;





namespace f_x
{
    public partial class Game : MaterialForm
    {
        // Variabile pentru controlul camerei și zoom

        private PointF cameraPos = new PointF(0, 0);
        private float zoom = 100f;
        private const float AXIS_LIMIT = 20f;
        private Point lastMousePos;
        private bool isDragging = false;
        private bool labelVisibileX = false;
        private bool labelVisibileY = false;

        // Liste pentru obiectele din joc

        private List<star> stars = new List<star>();
        private List<StaticPolygon> staticPolygons = new List<StaticPolygon>();
        private List<Staticline> staticLine = new List<Staticline>();
        private Image star_im;
        private Image gen_im;
        private int star_count;
        private GameStats stats;
        private bool free_mode = false;
        private int level_id;
        
        private DoubleBufferedPanel canvas;

        private System.Windows.Forms.Timer refreshTimer;
        private Font gridFont = new Font("Segoe UI", 8);
        private Brush gridBrush = Brushes.Black;
        private Color Graph_color = Color.FromArgb(50, 100, 150);
        private Color Ball_color = Color.FromArgb(225, 225, 200);

        // Structuri pentru stocarea graficelor

        private List<List<PointF[]>> PointList = new List<List<PointF[]>>();

        // Sisteme fizice și obiecte

        private List<BallGenerator> generators = new List<BallGenerator>();
        private List<Body> balls = new List<Body>();
        private World world = new World(new Vector2(0, -98f));
        private List<List<Body>> graphBodies = new List<List<Body>>();

        // Control pentru funcții matematice

        private readonly System.Windows.Forms.Timer typingTimer;
        private List<Func<float, float, float>> functions = new List<Func<float, float, float>>();
        private List<string> functionExpressions = new List<string>();
        private MaterialListView listViewFunctions;
        private MaterialTextBox2 textBox1;

        public Game()
        {
            InitializeComponent();
            this.Text = "Call of Coordinates";

            
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = true;

            // Inițializare panou de desenare

            canvas = new DoubleBufferedPanel();
            canvas.Dock = DockStyle.Fill;
            this.Controls.Add(canvas);

            // Atasare evenimente panou

            canvas.Paint += Canvas_Paint;
            canvas.MouseDown += Canvas_MouseDown;
            canvas.MouseMove += Canvas_MouseMove;
            canvas.MouseUp += Canvas_MouseUp;
            canvas.MouseWheel += Canvas_MouseWheel;
            canvas.SendToBack();

            this.KeyPreview = true;
            this.Width = 800;
            this.Height = 600;

            refreshTimer = new System.Windows.Forms.Timer { Interval = 16 }; // ~60 FPS
            refreshTimer.Tick += (_, __) => canvas.Invalidate();
            refreshTimer.Tick += tick;
            refreshTimer.Start();

            // Încărcare resurse grafice din assembly

            gen_im = LoadEmbeddedImage("cocstart.Resources.generator.png");
            star_im = LoadEmbeddedImage("cocstart.Resources.star1.png");

            // Configurare temă Material UI

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Grey900, Primary.Grey800, Primary.Grey500, Accent.LightBlue200, TextShade.WHITE);

            // Inițializare sistem de introducere funcții

            typingTimer = new System.Windows.Forms.Timer();
            typingTimer.Interval = 150;
            typingTimer.Tick += TypingTimer_Tick;

            textBox1 = new MaterialTextBox2
            {
                Hint = "Enter function (e.g., sin(x), sqrt(x), etc.)",
                Location = new Point(10, 70),
                Width = 300
            };
            textBox1.TextChanged += TextBox1_TextChanged;
            this.Controls.Add(textBox1);
            textBox1.BringToFront();

            MaterialButton buttonAdd = new MaterialButton
            {
                Text = "Add Function",
                Location = new Point(320, 70),
                Width = 150
            };
            buttonAdd.Click += buttonAddFunction_Click;
            this.Controls.Add(buttonAdd);
            buttonAdd.BringToFront();


            MaterialButton buttonBack = new MaterialButton
            {
                Text = "Back",
                Location = new Point(1850, 70),
                Width = 150
            };
            buttonBack.Click += buttonBackFunction_Click;
            this.Controls.Add(buttonBack);
            buttonBack.BringToFront();


            MaterialButton buttonStart = new MaterialButton
            {
                Text = "Start",
                Location = new Point(10, 340),
                Width = 150
            };
            buttonStart.Click += buttonStart_Click;
            this.Controls.Add(buttonStart);
            buttonStart.BringToFront();



            MaterialButton buttonClearAll = new MaterialButton
            {
                Text = "Clear All",
                Location = new Point(320, 130),
                Width = 150
            };
            buttonClearAll.Click += buttonClearAll_Click;
            this.Controls.Add(buttonClearAll);
            buttonClearAll.BringToFront();

            MaterialButton buttonDeleteFunction = new MaterialButton
            {
                Text = "Delete Function",
                Location = new Point(320, 190),
                Width = 150
            };
            buttonDeleteFunction.Click += buttonDeleteFunction_Click;
            this.Controls.Add(buttonDeleteFunction);
            buttonDeleteFunction.BringToFront();

            listViewFunctions = new MaterialListView
            {
                FullRowSelect = true,
                MultiSelect = false,
                HideSelection = false,
                View = View.Details,
                BorderStyle = BorderStyle.None,
                Width = 300,
                HeaderStyle = ColumnHeaderStyle.None,
                Location = new Point(10, 130), // adjust as needed
                Height = 200,                  // adjust as needed
            };

            listViewFunctions.Columns.Add("Functions", -2, HorizontalAlignment.Left);
            listViewFunctions.SelectedIndexChanged += ListViewFunctions_SelectedIndexChanged;
            this.Controls.Add(listViewFunctions);
            listViewFunctions.BringToFront();

            this.KeyPreview = true;
            this.KeyDown += textBoxFunction_KeyDown;
        }

        // Încărcare imagini din resursele assembly-ului
        public Image LoadEmbeddedImage(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new Exception("Resursa nu a fost găsită: " + resourceName);
                return Image.FromStream(stream);
            }
        }

        // Randare principală pe canvas

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int w = canvas.Width;
            int h = canvas.Height;

            // Calcule viewport

            float left = cameraPos.X - (canvas.Width / 2f) / zoom;
            float right = cameraPos.X + (canvas.Width / 2f) / zoom;
            float top = cameraPos.Y + (h / 2f) / zoom;
            float bottom = cameraPos.Y - (h / 2f) / zoom;

            g.TranslateTransform(w / 2, h / 2);
            g.ScaleTransform(zoom, -zoom);
            g.TranslateTransform(-cameraPos.X, -cameraPos.Y);
            DrawGrid(g, w, h);
            g.ResetTransform();

            using (var pen = new Pen(Color.Black, 1f))
            {
                float screenY = WorldToScreen(new PointF(0, 0)).Y;
                if (screenY >= 0 && screenY <= h)
                {
                    float x1 = WorldToScreen(new PointF(left, 0)).X;
                    float x2 = WorldToScreen(new PointF(right, 0)).X;
                    g.DrawLine(pen, x1, screenY, x2, screenY);
                }
                else
                {
                    float y = screenY < 0 ? 0 : h;
                    g.DrawLine(pen, 0, y, w, y);
                }

                float screenX = WorldToScreen(new PointF(0, 0)).X;
                if (screenX >= 0 && screenX <= w)
                {
                    float y1 = WorldToScreen(new PointF(0, bottom)).Y;
                    float y2 = WorldToScreen(new PointF(0, top)).Y;
                    g.DrawLine(pen, screenX, y1, screenX, y2);
                }
                else
                {
                    float x = screenX < 0 ? 0 : w;
                    g.DrawLine(pen, x, 0, x, h);
                }
            }

            DrawRelocatedAxisLabels(g, w, h, left, right, top, bottom);

            // Aplicare transformări pentru lumea virtuală

            g.TranslateTransform(canvas.Width / 2, canvas.Height / 2);
            g.ScaleTransform(zoom, -zoom);
            g.TranslateTransform(-cameraPos.X, -cameraPos.Y);

            // Desenare grafice funcții

            using (Pen pen = new Pen(Graph_color, 1f / zoom))
            {
                foreach (var points in PointList)
                {
                    foreach (var segment in points)
                    {
                        if (segment.Length > 1)
                            g.DrawLines(pen, segment);
                    }
                }
            }

            // Desenare bile fizice

            using (Brush brush = new SolidBrush(Ball_color))
            {
                foreach (var ball in balls)
                {
                    if (ball.FixtureList.Count > 0 && ball.FixtureList[0].Shape is CircleShape circle)
                    {
                        float radius = circle.Radius;
                        float x = ball.Position.X - radius;
                        float y = ball.Position.Y - radius;
                        float diameter = radius * 2;

                        g.FillEllipse(brush, x, y, diameter, diameter);
                    }
                }
            }
            foreach (var star in stars)
                star.tick(g);
            foreach (var gen in generators)
                gen.tick(g);
            foreach (var obj in staticPolygons)
                obj.Tick(g);
            foreach (var obj in staticLine)
                obj.Tick(g);


        }

        // Logica jocului pe frame
        private void tick(object sender, EventArgs e)
        {

            // Actualizare generatoare

            foreach (var gen in generators)
            {
                gen.Update();
            }

            int star_count = 0;
            foreach(var star in stars)
            {
                if (star.isActive == true)
                    star_count++;
            }

            // Verificare condiție victorie

            if (star_count == 0 && free_mode == false)
            {
                ShowWinScreen();
            }
            world.Step(1f / 60f);
        }

        // Construire coliziuni pentru grafice
        private void RebuildGraphCollisions(int index)
        {
            if (index < 0 || index >= graphBodies.Count)
                return;

            // Șterge vechile corpuri
            foreach (var body in graphBodies[index])
                world.RemoveBody(body);
            graphBodies[index].Clear();

            // Verifică dacă există segmente valide
            if (index >= PointList.Count || PointList[index].Count == 0)
                return;

            foreach (var segment in PointList[index])
            {
                if (segment.Length < 2)
                    continue;

                // Crează vertices - optimizare posibilă dacă ai deja Vector2
                Vertices verts = new Vertices(segment.Length);
                foreach (var pt in segment)
                    verts.Add(new Vector2(pt.X, pt.Y));

                // Creează corpul static
                Body body = BodyFactory.CreateChainShape(world, verts);
                body.BodyType = BodyType.Static;
                body.CollisionCategories = Category.Cat2;

                // Setează proprietățile fixture-ului (acces primul fixture)
                if (body.FixtureList.Count > 0)
                {
                    Fixture f = body.FixtureList[0];
                    f.Friction = 0f;
                    f.Restitution = 0f;
                }

                graphBodies[index].Add(body);
            }
        }

        // Generare puncte pentru funcții implicite

        private void GeneratePoints(Func<float, float, float> implicitFunc, int index,  float Xmin,  float Xmax, float Ymin,  float Ymax)
        {
            GenerateFunctionPointsDynamic(implicitFunc, zoom, cameraPos, index,  Xmin,  Xmax,  Ymin,  Ymax);
            RebuildGraphCollisions(index); 
        }

        // Algoritm de generare a contururilor (Marching Squares)

        private void GenerateFunctionPointsDynamic(Func<float, float, float> implicitFunc, float zoom, PointF cameraPosition, int index, float minX, float maxX,  float minY,  float maxY)
        {
            int resolution = 1000;  

            float stepX = (maxX - minX) / (resolution - 1);
            float stepY = (maxY - minY) / (resolution - 1);

            // Creare grid de valori

            float[,] grid = new float[resolution, resolution];
            for (int i = 0; i < resolution; i++)
            {
                float x = minX + i * stepX;
                for (int j = 0; j < resolution; j++)
                {
                    float y = minY + j * stepY;
                    grid[i, j] = implicitFunc(x, y);
                }
            }

            // Generare contururi cu algoritmul Marching Squares

            var contours = MarchingSquares.GenerateContours(grid, 0f, minX, minY, stepX, stepY);

            // Adăugare contururi în lista de puncte

            foreach (var contour in contours)
            {
                PointList[index].Add(contour);
            }
        }

        private void DrawRelocatedAxisLabels(Graphics g, int w, int h, float left, float right, float top, float bottom)
        {
            float step = GetGridStep();
            int precision = Math.Max(0, (int)Math.Ceiling(-Math.Log10(step)) + 1);
            precision = Math.Min(precision, 10);
            string fmt = precision > 0 ? "0." + new string('#', precision) : "0";

            float originScreenY = WorldToScreen(new PointF(0, 0)).Y;
            bool drawTop = originScreenY < 0;
            bool drawBottom = originScreenY > h;
            float yPos = drawTop ? 5 : (drawBottom ? h - 20 : float.NaN);

            float originScreenX = WorldToScreen(new PointF(0, 0)).X;
            bool drawLeft = originScreenX < 0;
            bool drawRight = originScreenX > w;
            float xPos = drawLeft ? 5 : (drawRight ? w - 30 : float.NaN);

            if (drawTop || drawBottom)
            {
                labelVisibileX = true;
                for (float x = (float)Math.Floor(left / step) * step; x <= right; x += step)
                {
                    PointF sp = new PointF(WorldToScreen(new PointF(x, 0)).X, yPos);
                    string label = Math.Abs(x % 1) < 1e-8 ? ((int)x).ToString() : x.ToString(fmt);
                    SizeF size = g.MeasureString(label, gridFont);
                    using (Brush backgroundBrush = new SolidBrush(this.BackColor))
                    {
                        g.FillRectangle(backgroundBrush, sp.X - 10, sp.Y, size.Width, size.Height);
                    }
                    g.DrawString(label, gridFont, gridBrush, sp.X - 10, sp.Y);
                }
            }
            else
            {
                labelVisibileX = false;
            }

            if (drawLeft || drawRight)
            {
                labelVisibileY = true;
                for (float y = (float)Math.Floor(bottom / step) * step; y <= top; y += step)
                {
                    if (Math.Abs(y) < 1e-6f) continue;
                    PointF sp = new PointF(xPos, WorldToScreen(new PointF(0, y)).Y);
                    string label = Math.Abs(y % 1) < 1e-8 ? ((int)y).ToString() : y.ToString(fmt);
                    SizeF size = g.MeasureString(label, gridFont);
                    using (Brush backgroundBrush = new SolidBrush(this.BackColor))
                    {
                        g.FillRectangle(backgroundBrush, sp.X, sp.Y - 7, size.Width, size.Height);
                    }
                    g.DrawString(label, gridFont, gridBrush, sp.X, sp.Y - 7);
                }
            }
            else
            {
                labelVisibileY = false;
            }
        }


        private void DrawGrid(Graphics g, int w, int h)
        {
            float step = GetGridStep();

            float left = cameraPos.X - (w / 2f) / zoom;
            float right = cameraPos.X + (w / 2f) / zoom;
            float top = cameraPos.Y + (h / 2f) / zoom;
            float bottom = cameraPos.Y - (h / 2f) / zoom;

            if (step * zoom < 20f)
                DrawMajorGridOnly(g, left, right, top, bottom, step * 5);
            else
                DrawFullGrid(g, left, right, top, bottom, step);

            List<PointF> labelX = new List<PointF>();
            List<PointF> labelY = new List<PointF>();

            for (float x = (float)Math.Floor(left / step) * step; x <= right; x += step)
                labelX.Add(new PointF(x, 0));
            for (float y = (float)Math.Floor(bottom / step) * step; y <= top; y += step)
                if (Math.Abs(y) > 1e-6f) labelY.Add(new PointF(0, y));

            int precision = Math.Max(0, (int)Math.Ceiling(-Math.Log10(step)) + 1);
            precision = Math.Min(precision, 10);

            g.ResetTransform();
            if (labelVisibileX == false)
                foreach (var p in labelX)
                {
                    PointF sp = WorldToScreen(p);
                    if (sp.X >= 0 && sp.X <= w && sp.Y >= 0 && sp.Y <= h) // draw only visible labels
                    {
                        string label = Math.Abs(p.X % 1) < 1e-8 ? ((int)p.X).ToString() : p.X.ToString("0." + new string('#', precision));
                        g.DrawString(label, gridFont, gridBrush, sp.X - 10, sp.Y + 5);
                    }
                }
            if (labelVisibileY == false)
                foreach (var p in labelY)
                {
                    PointF sp = WorldToScreen(p);
                    if (sp.X >= 0 && sp.X <= w && sp.Y >= 0 && sp.Y <= h) // draw only visible labels
                    {
                        string label = Math.Abs(p.Y % 1) < 1e-8 ? ((int)p.Y).ToString() : p.Y.ToString("0." + new string('#', precision));
                        g.DrawString(label, gridFont, gridBrush, sp.X + 5, sp.Y - 7);
                    }
                }

            g.TranslateTransform(w / 2, h / 2);
            g.ScaleTransform(zoom, -zoom);
            g.TranslateTransform(-cameraPos.X, -cameraPos.Y);
        }

        private void DrawFullGrid(Graphics g, float left, float right, float top, float bottom, float step)
        {
            float penWidth = 1f / zoom;
            Pen minorPen = new Pen(Color.LightGray, penWidth);

            for (float x = (float)Math.Floor(left / step) * step; x <= right; x += step)
                g.DrawLine(minorPen, x, bottom, x, top);
            for (float y = (float)Math.Floor(bottom / step) * step; y <= top; y += step)
                g.DrawLine(minorPen, left, y, right, y);
        }

        private void DrawMajorGridOnly(Graphics g, float left, float right, float top, float bottom, float majorStep)
        {
            float penWidth = Math.Min(2f / zoom, 2f);
            Pen majorPen = new Pen(Color.Gray, penWidth);

            for (float x = (float)Math.Floor(left / majorStep) * majorStep; x <= right; x += majorStep)
                g.DrawLine(majorPen, x, bottom, x, top);
        }



        private float GetGridStep()
        {
            float targetPx = 40f;
            float idealWorld = targetPx / zoom;
            float exp = (float)Math.Pow(10, Math.Floor(Math.Log10(idealWorld)));
            float[] nice = { 1f, 2f, 5f };

            foreach (float m in nice)
            {
                float step = exp * m;
                if (step >= idealWorld)
                    return step;
            }
            return exp * 10f;
        }

        private void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                lastMousePos = e.Location;
            }
            else if (e.Button == MouseButtons.Right && free_mode == true)
            {
                var worldPos = ScreenToWorld(e.Location);
                generators.Add(new BallGenerator(world, new Vector2(worldPos.X, worldPos.Y),1,  gen_im, balls));
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                float dx = (e.X - lastMousePos.X) / zoom;
                float dy = (e.Y - lastMousePos.Y) / zoom;

                // Calculăm noile poziții cu limitarea la ±500
                float newX = cameraPos.X - dx;
                float newY = cameraPos.Y + dy;

                // Aplicăm limitele pe axa X (-500, 500)
                cameraPos.X = Math.Max(-AXIS_LIMIT, Math.Min(AXIS_LIMIT, newX));
               

                // Aplicăm limitele pe axa Y (-500, 500)
                cameraPos.Y = Math.Max(-AXIS_LIMIT, Math.Min(AXIS_LIMIT, newY));

                lastMousePos = e.Location;
                canvas.Invalidate();
            }
        }

        private void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                isDragging = false;
        }

        private void Canvas_MouseWheel(object sender, MouseEventArgs e)
        {
            float zoomFactor = 1.1f;
            float oldZoom = zoom;

            if (e.Delta > 0)
                zoom *= zoomFactor;
            else
                zoom /= zoomFactor;

            zoom = Math.Max(zoom, 30f);
            zoom = Math.Min(zoom, 100f);

            PointF before = ScreenToWorld(e.Location);
            PointF after = ScreenToWorld(e.Location);
            cameraPos.X += before.X - after.X;
            cameraPos.Y += before.Y - after.Y;

            canvas.Invalidate();
        }
        private PointF WorldToScreen(PointF world)
        {
            float x = (world.X - cameraPos.X) * zoom + canvas.Width / 2f;
            float y = -(world.Y - cameraPos.Y) * zoom + canvas.Height / 2f;
            return new PointF(x, y);
        }

        private PointF ScreenToWorld(Point screen)
        {
            float x = (screen.X - canvas.Width / 2f) / zoom + cameraPos.X;
            float y = -(screen.Y - canvas.Height / 2f) / zoom + cameraPos.Y;
            return new PointF(x, y);
        }
       

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            typingTimer.Stop();
            typingTimer.Start();
        }

        private void TypingTimer_Tick(object sender, EventArgs e)
        {
            typingTimer.Stop();
           
        }

       


        
        


        private void buttonAddFunction_Click(object sender, EventArgs e)
        {
            AddFunc();

        }
        private void textBoxFunction_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {

                AddFunc();
            }
        }

        // Logica pentru adăugarea funcțiilor
        private void AddFunc()
        {
            bool implicit_function;
            string expression = textBox1.Text.Trim();
            if (string.IsNullOrWhiteSpace(expression))
                return;

            try
            {
                Func<float,float,float> func;
                float Xmin, Xmax, Ymin, Ymax;

                // Compilare funcție cu domeniu

                if (ContainsY(expression))
                {
                    implicit_function = true;
                    func = CompileImplicit(expression,out Xmin, out Xmax, out Ymin, out Ymax);
                }
                else
                {
                    implicit_function = false;
                    var result = SplitExpressionWithBrackets(expression);
                    func = CompileImplicit(result.Expression + " - y" + result.Conditions,out Xmin, out Xmax, out Ymin, out Ymax);
                }

                // Corecție domenii invalide

                if (float.IsInfinity(Xmin) || float.IsNaN(Xmin) ||
                    float.IsInfinity(Xmax) || float.IsNaN(Xmax))
                {
                    Xmin = -55f;
                    Xmax = 55f;
                }

                // Dacă Ymin/Ymax sunt infinite sau NaN, atunci le fixăm:

                if (float.IsInfinity(Ymin) || float.IsNaN(Ymin) ||
                    float.IsInfinity(Ymax) || float.IsNaN(Ymax))
                {
                    Ymin = -40f;
                    Ymax = 40f;
                }
                if (Xmin < -55f)
                    Xmin = -55f;
                if (Xmax > +55f)
                    Xmax = 55f;
                if (Ymin < -40f)
                    Ymin = -40f;
                if (Ymax > +40f)
                    Ymax = 40f;

                // Editare existent sau adăugare nouă

                int selectedIndex = listViewFunctions.SelectedIndices.Count > 0 ? listViewFunctions.SelectedIndices[0] : -1;

                if (selectedIndex >= 0)
                {
                    // Actualizare funcție existentă

                    functions[selectedIndex] = func;
                    functionExpressions[selectedIndex] = expression;
                    if (implicit_function)
                    {
                        listViewFunctions.Items[selectedIndex].Text = $"f{selectedIndex + 1}(x,y) = {expression}";
                    }
                    else
                    {
                        listViewFunctions.Items[selectedIndex].Text = $"f{selectedIndex + 1}(x) = {expression}";
                    }
                    ClearList(selectedIndex);
                    GeneratePoints(func, selectedIndex,  Xmin,  Xmax, Ymin, Ymax);
                }
                else
                {
                    // Adăugare funcție nouă

                    functions.Add(func);
                    functionExpressions.Add(expression);
                    int functionIndex = listViewFunctions.Items.Count + 1;
                    if(implicit_function)
                    {
                        listViewFunctions.Items.Add(new ListViewItem($"f{functionIndex}(x,y) = {expression}"));
                    }
                    else
                    {

                        listViewFunctions.Items.Add(new ListViewItem($"f{functionIndex}(x) = {expression}"));
                    }

                    // Inițializare structuri date

                    PointList.Add(new List<PointF[]>());
                    graphBodies.Add(new List<Body>());
                    GeneratePoints(func, functions.Count - 1,  Xmin,  Xmax, Ymin,  Ymax);
                }

                textBox1.Clear();
                listViewFunctions.SelectedIndices.Clear(); // Clear selection after edit
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
        
        private void buttonClearAll_Click(object sender, EventArgs e)
        {
            // Remove all physics bodies
            foreach (var bodyList in graphBodies)
            {
                foreach (var body in bodyList)
                {
                    world.RemoveBody(body);
                }
            }

            // Clear all your data collections
            functions.Clear();
            functionExpressions.Clear();
            PointList.Clear();
            graphBodies.Clear();

            // Clear the ListView
            listViewFunctions.Items.Clear();

        }

        private void ListViewFunctions_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = listViewFunctions.SelectedIndices.Count > 0 ? listViewFunctions.SelectedIndices[0] : -1;
            if (selectedIndex >= 0 && selectedIndex < functionExpressions.Count)
            {
                textBox1.Text = functionExpressions[selectedIndex];
                textBox1.Focus();
                textBox1.SelectAll();
            }
        }

        private void buttonDeleteFunction_Click(object sender, EventArgs e)
        {
            if (listViewFunctions.SelectedIndices.Count == 0) return;

            int index = listViewFunctions.SelectedIndices[0];
            if (index < 0 || index >= functions.Count) return;

            // Șterge corpurile de fizică
            foreach (var body in graphBodies[index])
                world.RemoveBody(body);

            functions.RemoveAt(index);
            functionExpressions.RemoveAt(index);
            PointList.RemoveAt(index);
            graphBodies.RemoveAt(index);

            listViewFunctions.Items.RemoveAt(index);

            // Actualizează numele funcțiilor rămase
            for (int i = 0; i < listViewFunctions.Items.Count; i++)
                listViewFunctions.Items[i].Text = $"f{i + 1}(x,y) = {functionExpressions[i]}";

            canvas.Invalidate();
        }
        public void buttonStart_Click(object sender, EventArgs e)
        {
            foreach (var ball in balls)
            {
                world.RemoveBody(ball); // sau alt cleanup necesar
            }
            balls.Clear();
            balls.TrimExcess();
            foreach (var star in stars)
            {
                star.isActive = true;
            }
            foreach (var gen in generators)
            {
                gen.ballsCreated = 0;
                gen.start = true;
            }
        }
       
        private void ClearList(int index)
        {
            if (index >= 0 && index < PointList.Count)
            {
                PointList[index].Clear();
                foreach (var body in graphBodies[index])
                {
                    world.RemoveBody(body);
                }
                graphBodies[index].Clear();
            }
        }
        public Game(Level level) : this() // apelează constructorul implicit
        {
            LoadLevelFromData(level);
        }
        private void LoadLevelFromData(Level level)
        {
            staticPolygons.Clear();
            staticLine.Clear();
            stars.Clear();
            generators.Clear();
            foreach(var pos in level.GenPositions)
                generators.Add(new BallGenerator(world, pos, 1, gen_im, balls));
            foreach (var pos in level.StarPositions)
                stars.Add(new star(world, pos, 1, star_im, balls));

            foreach (var line in level.Lines)
                staticLine.Add(new Staticline(world, line, balls));

            foreach (var polygon in level.Polygons)
                staticPolygons.Add(new StaticPolygon(world, polygon, balls));
            free_mode = level.free_mode;
            level_id = level.level_id;
            Graph_color = level.Graph_color;
            Ball_color = level.Ball_color;
        }

        private void buttonBackFunction_Click(object sender, EventArgs e)
        {
                this.Close();
        }

        // Compilare expresie matematică în funcție

        public Func<float, float, float> CompileImplicit(
            string exprWithDomain,
            out float Xmin, out float Xmax,
            out float Ymin, out float Ymax)
        {

            // Inițializare cu valori default

            Xmin = float.NaN;
            Xmax = float.NaN;
            Ymin = float.NaN;
            Ymax = float.NaN;

            // Parsare domeniu din expresie (dacă există)

            string expr = exprWithDomain;
            int braceStart = exprWithDomain.IndexOf('{');
            if (braceStart >= 0)
            {

                // Extrage condițiile domeniului

                int braceEnd = exprWithDomain.IndexOf('}', braceStart + 1);
                if (braceEnd > braceStart)
                {
                    string domainPart = exprWithDomain
                        .Substring(braceStart + 1, braceEnd - braceStart - 1);
                    expr = exprWithDomain.Substring(0, braceStart).Trim();

                    var conds = domainPart
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim());

                    // Procesare condiții individuale

                    foreach (var c in conds)
                    {
                        string s = c.Replace(" ", "");
                        try
                        {

                            // Interpretare condiții pentru x și y

                            if (s.StartsWith("x>="))
                                Xmin = float.IsNaN(Xmin) ? float.Parse(s.Substring(3), CultureInfo.InvariantCulture) : Math.Max(Xmin, float.Parse(s.Substring(3), CultureInfo.InvariantCulture));
                            else if (s.StartsWith("x>"))
                                Xmin = float.IsNaN(Xmin) ? float.Parse(s.Substring(2), CultureInfo.InvariantCulture) : Math.Max(Xmin, float.Parse(s.Substring(2), CultureInfo.InvariantCulture));
                            else if (s.StartsWith("x<="))
                                Xmax = float.IsNaN(Xmax) ? float.Parse(s.Substring(3), CultureInfo.InvariantCulture) : Math.Min(Xmax, float.Parse(s.Substring(3), CultureInfo.InvariantCulture));
                            else if (s.StartsWith("x<"))
                                Xmax = float.IsNaN(Xmax) ? float.Parse(s.Substring(2), CultureInfo.InvariantCulture) : Math.Min(Xmax, float.Parse(s.Substring(2), CultureInfo.InvariantCulture));
                            else if (s.StartsWith("y>="))
                                Ymin = float.IsNaN(Ymin) ? float.Parse(s.Substring(3), CultureInfo.InvariantCulture) : Math.Max(Ymin, float.Parse(s.Substring(3), CultureInfo.InvariantCulture));
                            else if (s.StartsWith("y>"))
                                Ymin = float.IsNaN(Ymin) ? float.Parse(s.Substring(2), CultureInfo.InvariantCulture) : Math.Max(Ymin, float.Parse(s.Substring(2), CultureInfo.InvariantCulture));
                            else if (s.StartsWith("y<="))
                                Ymax = float.IsNaN(Ymax) ? float.Parse(s.Substring(3), CultureInfo.InvariantCulture) : Math.Min(Ymax, float.Parse(s.Substring(3), CultureInfo.InvariantCulture));
                            else if (s.StartsWith("y<"))
                                Ymax = float.IsNaN(Ymax) ? float.Parse(s.Substring(2), CultureInfo.InvariantCulture) : Math.Min(Ymax, float.Parse(s.Substring(2), CultureInfo.InvariantCulture));

                        }
                        catch
                        {
                            // Ignoră condiții scrise greșit
                        }
                    }
                }
            }
            else
            {
                expr = exprWithDomain.Trim();
            }

            // Corecție valori lipsă

            if (float.IsNaN(Xmin)) Xmin = -55f;
            if (float.IsNaN(Xmax)) Xmax = 55f;
            if (float.IsNaN(Ymin)) Ymin = -40f;
            if (float.IsNaN(Ymax)) Ymax = 40f;

            // 3) Ensure min < max
            if (Xmin >= Xmax) Xmax = Xmin + 1f;
            if (Ymin >= Ymax) Ymax = Ymin + 1f;

            try
            {

                // Configurare interpretor expresii

                var interpreter = new Interpreter()
                    .Reference(typeof(Math))
                    .SetFunction("sin", (Func<double, double>)Math.Sin)
                    .SetFunction("cos", (Func<double, double>)Math.Cos)
                    .SetFunction("tan", (Func<double, double>)Math.Tan)
                    .SetFunction("asin", (Func<double, double>)Math.Asin)
                    .SetFunction("acos", (Func<double, double>)Math.Acos)
                    .SetFunction("atan", (Func<double, double>)Math.Atan)
                    .SetFunction("sinh", (Func<double, double>)Math.Sinh)
                    .SetFunction("cosh", (Func<double, double>)Math.Cosh)
                    .SetFunction("tanh", (Func<double, double>)Math.Tanh)
                    .SetFunction("sqrt", (Func<double, double>)Math.Sqrt)
                    .SetFunction("abs", (Func<double, double>)Math.Abs)
                    .SetFunction("log", (Func<double, double>)Math.Log)
                    .SetFunction("log10", (Func<double, double>)Math.Log10)
                    .SetFunction("exp", (Func<double, double>)Math.Exp)
                    .SetFunction("pow", (Func<double, double, double>)Math.Pow)
                    .SetFunction("atan2", (Func<double, double, double>)Math.Atan2)
                    .SetFunction("min", (Func<double, double, double>)Math.Min)
                    .SetFunction("max", (Func<double, double, double>)Math.Max)
                    .SetFunction("floor", (Func<double, double>)Math.Floor)
                    .SetFunction("ceil", (Func<double, double>)Math.Ceiling)
                    .SetFunction("round", (Func<double, double>)Math.Round)
                    .SetVariable("pi", Math.PI)
                    .SetVariable("e", Math.E)
                    .SetFunction("sind", (Func<double, double>)(x => Math.Sin(x * Math.PI / 180.0)))
                    .SetFunction("cosd", (Func<double, double>)(x => Math.Cos(x * Math.PI / 180.0)))
                    .SetFunction("tand", (Func<double, double>)(x => Math.Tan(x * Math.PI / 180.0)))
                    .SetFunction("mod", (Func<double, double, double>)((a, b) => a % b))
                    .SetFunction("if", (Func<bool, double, double, double>)((cond, t, f) => cond ? t : f));

                var del = interpreter.ParseAsDelegate<Func<double, double, double>>(expr, "x", "y");

                return (x, y) =>
                {
                    

                    try
                    {
                        double result = del(x, y);
                        if (double.IsNaN(result) || double.IsInfinity(result))
                            return float.NaN;
                        return (float)result;
                    }
                    catch
                    {
                        return float.NaN;
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error compiling '{expr}': {ex.Message}");
                return (x, y) => float.NaN;
            }
        }

        // Verificare dacă expresia conține 'y'
        public bool ContainsY(string expression)
        {
            if (string.IsNullOrEmpty(expression))
                return false;


            return Regex.IsMatch(expression, @"\by\b", RegexOptions.IgnoreCase);
        }

        public static (string Expression, string Conditions) SplitExpressionWithBrackets(string input)
        {
            input = input.Trim();
            string expression = input;
            string conditions = string.Empty;

            int braceStart = input.IndexOf('{');
            if (braceStart >= 0)
            {
                int braceEnd = input.IndexOf('}', braceStart);
                if (braceEnd > braceStart)
                {
                    // Extrage expresia (partea dinainte de '{')
                    expression = input.Substring(0, braceStart).Trim();

                    // Extrage condițiile CU acoladele incluse
                    conditions = input.Substring(braceStart, braceEnd - braceStart + 1).Trim();
                }
            }

            return (expression, conditions);
        }

        private void ShowWinScreen()
        {
            var winLabel = new Label
            {
                Text = "LEVEL COMPLETE",
                ForeColor = Color.Gold,
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                AutoSize = false,
                Size = new Size(400, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.CadetBlue
            };

            // Position the label in the center
            winLabel.Location = new Point((this.ClientSize.Width - winLabel.Width) / 2, 10);

            Controls.Add(winLabel);

            // Optional: Bring to front in case of overlapping controls
            winLabel.BringToFront();
        }
    }
}
