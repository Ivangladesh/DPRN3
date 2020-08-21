using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;

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
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private async void button5_Click(object sender, EventArgs e)
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

                var ok = await db.Post_ProductoLista(producto);

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
                    MessageBox.Show("La lista seleccionada no cuenta con productos.", ":(", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }          
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var fecha = dataGridView1.CurrentRow.Cells[1].Value.ToString();
            DateTime oDate = Convert.ToDateTime(fecha);
            if (dataGridView3.Rows.Count > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "PDF (*.pdf)|*.pdf";
                sfd.FileName = $"lista_{oDate.Day}_{oDate.Month}_{oDate.Year}.pdf";
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
        private void button7_Click(object sender, EventArgs e)
        {
            bm = new Bitmap(this.dataGridView3.Width, this.dataGridView3.Height);
            dataGridView3.DrawToBitmap(bm, new System.Drawing.Rectangle(0, 0, this.dataGridView3.Width, this.dataGridView3.Height));
            printPreviewDialog1.Document = printDocument1;
            printPreviewDialog1.PrintPreviewControl.Zoom = 1;
            printPreviewDialog1.ShowDialog();
            
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(bm, 0, 0);
        }
    }
}
