using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPRN3_CASG
{
    class _ProductoLista
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; }
        public int ListaId { get; set; }
        public double Cantidad { get; set; }
        public int IdUnidad { get; set; }
        public string Unidad { get; set; }
        public string Notas { get; set; }
        public bool EsUrgente { get; set; }
        public bool AceptaSustitutos { get; set; }
        public DateTime Fecha { get; set; }
        public bool Activo { get; set; }
    }
}
