﻿using Aspose.Cells;
using MvcPlatform.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MvcPlatform.Controllers
{
    [Authorize]
    public class EnterpriseOrderController : Controller
    {
        int totalProperty = 0;

        public ActionResult EntOrderList()  //文件委托
        {
            ViewBag.navigator = "企业服务>>委托任务";
            ViewBag.IfLogin = !string.IsNullOrEmpty(HttpContext.User.Identity.Name);
            return View();
        }
        public ActionResult EntOrder_Detail()  //文件委托
        {
            //ViewBag.navigator = "企业服务>>委托任务";
            //ViewBag.IfLogin = !string.IsNullOrEmpty(HttpContext.User.Identity.Name);
            return View();
        }
        public ActionResult EnterpriseHome()//企业服务
        {
            ViewBag.navigator = "企业服务>>委托中心";
            ViewBag.IfLogin = !string.IsNullOrEmpty(HttpContext.User.Identity.Name);
            return View();
        }

        public ActionResult ProcessOrder()//委托任务
        {
            ViewBag.navigator = "客户服务>>委托任务";
            ViewBag.IfLogin = !string.IsNullOrEmpty(HttpContext.User.Identity.Name);
            return View();
        }
        public ActionResult ProcessServer()//委托服务
        {
            ViewBag.navigator = "客户服务>>委托服务";
            ViewBag.IfLogin = !string.IsNullOrEmpty(HttpContext.User.Identity.Name);
            return View();
        }

        public string GetCode(string combin)
        {
            if (string.IsNullOrEmpty(combin))
            {
                return "";
            }
            else
            {
                int start = combin.LastIndexOf("(");
                int end = combin.LastIndexOf(")");
                return combin.Substring(start + 1, end - start - 1);
            }
        }

        public string GetName(string combin)
        {
            if (string.IsNullOrEmpty(combin))
            {
                return "";
            }
            else
            {
                int index = combin.LastIndexOf("(");
                return combin.Substring(0, index);
            }
        }

        public string Ini_Base_Data_REPWAY()
        {
            string sql = "";
            string json_sbfs = "[]";//申报方式
            string busitype = Request["busitype"];

            sql = "select CODE,NAME||'('||CODE||')' NAME from SYS_REPWAY where Enabled=1 and instr(busitype,'" + busitype + "')>0";
            json_sbfs = JsonConvert.SerializeObject(DBMgrBase.GetDataTable(sql));

            return "{sbfs:" + json_sbfs + "}";
        }

        public string loadform()
        {
            Object json_user = Extension.Get_UserInfo(HttpContext.User.Identity.Name);
            string ID = Request["ID"]; //string busitype = Request["busitype"];

            DataTable dt;
            string data = "{}", filedata = "[]";
            IsoDateTimeConverter iso = new IsoDateTimeConverter();//序列化JSON对象时,日期的处理格式
            iso.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            if (string.IsNullOrEmpty(ID))//如果为空、即新增的时候
            {
                string result = "{STATUS:0}";//string result = "{STATUS:0,BUSITYPEID:'" + busitype + "'}";
                return "{data:" + result + ",filedata:" + filedata + "}";
            }
            else
            {
                string sql = "select a.* from ENT_ORDER a where a.ID = '" + ID + "'";
                dt = DBMgr.GetDataTable(sql);
                data = JsonConvert.SerializeObject(dt, iso).TrimStart('[').TrimEnd(']');
                //随附文件
                sql = "select * from list_attachment t where t.entid='" + ID + "'";
                DataTable dt_detail = DBMgr.GetDataTable(sql);
                return "{data:" + data + ",filedata:" + JsonConvert.SerializeObject(dt_detail, iso) + "}";
            }
        }
        public ActionResult Upload_WebServer(int? chunk, string name)
        {
            var fileUpload = Request.Files[0];
            var uploadPath = Server.MapPath("/FileUpload/file");
            chunk = chunk ?? 0;
            using (var fs = new FileStream(Path.Combine(uploadPath, name), chunk == 0 ? FileMode.Create : FileMode.Append))
            {
                var buffer = new byte[fileUpload.InputStream.Length];
                fileUpload.InputStream.Read(buffer, 0, buffer.Length);
                fs.Write(buffer, 0, buffer.Length);
            }
            return Content("chunk uploaded", "text/plain");
        }
        public string Save()
        {
            try
            {
                JObject json_user = Extension.Get_UserInfo(HttpContext.User.Identity.Name);
                JObject json_data = (JObject)JsonConvert.DeserializeObject(Request["data"]);
                string filedata = Request["filedata"] + "";
                string insert_sql = "";
                string update_sql = "";
                string sql = "";
                string ent_id = "";
                if (string.IsNullOrEmpty(json_data.Value<string>("ID")))//新增
                {
                    insert_sql = @"insert into ENT_ORDER (id,createtime, unitcode,filerecevieunitcode, filerecevieunitname,
                    filedeclareunitcode,filedeclareunitname, busitypeid,customdistrictcode,customdistrictname,repwayid, 
                    createid, createname,enterprisecode, enterprisename,remark,code,createmode) 
                    values ('{0}',sysdate,(select fun_AutoQYBH(sysdate) from dual),'{1}','{2}','{3}','{4}','{5}',
                     '{6}','{7}','{8}','{9}','{10}', '{11}','{12}','{13}','{14}','{15}')";
                    if (json_data.Value<string>("CREATEMODE") == "按批次")
                    {
                        sql = "select ENT_ORDER_ID.Nextval from dual";
                        ent_id = DBMgr.GetDataTable(sql).Rows[0][0] + "";//获取ID
                        sql = string.Format(insert_sql, ent_id, GetCode(json_data.Value<string>("FILERECEVIEUNITNAME")), GetName(json_data.Value<string>("FILERECEVIEUNITNAME")),
                              GetCode(json_data.Value<string>("FILEDECLAREUNITNAME")), GetName(json_data.Value<string>("FILEDECLAREUNITNAME")),
                              json_data.Value<string>("BUSITYPEID"), json_data.Value<string>("CUSTOMDISTRICTCODE"), json_data.Value<string>("CUSTOMDISTRICTNAME"),
                              json_data.Value<string>("REPWAYID"), json_user.Value<string>("ID"), json_user.Value<string>("REALNAME"),
                              json_user.Value<string>("CUSTOMERHSCODE"), json_user.Value<string>("CUSTOMERNAME"), json_data.Value<string>("REMARK"),
                              json_data.Value<string>("CODE"), json_data.Value<string>("CREATEMODE"));
                        DBMgr.ExecuteNonQuery(sql);
                        //更新随附文件
                        Extension.Update_Attachment_ForEnterprise(ent_id, filedata, json_data.Value<string>("ORIGINALFILEIDS"), json_user);
                    }
                    if (json_data.Value<string>("CREATEMODE") == "按文件")
                    {
                        JArray jarry = JsonConvert.DeserializeObject<JArray>(filedata);
                        foreach (JObject json in jarry)
                        {
                            sql = "select ENT_ORDER_ID.Nextval from dual";
                            ent_id = DBMgr.GetDataTable(sql).Rows[0][0] + "";//获取ID
                            sql = string.Format(insert_sql, ent_id, GetCode(json_data.Value<string>("FILERECEVIEUNITNAME")), GetName(json_data.Value<string>("FILERECEVIEUNITNAME")),
                                  GetCode(json_data.Value<string>("FILEDECLAREUNITNAME")), GetName(json_data.Value<string>("FILEDECLAREUNITNAME")),
                                  json_data.Value<string>("BUSITYPEID"), json_data.Value<string>("CUSTOMDISTRICTCODE"), json_data.Value<string>("CUSTOMDISTRICTNAME"),
                                  json_data.Value<string>("REPWAYID"), json_user.Value<string>("ID"), json_user.Value<string>("REALNAME"), json_data.Value<string>("REMARK"),
                                  json_user.Value<string>("CUSTOMERHSCODE"), json_user.Value<string>("CUSTOMERNAME"),
                                  json_data.Value<string>("CODE"), json_data.Value<string>("CREATEMODE"));
                            DBMgr.ExecuteNonQuery(sql);
                            //更新随附文件
                            Extension.Update_Attachment_ForEnterprise(ent_id, "[" + JsonConvert.SerializeObject(json) + "]", json_data.Value<string>("ORIGINALFILEIDS"), json_user);
                        }
                    }
                }
                else
                {
                    update_sql = @"update ENT_ORDER  set filerecevieunitcode='{1}',filerecevieunitname='{2}',filedeclareunitcode='{3}',
                    filedeclareunitname='{4}',busitypeid='{5}',customdistrictcode='{6}', customdistrictname='{7}',
                    repwayid='{8}',remark='{9}',code='{10}' where id='{0}'";
                    sql = string.Format(update_sql, json_data.Value<string>("ID"), GetCode(json_data.Value<string>("FILERECEVIEUNITNAME")),
                    GetName(json_data.Value<string>("FILERECEVIEUNITNAME")), GetCode(json_data.Value<string>("FILEDECLAREUNITNAME")),
                    GetName(json_data.Value<string>("FILEDECLAREUNITNAME")), json_data.Value<string>("BUSITYPEID"), json_data.Value<string>("CUSTOMDISTRICTCODE"),
                    json_data.Value<string>("CUSTOMDISTRICTNAME"), json_data.Value<string>("REPWAYID"), json_data.Value<string>("REMARK"), json_data.Value<string>("CODE"));
                    DBMgr.ExecuteNonQuery(sql);
                    //更新随附文件
                    Extension.Update_Attachment_ForEnterprise(json_data.Value<string>("ID"), filedata, json_data.Value<string>("ORIGINALFILEIDS"), json_user);
                }
                return "{success:true}";
            }
            catch (Exception ex)
            {
                return "{success:false}";
            }
        }

        public string Delete()
        {
            string id = Request["id"].ToString();
            string json = "{success:false}"; string sql = "";

            //删除订单随附文件
            System.Uri Uri = new Uri("ftp://" + ConfigurationManager.AppSettings["FTPServer"] + ":" + ConfigurationManager.AppSettings["FTPPortNO"]);
            string UserName = ConfigurationManager.AppSettings["FTPUserName"];
            string Password = ConfigurationManager.AppSettings["FTPPassword"];
            FtpHelper ftp = new FtpHelper(Uri, UserName, Password);
            sql = "select * from list_attachment where entid='" + id + "'";
            DataTable dt = DBMgr.GetDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                ftp.DeleteFile(dr["FILENAME"] + "");
            }

            sql = "delete from list_attachment where entid='" + id + "'";
            DBMgr.ExecuteNonQuery(sql);

            sql = "delete from ENT_ORDER where id = '" + id + "'";
            DBMgr.ExecuteNonQuery(sql);

            json = "{success:true}";
            return json;
        }

        private string GetPageSql(string tempsql, string order, string asc)
        {
            int start = Convert.ToInt32(Request["start"]);
            int limit = Convert.ToInt32(Request["limit"]);
            string sql = "select count(1) from ( " + tempsql + " )";
            totalProperty = Convert.ToInt32(DBMgr.GetDataTable(sql).Rows[0][0]);
            string pageSql = @"SELECT * FROM ( SELECT tt.*, ROWNUM AS rowno FROM ({0} ORDER BY {1} {2}) tt WHERE ROWNUM <= {4}) table_alias WHERE table_alias.rowno >= {3}";
            pageSql = string.Format(pageSql, tempsql, order, asc, start + 1, limit + start);
            return pageSql;
        }

        public string IniChart()
        {
            JObject json_user = Extension.Get_UserInfo(HttpContext.User.Identity.Name);
            string sql = "";
            DataTable dt = new DataTable();
            DataColumn dc = new DataColumn("date");
            dt.Columns.Add(dc);
            dc = new DataColumn("total");
            dt.Columns.Add(dc);
            dc = new DataColumn("submit");
            dt.Columns.Add(dc);
            DataTable tmp_dt;
            for (int i = 0; i < 7; i++)
            {
                DataRow dr = dt.NewRow();
                dr["date"] = DateTime.Now.AddDays(-i).ToShortDateString();
                sql = "select count(1) from ent_order where  to_char(CREATETIME, 'yyyy-MM-dd')=to_char(sysdate-" + i + ",'yyyy-MM-dd') and ENTERPRISECODE='" + json_user.Value<string>("CUSTOMERCODE") + "'";
                tmp_dt = DBMgr.GetDataTable(sql);
                dr["total"] = tmp_dt.Rows[0][0];
                sql = "select count(1) from ent_order where  to_char(SUBMITTIME, 'yyyy-MM-dd')=to_char(sysdate-" + i + ",'yyyy-MM-dd') and ENTERPRISECODE='" + json_user.Value<string>("CUSTOMERCODE") + "'";
                tmp_dt = DBMgr.GetDataTable(sql);
                dr["submit"] = tmp_dt.Rows[0][0];
                dt.Rows.Add(dr);
            }
            return "{result:" + JsonConvert.SerializeObject(dt) + "}";
        }


        //报关行角色查看订单暂存区数据时 加载申报单位与之对应的数据 by panhuaguo 2016-08-12
        public string LoadProcess()
        {
            JObject json_user = Extension.Get_UserInfo(HttpContext.User.Identity.Name);
            string where = "";
            if (!string.IsNullOrEmpty(Request["ENTERPRISENAME"]))//判断查询条件是否有值
            {
                where += " and t.ENTERPRISECODE='" + GetCode(Request["ENTERPRISENAME"]) + "'";
            }
            if (!string.IsNullOrEmpty(Request["CODE"]))//判断查询条件是否有值
            {
                where += " and instr(t.CODE,'" + Request["CODE"].ToString().Trim() + "')>0 ";
            }
            if (!string.IsNullOrEmpty(Request["PRINTSTATUS"]))//判断查询条件是否有值
            {
                where += " and  t.PRINTSTATUS='" + Request["PRINTSTATUS"] + "'";
            }
            if (!string.IsNullOrEmpty(Request["STARTDATE"]))//如果开始时间有值
            {
                where += " and t.SUBMITTIME>=to_date('" + Request["STARTDATE"] + "','yyyy-mm-dd hh24:mi:ss') ";
            }
            if (!string.IsNullOrEmpty(Request["ENDDATE"]))//如果结束时间有值
            {
                where += " and t.SUBMITTIME<=to_date('" + Request["ENDDATE"].Replace("00:00:00", "23:59:59") + "','yyyy-mm-dd hh24:mi:ss') ";
            }
            IsoDateTimeConverter iso = new IsoDateTimeConverter();//序列化JSON对象时,日期的处理格式
            iso.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            //string sql = @"select t.*,(select count(1) from list_attachment l where l.entid=t.ID ) FILENUM from ENT_ORDER t where t.FILEDECLAREUNITCODE='" + json_user.Value<string>("CUSTOMERHSCODE") + "'" + where;
            //2016/10/9 为了提升load效能 FILENUM获取修改为：
            string sql = @"select t.*,l.FILENUM 
                            from ENT_ORDER t 
                                left join (select entid,count(1) as FILENUM from list_attachment where entid is not null group by entid) l on t.ID=l.entid
                            where t.FILEDECLAREUNITCODE='" + json_user.Value<string>("CUSTOMERHSCODE") + "'" + where;
            DataTable dt = DBMgr.GetDataTable(GetPageSql(sql, "CREATETIME", "desc"));
            var json = JsonConvert.SerializeObject(dt, iso);
            return "{rows:" + json + ",total:" + totalProperty + "}";
        }

        //报关行在看到已提交的委托后,可以直接标记为已受理，打印或者抓取电子档文件后,可以从其他渠道进行报关报检
        public string SignProcess()
        {
            JObject json_user = Extension.Get_UserInfo(HttpContext.User.Identity.Name);
            string sql = "update ent_order set acceptid='" + json_user.Value<string>("ID") + "',acceptname='" + json_user.Value<string>("REALNAME") + "',accepttime=sysdate,status=15 where instr('" + Request["ids"] + "',ID)>0";
            int result = DBMgr.ExecuteNonQuery(sql);
            return result == 1 ? "{success:true}" : "{success:false}";
        }
        //文件申报单位维度委托任务的批量打印功能 by panhuaguo 20160929
        public string BatchPrint()
        {
            string entids = Request["entids"];
            string[] id_array = entids.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            string output = Guid.NewGuid() + "";
            string sql = "";
            DataTable dt = null;
            IList<string> filelist = new List<string>();
            foreach (string entid in id_array)
            {
                sql = @"select t.*,instr('44515250',t.filetype) as typeindex from list_attachment t  where t.entid='" + entid + "' order by typeindex ASC";
                dt = DBMgr.GetDataTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    filelist.Add(ConfigurationManager.AppSettings["AdminUrl"] + "/file/" + dr["FILENAME"]);
                }
                sql = "update ent_order set printstatus=1 where id='" + entid + "'";
                DBMgr.ExecuteNonQuery(sql);
            }
            Extension.MergePDFFiles(filelist, Server.MapPath("~/Declare/") + output + ".pdf");
            return "/Declare/" + output + ".pdf";
        }

        public FileResult DownFile(string filename, string ID)//ActionResult
        {
            string url = ""; byte[] bytes;
            if (ID != "")//文件在文件服务器上
            {
                url = ConfigurationManager.AppSettings["AdminUrl"] + "/file" + filename;
                WebClient wc = new WebClient();
                bytes = wc.DownloadData(url);
                wc.Dispose();
            }
            else//企业端上传的文件，未保存前还在当前服务器上
            {
                url = Server.MapPath(filename);
                FileStream fs = new FileStream(url, FileMode.Open);
                bytes = new byte[(int)fs.Length];
                fs.Read(bytes, 0, bytes.Length);
                fs.Close();
            }
            MemoryStream ms = new MemoryStream(bytes);
            return File(ms, "application/octet-stream", Path.GetFileName(filename));
        }
        public string load_entorder_detail()
        {
            string result = string.Empty;
            string ID = Request["ID"]; //string busitype = Request["busitype"];
            DataTable dt;
            string data = "{}";
            IsoDateTimeConverter iso = new IsoDateTimeConverter();//序列化JSON对象时,日期的处理格式
            iso.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            string sql = "select a.* from ENT_ORDER a where a.ID = '" + ID + "'";
            dt = DBMgr.GetDataTable(sql);
            data = JsonConvert.SerializeObject(dt, iso).TrimStart('[').TrimEnd(']');
            //申报数据
            string decl_data = "[]";
            string product_data = "[]";
            string file_data = "[]";
            if (dt.Rows.Count > 0)
            {
                sql = "select * from LIST_CUSDATA_FL where cusno='" + dt.Rows[0]["CODE"] + "'";
                DataTable dt2 = DBMgr.GetDataTable(sql);
                decl_data = JsonConvert.SerializeObject(dt2, iso);
                if (dt2.Rows.Count > 0)
                {
                    sql = "select * from LIST_CUSDATA_SUB_FL where PCODE='" + dt2.Rows[0]["CUSNO"] + "'";
                    DataTable dt3 = DBMgr.GetDataTable(sql);
                    product_data = JsonConvert.SerializeObject(dt3, iso);
                }
                sql = "select * from list_attachment t where t.entid='" + ID + "'";
                file_data = JsonConvert.SerializeObject(DBMgr.GetDataTable(sql), iso);
            }
            result = "{data:" + data + ",decl_data:" + decl_data + ",product_data:" + product_data + ",file_data:" + file_data + "}";
            return result;

        }

        #region excel重组
        /// <summary>
        /// 读取Excel数据并重组
        /// </summary>
        /// <param name="hasTitle"></param>
        /// <returns></returns>
        private DataTable GetDataFromExcelByConn(string filepath, bool hasTitle = false)
        {
            /* OpenFileDialog openFile = new OpenFileDialog();
             openFile.Filter = "Excel文件(*.xls)|*.xls;*.xlsx";
             openFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
             openFile.Multiselect = false;
             if (openFile.ShowDialog() == DialogResult.Cancel) return null;
             var filePath = openFile.FileName;
             string fileType = System.IO.Path.GetExtension(filePath);
             if (string.IsNullOrEmpty(fileType)) return null;
             * */



            DataTable dtExcel = ExcelToDatatalbe(Server.MapPath(filepath));


            /* string strConn = string.Format("Provider=Microsoft.Jet.OLEDB.{0}.0;" +
                             "Extended Properties=\"Excel {1}.0;HDR={2};IMEX=1;\";" +
                             "data source={3};",
                             (fileType == ".xls" ? 4 : 12), (fileType == ".xls" ? 8 : 12), (hasTitle ? "Yes" : "NO"), filePath);

             OleDbConnection conn = new OleDbConnection(strConn);
             conn.Open();

             DataTable dtSheetName = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "Table" });
             string strTableName = dtSheetName.Rows[0]["TABLE_NAME"].ToString();
             OleDbDataAdapter myCommand = null;
             string strExcel = "select * from [" + strTableName + "]";
             myCommand = new OleDbDataAdapter(strExcel, strConn);
             myCommand.Fill(ds);
             * */

            DataTable newdt = new DataTable();
            newdt.Columns.Add("BR", typeof(string));
            newdt.Columns.Add("BS", typeof(Int32));
            newdt.Columns.Add("BS1", typeof(Int32));
            for (int i = 0; i < dtExcel.Rows.Count; i++)
            {
                string bs = dtExcel.Rows[i]["REMARKS"].ToString();
                string[] bs1 = bs.Split(';');
                List<string> bsnew = new List<string>();
                for (int t1 = 0; t1 < bs1.Length; t1++)
                {
                    string bs2 = bs1[t1].ToString();
                    if (bs2.Contains("-"))
                    {
                        string bs21 = bs2.Split('-')[0].ToString();
                        string bs22 = bs2.Split('-')[1].ToString();
                        int bs211 = Convert.ToInt32(bs21.Split('(')[0].ToString());
                        int bs212 = Convert.ToInt32(bs22.Split('(')[0].ToString());
                        for (int t3 = 0; t3 < bs212 - bs211 + 1; t3++)
                        {
                            string newbs = (bs211 + t3) + "(" + bs21.Split('(')[1].ToString();
                            bsnew.Add(newbs);
                        }
                    }
                    else
                        bsnew.Add(bs2);
                }
                for (int t2 = 0; t2 < bsnew.Count(); t2++)
                {
                    string br = dtExcel.Rows[i]["PRECUSTOMS_CLEARANCE_ID"].ToString();
                    string bs3 = bsnew[t2].Split('(')[0].ToString();
                    string bs4 = bsnew[t2].Split('(')[1].ToString().Replace(")", "");
                    DataRow[] rows = newdt.Select("BR='" + br + "' and BS='" + bs3.ToString() + "'");
                    if (rows.Count() > 0)
                    {
                        DataRow drs = rows[0];
                        drs["BS1"] = (Convert.ToInt32(drs[2].ToString()) + Convert.ToInt32(bs4));
                    }
                    else
                    {
                        DataRow dr = newdt.NewRow();
                        dr["BR"] = dtExcel.Rows[i]["PRECUSTOMS_CLEARANCE_ID"].ToString();
                        dr["BS"] = bs3;
                        dr["BS1"] = bs4;
                        newdt.Rows.Add(dr);
                    }
                }
            }
            DataView dv = newdt.DefaultView;
            dv.Sort = " BR Asc,BS Asc";
            DataTable ndt = dv.ToTable();
            DataTable newbsdt = CreateNewDt(ndt);
            DataTable dt = CreateOldDt(dtExcel, newbsdt);
            return dt;

        }

        /// <summary>
        /// 重组原表，把原表中REMARKS格式重组为34061(30);34062(4)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="newdt"></param>
        /// <returns></returns>
        private DataTable CreateOldDt(DataTable dt, DataTable newdt)
        {
            string br = dt.Rows[dt.Rows.Count - 1]["PRECUSTOMS_CLEARANCE_ID"].ToString();
            //string bs=
            for (int i = dt.Rows.Count - 2; i > -1; i--)
            {
                if (br == dt.Rows[i]["PRECUSTOMS_CLEARANCE_ID"].ToString())
                {
                    dt.Rows.RemoveAt(i + 1);
                    if (i == 0)
                    {
                        DataRow[] rows = newdt.Select("BR='" + br + "' ");
                        if (rows.Count() > 0)
                        {
                            DataRow drs = rows[0];
                            dt.Rows[i]["REMARKS"] = (drs["BS"].ToString());
                        }
                    }
                }
                else
                {
                    DataRow[] rows = newdt.Select("BR='" + br + "' ");
                    if (rows.Count() > 0)
                    {
                        DataRow drs = rows[0];
                        dt.Rows[i + 1]["REMARKS"] = (drs["BS"].ToString());

                    }
                    br = dt.Rows[i]["PRECUSTOMS_CLEARANCE_ID"].ToString();
                }
            }

            return dt;
        }

        #endregion

        #region 重组临时表数据，重组后格式为：E1C1703848 34061(30);34062(4)
        /// <summary>
        /// 重组临时表数据，组合后格式为：34061(30);34062(4)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private DataTable CreateNewDt(DataTable dt)
        {
            DataTable newdt = new DataTable();
            newdt.Columns.Add("BR", typeof(string));
            newdt.Columns.Add("BS", typeof(string));
            string br = "";
            string bs = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i == 0)
                {
                    br = dt.Rows[i]["BR"].ToString();
                    bs = dt.Rows[i]["BS"].ToString() + "(" + dt.Rows[i]["BS1"].ToString() + ")" + ";";
                }
                else
                {
                    if (dt.Rows[i]["BR"].ToString() == dt.Rows[i - 1]["BR"].ToString())
                    {
                        bs += dt.Rows[i]["BS"].ToString() + "(" + dt.Rows[i]["BS1"].ToString() + ")" + ";";
                        if (i == dt.Rows.Count - 1)
                            newdt = ADDNewRow(newdt, br, bs);
                    }
                    else
                    {
                        newdt = ADDNewRow(newdt, br, bs);
                        br = dt.Rows[i]["BR"].ToString();
                        bs = dt.Rows[i]["BS"].ToString() + "(" + dt.Rows[i]["BS1"].ToString() + ")" + ";";
                        if (i == dt.Rows.Count - 1)
                            newdt = ADDNewRow(newdt, br, bs);
                    }
                }
            }
            return newdt;
        }

        /// <summary>
        /// 新增一条数据
        /// </summary>
        /// <param name="newdt"></param>
        /// <param name="br"></param>
        /// <param name="bs"></param>
        /// <returns></returns>
        private static DataTable ADDNewRow(DataTable newdt, string br, string bs)
        {
            DataRow dr = newdt.NewRow();
            dr["BR"] = br;
            dr["BS"] = bs.Substring(0, bs.Length - 1).ToString();
            newdt.Rows.Add(dr);
            return newdt;
        }

        public DataTable ExcelToDatatalbe(string filePath)
        {
            Workbook book = new Workbook();
            book.Open(filePath);
            Worksheet sheet = book.Worksheets[0];
            Cells cells = sheet.Cells;
            DataTable dt_Import = cells.ExportDataTableAsString(0, 0, cells.MaxDataRow + 1, cells.MaxDataColumn + 1, true);//获取excel中的数据保存到一个datatable中
            return dt_Import;
        }


        public string UploadExcel()
       {
        HttpPostedFileBase file = Request.Files[0];
            string oldfilename = Path.GetFileName(file.FileName);//file.FileName:IE返回全路径，其他浏览器只返回文件名称
            if (!Directory.Exists(Server.MapPath("~/FileUpload/Excel")))
            {
                Directory.CreateDirectory(Server.MapPath("~/FileUpload/Excel"));
            }
            string filename = oldfilename.Insert(oldfilename.LastIndexOf("."), "_" + DateTime.Now.ToString("yyyyMMddhhmmss"));
            string newfile = @"~/FileUpload/Excel/" + filename;
            string filepath = Server.MapPath(newfile);
            file.SaveAs(filepath);


            string json = "{\"filepath\":'" + (@"\/FileUpload\/Excel\/" + filename) + "',\"filename\":'" + filename + "'}";
            return json;
       
       }
        public FileResult ExportStu2()
        {
            string filepath = Request["filepath"];
            DataTable dt = GetDataFromExcelByConn(filepath);
            

            NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
            //添加一个导出成功sheet
            NPOI.SS.UserModel.ISheet sheet_S = book.CreateSheet("导入成功");

            //给sheet1添加第一行的头部标题
            NPOI.SS.UserModel.IRow row1 = sheet_S.CreateRow(0);
            for (int j = 0; j < dt.Columns.Count; j++)
            {
                row1.CreateCell(j).SetCellValue(dt.Columns[j].ColumnName);
            }
           


            //将数据逐步写入sheet_S各个行
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                NPOI.SS.UserModel.IRow rowtemp = sheet_S.CreateRow(i + 1);
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    rowtemp.CreateCell(j).SetCellValue(dt.Rows[i][j].ToString());
                }
            }


            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, "application/vnd.ms-excel", "PPAF_abc.xls");

        }
        #endregion

    }
}
