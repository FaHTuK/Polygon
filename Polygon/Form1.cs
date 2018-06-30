using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Polygon
{
    public partial class Form1 : Form
    {
        List<Point> pointsList = new List<Point>();

        private Point[] m_aPolygon;
        private ArrayList m_alSelectedPts = new ArrayList();
        private Pen m_PolygonPen = new Pen(Color.Blue, 2);

        public Form1()
        {
            InitializeComponent();
        }

        //Рисуем крестики (вершины) по клику на экране
        private void DrawCross(int x, int y)
        {
            Graphics graphics = panelDraw.CreateGraphics();
            Point startPoint = new Point(x - 2, y);
            Point endPoint = new Point(x + 2, y);
            graphics.DrawLine(Pens.Red, startPoint, endPoint);

            startPoint = new Point(x, y - 2);
            endPoint = new Point(x, y + 2);
            graphics.DrawLine(Pens.Red, startPoint, endPoint);
        }

        //Соединяем вершины (в порядке их добавления) и рисуем полигон
        private void DrawPolygon()
        {
            Read(); 

            if (m_aPolygon != null)
            {
                if (m_aPolygon.Length > 0)
                {
                    Graphics gfx = this.panelDraw.CreateGraphics();
                    gfx.DrawPolygon(m_PolygonPen, m_aPolygon);
                }
            }
        }

        //Считываем все вершины на панели
        private void Read()
        {
            //Проверяем, чтобы крестиков было не менее 3-х штук
            if (m_alSelectedPts.Count < 3)
                MessageBox.Show("Please using mouse to pick at least 3 points!",
                    "How to initialize a Polygon",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                //Получаем координаты всех вершин
                m_aPolygon = new Point[m_alSelectedPts.Count];
                for (int i = 0; i < m_alSelectedPts.Count; i++)
                {
                    m_aPolygon[i] = (Point)m_alSelectedPts[i];

                    //Записываем координаты вершин в List
                    pointsList.AddRange(new Point[] {
                        new Point(m_aPolygon[i].X, m_aPolygon[i].Y)
                    });
                }
            }
        }

        //Метод для проверки, находится ли точка внутри полигона или нет
        public bool IsPointInPolygon(Point point, Point[] polygonPoints)
        {
            double x_min, x_max, y_min, y_max;

            x_min = x_max = polygonPoints[0].X;
            y_min = y_max = polygonPoints[0].Y;
            for (int i = 1; i < polygonPoints.Length; i++)
            {
                Point p = polygonPoints[i];
                x_min = Math.Min(p.X, x_min);
                x_max = Math.Max(p.X, x_max);
                y_min = Math.Min(p.Y, y_min);
                y_max = Math.Max(p.Y, y_max);
            }

            if (point.X < x_min || point.X > x_max || point.Y < y_min || point.Y > y_max)
            {
                return false;
            }

            bool ins = false;
            for (int i = 0, j = polygonPoints.Length - 1; i < polygonPoints.Length; j = i++)
            {
                if ((polygonPoints[i].Y > point.Y) != (polygonPoints[j].Y > point.Y) &&
                     point.X < (polygonPoints[j].X - polygonPoints[i].X) * (point.Y - polygonPoints[i].Y) / (polygonPoints[j].Y - polygonPoints[i].Y) + polygonPoints[i].X)
                {
                    ins = !ins;
                }
            }

            return ins;
        }
        
        //Очищаем панель
        private void Clear()
        {
            m_aPolygon = new Point[0];
            m_alSelectedPts.Clear();
            panelDraw.CreateGraphics().Clear(panelDraw.BackColor);
            Invalidate();
        }

        //Рисуем вершины по клику
        private void panelDraw_MouseUp(object sender, MouseEventArgs e)
        {
            Point clickedPt = new Point(e.X, e.Y);
            m_alSelectedPts.Add(clickedPt);
            DrawCross(e.X, e.Y);

            if (btnDraw.Enabled == false)
            {
                //Выводим, находится ли точка внутри полигона
                statusLabel.Text = "Is Point In Polygon: " + IsPointInPolygon(clickedPt, m_aPolygon).ToString();
            }
        }

        //Соединяем вершины
        private void btnDraw_Click(object sender, EventArgs e)
        {
            DrawPolygon();
            btnDraw.Enabled = false;
            btnSave.Enabled = true;
        }

        //Очищаем панель
        private void btnClear_Click(object sender, EventArgs e)
        {
            Clear();
            btnDraw.Enabled = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            statusLabel.Text = "";
            btnSave.Enabled = false;
        }

        //Сохраняем полигон в XML-файл
        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Xml Files (*.xml)|*.xml";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Point>));
                    xmlSerializer.Serialize(writer, pointsList);
                }
            }
        }

        //Загружаем полигон из XML-файла
        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Xml Files (*.xml)|*.xml";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Clear();
                using (StreamReader reader = new StreamReader(openFileDialog.FileName))
                {
                    XmlSerializer xml = new XmlSerializer(typeof(List<Point>));
                    List<Point> point = (List<Point>)xml.Deserialize(reader);
                    foreach (var a in point)
                    {
                        int x = a.X;
                        int y = a.Y;
                        Point loadPt = new Point(x, y);
                        m_alSelectedPts.Add(loadPt);
                        DrawCross(x, y);
                    }
                    DrawPolygon();
                    btnDraw.Enabled = false;
                }
            }
        }
    }
}
