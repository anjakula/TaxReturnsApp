using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Data.OleDb;
using System.Globalization;
using System.Collections;

namespace TaxReturns.Helpers
{
    public static class Utility
    {
        public static DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(strFilePath))
            {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }

                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(',');
                    if (rows.Length > 1)
                    {
                        DataRow dr = dt.NewRow();
                        for (int i = 0; i < headers.Length; i++)
                        {
                            dr[i] = rows[i].Trim();
                        }
                        dt.Rows.Add(dr);
                    }
                }

            }


            return dt;
        }

        public static DataTable ConvertXSLXtoDataTable(string strFilePath, string connString)
        {
            OleDbConnection oledbConn = new OleDbConnection(connString);
            DataTable dt = new DataTable();
            try
            {

                oledbConn.Open();
                OleDbCommand cmd = new OleDbCommand("SELECT * FROM [Sheet1$]", oledbConn);
                OleDbDataAdapter oleda = new OleDbDataAdapter();
                oleda.SelectCommand = cmd;
                DataSet ds = new DataSet();
                oleda.Fill(ds);

                dt = ds.Tables[0];

            }
            catch
            {
            }
            finally
            {

                oledbConn.Close();
            }

            return dt;

        }

        public static class CurrencyCodeMapper
        {
            private static readonly Dictionary<string, string> SymbolsByCode;
            
            public static string GetSymbol(string code) { return SymbolsByCode[code]; }

            static CurrencyCodeMapper()
            {
                SymbolsByCode = new Dictionary<string, string>();

                var regions = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                              .Select(x => new RegionInfo(x.LCID));

                foreach (var region in regions)
                    if (!SymbolsByCode.ContainsKey(region.ISOCurrencySymbol))
                        SymbolsByCode.Add(region.ISOCurrencySymbol, region.CurrencySymbol);
            }
        }

        public static IList CultureInfoFromCurrencyISO(string isoCode)
        {
            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            IList Result = new ArrayList();
            foreach (CultureInfo ci in cultures)
            {
                RegionInfo ri = new RegionInfo(ci.LCID);
                if (ri.ISOCurrencySymbol == isoCode)
                {
                    if (!Result.Contains(ci))
                        Result.Add(ci);
                }
            }
            return Result;
        }
    }
}