using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DPRN3_CASG
{
    public partial class Principal : Form
    {
        DataBase db = new DataBase();
        public Principal()
        {
            InitializeComponent();
            this.comboBox1.DataSource = db.Get_CatalogoUnidades();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView2.Rows.Add(
                textBox1.Text, 
                textBox2.Text,
                comboBox1.SelectedItem.GetType().GetProperty("Unidad").GetValue(comboBox1.SelectedItem, null), 
                checkBox2.Checked, 
                checkBox1.Checked, 
                textBox3.Text,
                comboBox1.SelectedItem.GetType().GetProperty("Id").GetValue(comboBox1.SelectedItem, null)
                );

        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            int listaId = db.Post_Lista();
            _ProductoLista producto = new _ProductoLista();
            for (int i = 0; i < dataGridView2.Rows.Count - 1; i++)
            {
                producto.NombreProducto     = dataGridView2.Rows[i].Cells[0].Value.ToString();
                producto.ListaId            = listaId;
                producto.IdUnidad           = Int32.Parse(dataGridView2.Rows[i].Cells[6].Value.ToString());
                producto.Cantidad           = Double.Parse(dataGridView2.Rows[i].Cells[1].Value.ToString());
                producto.Notas              = dataGridView2.Rows[i].Cells[5].Value.ToString();
                producto.EsUrgente          = Boolean.Parse(dataGridView2.Rows[i].Cells[3].Value.ToString());
                producto.AceptaSustitutos   = Boolean.Parse(dataGridView2.Rows[i].Cells[4].Value.ToString());

                var ok = db.Post_ProductoLista(producto);
            }
        }

        private void tabControl1_DoubleClick(object sender, EventArgs e)
        {

        }

        private void tabControl1_Click(object sender, EventArgs e)
        {
            var historialLista = db.Get_HistorialLista();

            for (int i = 0; i < historialLista.Count; i++)
            {
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[0].Value = historialLista[i].ListaId;
                dataGridView1.Rows[i].Cells[1].Value = historialLista[i].Fecha;
                dataGridView1.Rows[i].Cells[2].Value = historialLista[i].Activo;
            }        
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView3.Rows.Clear();
            dataGridView3.Refresh();
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = this.dataGridView1.Rows[e.RowIndex];

                int listaId = Int32.Parse(row.Cells["Lista"].Value.ToString());

                List<_ProductoLista> detalle = db.Get_DetalleLista(listaId);

                if(detalle.Count > 0)
                {
                    for (int i = 0; i < detalle.Count; i++)
                    {
                        dataGridView3.Rows.Add();
                        dataGridView3.Rows[i].Cells[0].Value = detalle[i].NombreProducto;
                        dataGridView3.Rows[i].Cells[1].Value = detalle[i].Cantidad;
                        dataGridView3.Rows[i].Cells[2].Value = detalle[i].Unidad;
                        dataGridView3.Rows[i].Cells[3].Value = detalle[i].EsUrgente;
                        dataGridView3.Rows[i].Cells[4].Value = detalle[i].AceptaSustitutos;
                        dataGridView3.Rows[i].Cells[5].Value = detalle[i].Notas;
                    }
                } else
                {
                    dataGridView3.Rows.Clear();
                    dataGridView3.Refresh();
                    MessageBox.Show("La lista seleccionada no cuenta con productos.");
                }
            }          
        }
    }
}
