﻿using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DPRN3_CASG
{
    public partial class Principal : Form
    {
        readonly DataBase db = new DataBase();
        public Principal()
        {
            InitializeComponent();
            ObtenerUnidades();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "" || textBox2.Text == "" || ((KeyValuePair<int, string>)comboBox1.SelectedItem).Key == 0)
            {
                MessageBox.Show("Agregue los campos necesarios", ":(", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                dataGridView2.Rows.Add(
                    textBox1.Text,
                    textBox2.Text,
                    ((KeyValuePair<int, string>)comboBox1.SelectedItem).Value,
                    checkBox2.Checked,
                    checkBox1.Checked,
                    textBox3.Text,
                    ((KeyValuePair<int, string>)comboBox1.SelectedItem).Key
                    );

                ReiniciarControles(groupBox1);
            }


        }

        private void TextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private async void Button5_Click(object sender, EventArgs e)
        {
            int listaId = db.Post_Lista();
            int cantidadRegistros = dataGridView2.Rows.Count;
            _ProductoLista producto = new _ProductoLista();
            for (int i = 0; i < dataGridView2.Rows.Count; i++)
            {
                producto.NombreProducto     = dataGridView2.Rows[i].Cells[0].Value.ToString();
                producto.ListaId            = listaId;
                producto.IdUnidad           = Int32.Parse(dataGridView2.Rows[i].Cells[6].Value.ToString());
                producto.Cantidad           = Double.Parse(dataGridView2.Rows[i].Cells[1].Value.ToString());
                producto.Notas              = dataGridView2.Rows[i].Cells[5].Value.ToString();
                producto.EsUrgente          = Boolean.Parse(dataGridView2.Rows[i].Cells[3].Value.ToString());
                producto.AceptaSustitutos   = Boolean.Parse(dataGridView2.Rows[i].Cells[4].Value.ToString());

                bool ok = await db.Post_ProductoLista(producto);

                if (ok)
                {
                    cantidadRegistros--;
                }
            }
            if (cantidadRegistros == 0)
            {
                MessageBox.Show("Operación exitosa!");
                dataGridView2.Rows.Clear();
                dataGridView2.Refresh();
                ReiniciarControles(groupBox1);
            }
            else
            {
                MessageBox.Show("Ha ocurrido un error consulte a su administrador.", ":(", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void TabControl1_Click(object sender, EventArgs e)
        {
            ObtenerHIstorialLista();
        }

        private async void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView3.Rows.Clear();
            dataGridView3.Refresh();
            if(e.RowIndex >= 0)
            {
                if (e.ColumnIndex == 0 || e.ColumnIndex == 1)
                {
                    DataGridViewRow row = this.dataGridView1.Rows[e.RowIndex];

                    int listaId = Int32.Parse(row.Cells["Lista"].Value.ToString());

                    List<_ProductoLista> detalle = await db.Get_DetalleLista(listaId);

                    if (detalle.Count > 0)
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
                    }
                    else
                    {
                        dataGridView3.Rows.Clear();
                        dataGridView3.Refresh();
                        MessageBox.Show("La lista seleccionada no cuenta con productos.", ":(", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
           
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            var fecha = dataGridView1.CurrentRow.Cells[1].Value.ToString();
            DateTime oDate = Convert.ToDateTime(fecha);
            if (dataGridView3.Rows.Count > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "PDF (*.pdf)|*.pdf",
                    FileName = $"lista_{oDate.Day}_{oDate.Month}_{oDate.Year}.pdf"
                };
                bool fileError = false;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists(sfd.FileName))
                    {
                        try
                        {
                            File.Delete(sfd.FileName);
                        }
                        catch (IOException ex)
                        {
                            fileError = true;
                            MessageBox.Show("Ha ocurrido un error al intentar guardar." + ex.Message);
                        }
                    }
                    if (!fileError)
                    {
                        try
                        {
                            PdfPTable pdfTable = new PdfPTable(dataGridView3.Columns.Count);
                            pdfTable.DefaultCell.Padding = 3;
                            pdfTable.WidthPercentage = 100;
                            pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;

                            foreach (DataGridViewColumn column in dataGridView3.Columns)
                            {
                                PdfPCell cell = new PdfPCell(new Phrase(column.HeaderText));
                                pdfTable.AddCell(cell);
                            }

                            foreach (DataGridViewRow row in dataGridView3.Rows)
                            {
                                foreach (DataGridViewCell cell in row.Cells)
                                {
                                    pdfTable.AddCell(cell.Value.ToString());
                                }
                            }

                            using (FileStream stream = new FileStream(sfd.FileName, FileMode.Create))
                            {
                                Document pdfDoc = new Document(PageSize.A4, 10f, 20f, 20f, 10f);
                                PdfWriter.GetInstance(pdfDoc, stream);
                                pdfDoc.Open();
                                pdfDoc.Add(pdfTable);
                                pdfDoc.Close();
                                stream.Close();
                            }

                            MessageBox.Show("Archivo generado correctamente!", "Info");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error :" + ex.Message);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("No exite información para imprimir", "Info");
            }
        }
        Bitmap bm;
        private void Button7_Click(object sender, EventArgs e)
        {
            bm = new Bitmap(this.dataGridView3.Width, this.dataGridView3.Height);
            dataGridView3.DrawToBitmap(bm, new System.Drawing.Rectangle(0, 0, this.dataGridView3.Width, this.dataGridView3.Height));
            printPreviewDialog1.Document = printDocument1;
            printPreviewDialog1.PrintPreviewControl.Zoom = 1;
            printPreviewDialog1.ShowDialog();
            
        }

        private void PrintDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(bm, 0, 0);
        }

        private async void Button4_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = this.dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex];

            int listaId = Int32.Parse(row.Cells["Lista"].Value.ToString());

            var ok = await db.Delete_Lista(listaId);

            if (ok)
            {
                ObtenerHIstorialLista();
                MessageBox.Show("Operación exitosa!");
            }
            else
            {
                MessageBox.Show("Ha ocurrido un error consulte a su administrador.", ":(", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void ObtenerHIstorialLista()
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();

            var historialLista = db.Get_HistorialLista();

            for (int i = 0; i < historialLista.Count; i++)
            {
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[0].Value = historialLista[i].ListaId;
                dataGridView1.Rows[i].Cells[1].Value = historialLista[i].Fecha;
                dataGridView1.Rows[i].Cells[2].Value = historialLista[i].Activo;
            }
        }

        public static void ReiniciarControles(Control form)
        {
            foreach (Control control in form.Controls)
            {
                if (control is TextBox textBox)
                {
                    textBox.Text = null;
                }
                if (control is ComboBox comboBox)
                {
                    if (comboBox.Items.Count > 0)
                        comboBox.SelectedIndex = 0;
                }
                if (control is CheckBox checkBox)
                {
                    checkBox.Checked = false;
                }
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            ReiniciarControles(groupBox1);
        }

        private void ObtenerUnidades()
        {
            Dictionary<int, string> lista = db.Get_CatalogoUnidades();
            lista = (new Dictionary<int, string> { { 0, "Seleccione" } }).Concat(lista).ToDictionary(k => k.Key, v => v.Value);
            comboBox1.Items.Insert(0, "Seleccione");
            comboBox1.SelectedIndex = 0;

            comboBox1.DataSource = new BindingSource(lista, null);
            comboBox1.DisplayMember = "Value";
            comboBox1.ValueMember = "Key";
        }

        private async void DataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2)
            {
                DataGridViewRow row = this.dataGridView1.Rows[e.RowIndex];
                int listaId = Int32.Parse(row.Cells["Lista"].Value.ToString());
                bool estado = Boolean.Parse(row.Cells["Estado"].Value.ToString());
                bool ok = await db.Update_EstadoLista(listaId, estado);
                if (ok)
                {
                    MessageBox.Show("Operación exitosa!");
                }
                else
                {
                    MessageBox.Show("Ha ocurrido un error consulte a su administrador.", ":(", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }
    }
}
