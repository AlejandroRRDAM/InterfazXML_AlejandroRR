using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using Microsoft.VisualBasic;

namespace InterfazXML_AlejandroRR
{
    public partial class Form1 : Form
    {
        private string datosXmlPath;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            datosXmlPath = Path.Combine(Application.StartupPath, "Datos.xml");
            GenerarControlesDesdeXML();
            CargarDatosSiExisten();
        }

        // --- (GenerarControles, CrearControl, CargarDatosSiExisten... son IDÉNTICOS) ---

        private void GenerarControlesDesdeXML()
        {
            try
            {
                string xmlPath = Path.Combine(Application.StartupPath, "Interfaz.xml");
                if (!File.Exists(xmlPath)) { /* ... manejo de error ... */ return; }

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
                MessageBox.Show($"Error al leer Interfaz.xml: {ex.Message}", "Error Fatal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Control CrearControl(XElement controlElement)
        {
            string tipo = controlElement.Element("tipo")?.Value.ToLower();
            string nombre = controlElement.Element("nombre")?.Value;
            string texto = controlElement.Element("texto")?.Value;
            string[] pos = controlElement.Element("posicion")?.Value.Split(',');
            Point location = new Point(int.Parse(pos[0]), int.Parse(pos[1]));
            string[] tam = controlElement.Element("tamaño")?.Value.Split(',');
            Size size = new Size(int.Parse(tam[0]), int.Parse(tam[1]));

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
                case "panel":
                    controlGenerado = new Panel { BorderStyle = BorderStyle.FixedSingle };
                    break;
                case "datagridview":
                    DataGridView dgv = new DataGridView();
                    var columnas = controlElement.Element("columnas")?.Elements("columna");
                    if (columnas != null)
                    {
                        foreach (var col in columnas)
                        {
                            dgv.Columns.Add(col.Value, col.Value);
                        }
                    }
                    dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dgv.AllowUserToAddRows = false;
                    dgv.ReadOnly = true;
                    dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dgv.MultiSelect = false;

                    controlGenerado = dgv;
                    break;
            }

            if (controlGenerado != null)
            {
                controlGenerado.Name = nombre;
                controlGenerado.Text = texto;
                controlGenerado.Location = location;
                controlGenerado.Size = size;
                AplicarEstilosGenericos(controlGenerado, controlElement);
            }

            return controlGenerado;
        }

        private void CargarDatosSiExisten()
        {
            try
            {
                if (!File.Exists(datosXmlPath))
                {
                    return;
                }

                XDocument doc = XDocument.Load(datosXmlPath);
                DataGridView dgv = this.Controls.Find("dgvPokemon", true).FirstOrDefault() as DataGridView;
                if (dgv == null) return;
                dgv.Rows.Clear();

                foreach (XElement pkm in doc.Root.Elements("Pokemon"))
                {
                    string nombre = pkm.Element("Nombre")?.Value;
                    string tipo = pkm.Element("Tipo")?.Value;
                    string color = pkm.Element("Color")?.Value;

                    dgv.Rows.Add(nombre, tipo, color);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar Datos.xml: {ex.Message}");
            }
        }

        // --- (GuardarDatosEnGrid es IDÉNTICO) ---
        private void GuardarDatosEnGrid()
        {
            try
            {
                DataGridView dgv = this.Controls.Find("dgvPokemon", true).FirstOrDefault() as DataGridView;
                if (dgv == null) return;

                XDocument doc = new XDocument();
                XElement root = new XElement("PokemonData");
                doc.Add(root);

                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.IsNewRow) continue;
                    XElement pkm = new XElement("Pokemon",
                        new XElement("Nombre", row.Cells[0].Value?.ToString()),
                        new XElement("Tipo", row.Cells[1].Value?.ToString()),
                        new XElement("Color", row.Cells[2].Value?.ToString())
                    );
                    root.Add(pkm);
                }

                doc.Save(datosXmlPath);

                // ¡Añadimos feedback para el usuario!
                MessageBox.Show("¡Cambios guardados con éxito!", "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar Datos.xml: {ex.Message}");
            }
        }


        // --- MANEJADORES DE BOTONES (ACTUALIZADOS) ---

        private void GenericButton_Click(object sender, EventArgs e)
        {
            Button botonPresionado = sender as Button;
            if (botonPresionado == null) return;

            switch (botonPresionado.Name)
            {
                case "btnAnadir":
                    AnadirPokemon();
                    break;

                case "btnEditar":
                    EditarPokemon();
                    break;

                case "btnEliminar":
                    EliminarPokemon();
                    break;

                // ¡NUEVO CASE!
                case "btnGuardar":
                    GuardarDatosEnGrid();
                    break;
            }
        }

        private void AnadirPokemon()
        {
            string nombre = Interaction.InputBox("Introduce el nombre del Pokémon:", "Añadir Pokémon", "");
            if (string.IsNullOrWhiteSpace(nombre)) return;
            string tipo = Interaction.InputBox("Introduce el tipo:", "Añadir Pokémon", "");
            string color = Interaction.InputBox("Introduce el color:", "Añadir Pokémon", "");

            DataGridView dgv = this.Controls.Find("dgvPokemon", true).FirstOrDefault() as DataGridView;
            if (dgv == null) return;

            dgv.Rows.Add(nombre, tipo, color);

            // ¡YA NO SE GUARDA AQUÍ!
            // GuardarDatosEnGrid(); 
        }

        private void EditarPokemon()
        {
            DataGridView dgv = this.Controls.Find("dgvPokemon", true).FirstOrDefault() as DataGridView;
            if (dgv == null) return;

            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Por favor, selecciona una fila para editar.");
                return;
            }

            DataGridViewRow fila = dgv.SelectedRows[0];
            string nombreActual = fila.Cells[0].Value?.ToString();
            string tipoActual = fila.Cells[1].Value?.ToString();
            string colorActual = fila.Cells[2].Value?.ToString();
            string nombreNuevo = Interaction.InputBox("Nombre:", "Editar Pokémon", nombreActual);
            if (string.IsNullOrWhiteSpace(nombreNuevo)) return;
            string tipoNuevo = Interaction.InputBox("Tipo:", "Editar Pokémon", tipoActual);
            string colorNuevo = Interaction.InputBox("Color:", "Editar Pokémon", colorActual);

            fila.Cells[0].Value = nombreNuevo;
            fila.Cells[1].Value = tipoNuevo;
            fila.Cells[2].Value = colorNuevo;

            // ¡YA NO SE GUARDA AQUÍ!
            // GuardarDatosEnGrid();
        }

