using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPRN3_CASG
{
    class DataBase
    {
        private string conn = ConfigurationManager.ConnectionStrings["connectionstringDPRN3"].ToString();

        public async Task<string> Get_Lista()
        {
            string confirmacion = "";
            try
            {
                using (SqlConnection cn = new SqlConnection(conn))
                {
                    cn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.spObtenerLista",cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                confirmacion = reader.GetString(0);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            return confirmacion;
        }

    }
}
