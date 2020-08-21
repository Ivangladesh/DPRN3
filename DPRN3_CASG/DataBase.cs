using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPRN3_CASG
{
    class DataBase
    {
        private string conn = ConfigurationManager.ConnectionStrings["connectionstringDPRN3"].ToString();

        public List<_ProductoLista> Get_HistorialLista()
        {
            List<_ProductoLista> lista = new List<_ProductoLista>();
            try
            {
                using (SqlConnection cn = new SqlConnection(conn))
                {
                    cn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.spObtenerLista",cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new _ProductoLista
                                {
                                    ListaId = reader.GetInt32(0),
                                    Fecha = reader.GetDateTime(1),
                                    Activo = reader.GetBoolean(2)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lista;
        }

        public List<_CatalogoUnidades> Get_CatalogoUnidades()
        {
            List<_CatalogoUnidades> unidades = new List<_CatalogoUnidades>();
            try
            {
                using (SqlConnection cn = new SqlConnection(conn))
                {
                    cn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.spObtenerCatalogoUnidadesMedida", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                unidades.Add( new _CatalogoUnidades
                                {
                                    Id = reader.GetInt32(0),
                                    Unidad = reader.GetString(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return unidades;
        }

        public int Post_Lista()
        {
            int listaId = 0;
            try
            {
                using (SqlConnection cn = new SqlConnection(conn))
                {
                    cn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.spInsertarLista", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        var paramReturn = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
                        paramReturn.Direction = ParameterDirection.ReturnValue;
                        cmd.ExecuteNonQuery();
                        listaId = Convert.ToInt32(paramReturn.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return listaId;
        }

        public async Task<bool> Post_ProductoLista(_ProductoLista oProducto)
        {
            bool ok = false;
            try
            {
                using (SqlConnection cn = new SqlConnection(conn))
                {
                    cn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.spInsertarProductoLista", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@ListaId", SqlDbType.Int)).Value               = oProducto.ListaId;
                        cmd.Parameters.Add(new SqlParameter("@NombreProducto", SqlDbType.NVarChar)).Value   = oProducto.NombreProducto;
                        cmd.Parameters.Add(new SqlParameter("@Cantidad", SqlDbType.Float)).Value            = oProducto.Cantidad;
                        cmd.Parameters.Add(new SqlParameter("@IdUnidad", SqlDbType.Int)).Value              = oProducto.IdUnidad;
                        cmd.Parameters.Add(new SqlParameter("@Notas", SqlDbType.NVarChar)).Value            = oProducto.Notas;
                        cmd.Parameters.Add(new SqlParameter("@EsUrgente", SqlDbType.Bit)).Value             = oProducto.EsUrgente;
                        cmd.Parameters.Add(new SqlParameter("@AceptaSustitutos", SqlDbType.Bit)).Value      = oProducto.AceptaSustitutos;
                        var paramReturn = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
                        paramReturn.Direction = ParameterDirection.ReturnValue;
                        await cmd.ExecuteNonQueryAsync();
                        ok = Convert.ToBoolean(paramReturn.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ok;
        }

        public List<_ProductoLista> Get_DetalleLista(int listaId)
        {
            List<_ProductoLista> lista = new List<_ProductoLista>();
            try
            {
                using (SqlConnection cn = new SqlConnection(conn))
                {
                    cn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.spObtenerDetalleLista", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@ListaId", SqlDbType.Int)).Value = listaId;
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new _ProductoLista
                                {
                                    NombreProducto      = reader.GetString(0),
                                    Cantidad            = reader.GetDouble(1),
                                    Unidad              = reader.GetString(2),
                                    EsUrgente           = reader.GetBoolean(3),
                                    AceptaSustitutos    = reader.GetBoolean(4),
                                    Notas               = reader.GetString(5),
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lista;
        }
    }
}