        private void EliminarPokemon()
        {
            DataGridView dgv = this.Controls.Find("dgvPokemon", true).FirstOrDefault() as DataGridView;
            if (dgv == null) return;

            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Por favor, selecciona una fila para eliminar.");
                return;
            }

            DataGridViewRow fila = dgv.SelectedRows[0];
            string nombre = fila.Cells[0].Value?.ToString();

            DialogResult resultado = MessageBox.Show($"¿Estás seguro de que quieres eliminar a {nombre}?", "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (resultado == DialogResult.Yes)
            {
                dgv.Rows.Remove(fila);

                // ¡YA NO SE GUARDA AQUÍ!
                // GuardarDatosEnGrid();
            }
        }

        // --- (AplicarEstilosGenericos es IDÉNTICO) ---
        private void AplicarEstilosGenericos(Control control, XElement controlElement)
        {
            var estilosNode = controlElement.Element("estilos");
            if (estilosNode == null) return;
            foreach (var propElement in estilosNode.Elements("propiedad"))
            {
                string nombre = null;
                string valor = null;
                try
                {
                    nombre = propElement.Attribute("nombre")?.Value;
                    valor = propElement.Attribute("valor")?.Value;
                    if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(valor)) continue;
                    PropertyInfo propInfo = control.GetType().GetProperty(nombre);
                    if (propInfo != null && propInfo.CanWrite)
                    {
                        var converter = TypeDescriptor.GetConverter(propInfo.PropertyType);
                        object convertedValue = converter.ConvertFromInvariantString(valor);
                        propInfo.SetValue(control, convertedValue);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error aplicando estilo: {nombre} - {ex.Message}");
                }
            }
        }
    }
}