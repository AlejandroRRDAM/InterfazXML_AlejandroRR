using System;
using System.Drawing; // Para Point y Size
using System.IO; // Para Path y File
using System.Windows.Forms; // Para Form, Control, Button, Label, etc.
using System.Xml.Linq; // ¡Importante! Para XDocument, XElement
using System.Linq; // ¡Importante! Para .Select() y .ToArray()

namespace InterfazXML_AlejandroRR // Tu namespace
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GenerarControlesDesdeXML();
        }

        private void GenerarControlesDesdeXML()
        {
            try
            {
                string xmlPath = Path.Combine(Application.StartupPath, "Interfaz.xml");

                if (!File.Exists(xmlPath))
                {
                    MessageBox.Show("Error: No se encontró el archivo Interfaz.xml", "Error de Configuración", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                XDocument doc = XDocument.Load(xmlPath);

                foreach (XElement controlElement in doc.Root.Elements("control"))
                {
                    Control nuevoControl = CrearControl(controlElement);
                    if (nuevoControl != null)
                    {
                        this.Controls.Add(nuevoControl);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al leer o parsear el XML: {ex.Message}", "Error Fatal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// "Fábrica" que crea un control específico basado en un nodo XElement del XML.
        /// </summary>
        private Control CrearControl(XElement controlElement)
        {
            // Lectura de propiedades comunes
            string tipo = controlElement.Element("tipo")?.Value.ToLower();
            string nombre = controlElement.Element("nombre")?.Value;
            string texto = controlElement.Element("texto")?.Value;
            string[] pos = controlElement.Element("posicion")?.Value.Split(',');
            Point location = new Point(int.Parse(pos[0]), int.Parse(pos[1]));
            string[] tam = controlElement.Element("tamaño")?.Value.Split(',');
            Size size = new Size(int.Parse(tam[0]), int.Parse(tam[1]));

            // --- NUEVO: Leer el color ---
            string colorFondo = controlElement.Element("colorFondo")?.Value;

            Control controlGenerado = null;

            switch (tipo)
            {
                case "label":
                    controlGenerado = new Label();
                    break;

                case "button":
                    controlGenerado = new Button();
                    controlGenerado.Click += new EventHandler(GenericButton_Click);
                    break;

                case "panel": // Lo mantenemos por si acaso
                    controlGenerado = new Panel
                    {
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    break;

                // --- NUEVO: Soporte para DataGridView ---
                case "datagridview":
                    DataGridView dgv = new DataGridView();

                    // Parsear Columnas
                    var columnas = controlElement.Element("columnas")?.Elements("columna");
                    if (columnas != null)
                    {
                        foreach (var col in columnas)
                        {
                            dgv.Columns.Add(col.Value, col.Value); // (nombre, textoDeCabecera)
                        }
                    }

                    // Parsear Filas
                    var filas = controlElement.Element("filas")?.Elements("fila");
                    if (filas != null)
                    {
                        foreach (var fila in filas)
                        {
                            // Usamos LINQ para coger el valor de cada <celda> y ponerlo en un array
                            var celdas = fila.Elements("celda").Select(c => c.Value).ToArray();
                            dgv.Rows.Add(celdas);
                        }
                    }

                    // Propiedades extra para que se vea bien
                    dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Rellena el espacio
                    dgv.AllowUserToAddRows = false; // Oculta la fila vacía del final
                    dgv.ReadOnly = true; // El usuario no puede editar

                    controlGenerado = dgv;
                    break;
            }

            // Asigna las propiedades comunes al control recién creado
            if (controlGenerado != null)
            {
                controlGenerado.Name = nombre;
                controlGenerado.Text = texto;
                controlGenerado.Location = location;
                controlGenerado.Size = size;

                // --- NUEVO: Aplicar el color ---
                if (!string.IsNullOrEmpty(colorFondo))
                {
                    try
                    {
                        // ColorTranslator nos deja usar "Red", "LightBlue", "#FF0000", etc.
                        controlGenerado.BackColor = ColorTranslator.FromHtml(colorFondo);
                    }
                    catch (Exception)
                    {
                        // Ignora el color si está mal escrito
                    }
                }
            }

            return controlGenerado;
        }

        private void GenericButton_Click(object sender, EventArgs e)
        {
            Button botonPresionado = sender as Button;
            if (botonPresionado != null)
            {
                string mensaje = $"Has hecho clic en el botón: '{botonPresionado.Text}'";
                MessageBox.Show(mensaje, "Evento Dinámico Detectado");
            }
        }
    }
}
