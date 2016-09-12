﻿using MvcPlatform.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcPlatform.Controllers
{
    public class EnterpriseOrderController : Controller
    {
        int totalProperty = 0;
        //
        // GET: /EnterpriseOrder/

        public ActionResult EntOrderList()  //文件委托
        {
            return View();
        }

        public ActionResult EnterpriseHome()//企业服务
        {
            return View();
        }

        public ActionResult ProcessOrder()//订单暂存区
        {
            return View();
        }

        #region 文件委托
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

        public string LoadList()
        {
            JObject json_user = Extension.Get_UserInfo(HttpContext.User.Identity.Name);
            string where = "";
            if (!string.IsNullOrEmpty(Request["busitype"]))//判断查询条件是否有值
            {
                where += " and t.BUSITYPEID='" + Request["busitype"].ToString() + "'";
            }
            if (!string.IsNullOrEmpty(Request["FILERECEVIEUNIT"]))//判断查询条件是否有值
            {
                string FILERECEVIEUNIT = Request["FILERECEVIEUNIT"].ToString();
                where += " and t.FILERECEVIEUNITCODE='" + GetCode(FILERECEVIEUNIT) + "' and FILERECEVIEUNITNAME='" + GetName(FILERECEVIEUNIT) + "' ";
            }
            if (!string.IsNullOrEmpty(Request["FILEDECLAREUNIT"]))//判断查询条件是否有值
            {
                string FILEDECLAREUNIT = Request["FILEDECLAREUNIT"].ToString();
                where += " and t.FILEDECLAREUNITCODE='" + GetCode(FILEDECLAREUNIT) + "' and FILEDECLAREUNITNAME='" + GetName(FILEDECLAREUNIT) + "' ";
            }
            if (!string.IsNullOrEmpty(Request["CODE"]))//判断查询条件是否有值
            {
                string CODE = Request["CODE"].ToString().Trim();
                where += " and instr(t.CODE,'" + CODE + "')>0 ";
            }

            if (!string.IsNullOrEmpty(Request["STATUS_S"]))//判断查询条件是否有值
            {
                where += " and t.STATUS='" + Request["STATUS_S"].ToString().Trim() + "'";
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

            string sql = @"select t.ID, t.FILERECEVIEUNITCODE, t.FILERECEVIEUNITNAME, t.FILEDECLAREUNITCODE, t.FILEDECLAREUNITNAME, t.BUSITYPEID
                            , t.CUSTOMDISTRICTCODE, t.CUSTOMDISTRICTNAME, t.REPWAYID, t.STATUS
                            ,t.CODE, t.CREATEID, t.CREATENAME, t.CREATETIME, t.ASSOCIATENO, t.ORDERCODE, t.ENTERPRISECODE, t.ENTERPRISENAME, t.ACCEPTID, t.ACCEPTNAME
                            ,t.ACCEPTTIME, t.UNITCODE, t.CREATEMODE, t.REMARK 
                        from ENT_ORDER t                
                        where ENTERPRISECODE='" + json_user.Value<string>("CUSTOMERCODE") + "' " + where;
            DataTable dt = DBMgr.GetDataTable(GetPageSql(sql,"CREATETIME","desc"));
            var json = JsonConvert.SerializeObject(dt, iso);
            return "{rows:" + json + ",total:" + totalProperty + "}";
        }

        public string loadform()
        {
            Object json_user = Extension.Get_UserInfo(HttpContext.User.Identity.Name);
            string ID = Request["ID"]; string busitype = Request["busitype"];

            DataTable dt;
            string data = "{}", filedata = "[]";
            IsoDateTimeConverter iso = new IsoDateTimeConverter();//序列化JSON对象时,日期的处理格式
            iso.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            if (string.IsNullOrEmpty(ID))//如果为空、即新增的时候
            {
                string result = "{STATUS:0,BUSITYPEID:'" + busitype + "'}";
                return "{data:" + result + ",filedata:" + filedata + "}";
            }
            else
            {
                string sql = @"select id, code
                                    , case  when length(filerecevieunitname || '(' || filerecevieunitcode || ')')=2 then '' || '' else filerecevieunitname || '(' || filerecevieunitcode || ')' end as filerecevieunit
                                    , filerecevieunitname
                                    , case  when length(filedeclareunitname||'('||filedeclareunitcode||')')=2 then '' || '' else  filedeclareunitname||'('||filedeclareunitcode||')' end as filedeclareunit
                                    , filedeclareunitname
                                    , busitypeid, customdistrictcode, customdistrictname, repwayid, status, files, associateno
                                    , createid, createname, createtime, enterprisecode, enterprisename ,remark, code, isupload
                                    , unitcode, createmode
                                from ENT_ORDER a where ID = '" + ID + "'";

                dt = DBMgr.GetDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    data = JsonConvert.SerializeObject(dt, iso).TrimStart('[').TrimEnd(']');
                    filedata = dt.Rows[0]["FILES"].ToString();
                }
                return "{data:" + data + ",filedata:" + filedata + "}";
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
            JObject json_user = Extension.Get_UserInfo(HttpContext.User.Identity.Name);

            string ID = Request["ID"].ToString();
            JObject json_data = (JObject)JsonConvert.DeserializeObject(Request["data"]);
            string filedata = Request["filedata"].ToString();

            string CREATEMODE = json_data.Value<string>("CREATEMODE");            

            string AssociateNo = "";//两单关联号          
            string insert_sql = "", update_sql = "", sql = "";

            string exe_desc = ""; int order_res = 0;//订单保存时记录各订单的执行情况

            if (string.IsNullOrEmpty(ID))//新增
            {
                DataTable dtid = new DataTable();
                insert_sql = @"insert into ENT_ORDER (id,  filerecevieunitcode, filerecevieunitname, filedeclareunitcode, filedeclareunitname, busitypeid
                                            , customdistrictcode, customdistrictname, repwayid, status, files
                                            , associateno, createid, createname, createtime, enterprisecode, enterprisename
                                            , remark, code, unitcode, createmode) 
                        values ('{0}','{1}','{2}','{3}','{4}','{5}'
                                ,'{6}','{7}','{8}','{9}','{10}'
                                ,'{11}','{12}','{13}',sysdate,'{14}','{15}'
                                ,'{16}','{17}',(select fun_AutoQYBH(sysdate) from dual),'{18}')";

                if (CREATEMODE == "按批次")
                {
                    dtid = DBMgr.GetDataTable("select ENT_ORDER_ID.Nextval from dual");
                    ID = dtid.Rows[0][0].ToString();

                    sql = string.Format(insert_sql, ID, GetCode(json_data.Value<string>("FILERECEVIEUNIT")), GetName(json_data.Value<string>("FILERECEVIEUNIT")), GetCode(json_data.Value<string>("FILEDECLAREUNIT")), GetName(json_data.Value<string>("FILEDECLAREUNIT")), json_data.Value<string>("BUSITYPEID")
                       , json_data.Value<string>("CUSTOMDISTRICTCODE"), json_data.Value<string>("CUSTOMDISTRICTNAME"), json_data.Value<string>("REPWAYID"), json_data.Value<string>("STATUS"), filedata
                       , AssociateNo, json_user.Value<string>("ID"), json_user.Value<string>("REALNAME"), json_user.Value<string>("CUSTOMERCODE"), json_user.Value<string>("CUSTOMERNAME")
                       , json_data.Value<string>("REMARK"), json_data.Value<string>("CODE"), CREATEMODE
                       );
                    order_res = DBMgr.ExecuteNonQuery(sql);
                }

                if (CREATEMODE == "按文件")
                {
                    JArray jarry = JsonConvert.DeserializeObject<JArray>(filedata);
                    foreach (JObject json in jarry)
                    {
                        dtid = DBMgr.GetDataTable("select ENT_ORDER_ID.Nextval from dual");
                        ID = dtid.Rows[0][0].ToString();

                        sql = string.Format(insert_sql, ID, GetCode(json_data.Value<string>("FILERECEVIEUNIT")), GetName(json_data.Value<string>("FILERECEVIEUNIT")), GetCode(json_data.Value<string>("FILEDECLAREUNIT")), GetName(json_data.Value<string>("FILEDECLAREUNIT")), json_data.Value<string>("BUSITYPEID")
                                                    , json_data.Value<string>("CUSTOMDISTRICTCODE"), json_data.Value<string>("CUSTOMDISTRICTNAME"), json_data.Value<string>("REPWAYID"), json_data.Value<string>("STATUS"), "[" + JsonConvert.SerializeObject(json) + "]"
                                                    , AssociateNo, json_user.Value<string>("ID"), json_user.Value<string>("REALNAME"), json_user.Value<string>("CUSTOMERCODE"), json_user.Value<string>("CUSTOMERNAME")
                                                    , json_data.Value<string>("REMARK"), json_data.Value<string>("CODE"), CREATEMODE
                                                    );
                        order_res = DBMgr.ExecuteNonQuery(sql);
                    }
                }


            }
            else
            {
                update_sql = @"update ENT_ORDER  set filerecevieunitcode='{1}',filerecevieunitname='{2}',filedeclareunitcode='{3}',filedeclareunitname='{4}',busitypeid='{5}',customdistrictcode='{6}',
                            customdistrictname='{7}',repwayid='{8}' ,files='{9}',associateno='{10}',createid='{11}',
                            createname='{12}',createtime=sysdate,enterprisecode='{13}',enterprisename='{14}',remark='{15}',code='{16}' where id='{0}'";
                sql = string.Format(update_sql, ID, GetCode(json_data.Value<string>("FILERECEVIEUNIT")), GetName(json_data.Value<string>("FILERECEVIEUNIT")), GetCode(json_data.Value<string>("FILEDECLAREUNIT"))
                        , GetName(json_data.Value<string>("FILEDECLAREUNIT")), json_data.Value<string>("BUSITYPEID"), json_data.Value<string>("CUSTOMDISTRICTCODE")
                        , json_data.Value<string>("CUSTOMDISTRICTNAME"), json_data.Value<string>("REPWAYID"), filedata, AssociateNo, json_user.Value<string>("ID")
                        , json_user.Value<string>("REALNAME"), json_user.Value<string>("CUSTOMERCODE"), json_user.Value<string>("CUSTOMERNAME")
                        , json_data.Value<string>("REMARK"), json_data.Value<string>("CODE")
                        );
                order_res = DBMgr.ExecuteNonQuery(sql);
            }


            if (order_res == 0)
            {
                exe_desc = "保存失败!";
            }

            return "{ID:'" + ID + "',result:'" + exe_desc + "'}";
        }

        public string Delete()
        {
            string ids = Request["id"].ToString().TrimEnd(',');
            string json = "{success:false}";

            string sql = "delete from ENT_ORDER where id in (" + ids + ")";
            int i = DBMgr.ExecuteNonQuery(sql);
            if (i > 0)
            {
                json = "{success:true}";
            }
            
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

        #endregion

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
            if (!string.IsNullOrEmpty(Request["FILERECEVIEUNIT"]))//判断查询条件是否有值
            {
                where += " and t.FILERECEVIEUNITCODE='" + GetCode(Request["FILERECEVIEUNIT"]) + "'";
            }
            if (!string.IsNullOrEmpty(Request["CODE"]))//判断查询条件是否有值
            {
                where += " and instr(t.CODE,'" + Request["CODE"].ToString().Trim() + "')>0 ";
            }
            if (!string.IsNullOrEmpty(Request["STATUS"]))//判断查询条件是否有值
            {
                where += " and  t.STATUS= " + Request["STATUS"];
            }
            if (!string.IsNullOrEmpty(Request["STARTDATE"]))//如果开始时间有值
            {
                where += " and t.SUBMITTIME>=to_date('" + Request["VALUE4_1"] + "','yyyy-mm-dd hh24:mi:ss') ";
            }
            if (!string.IsNullOrEmpty(Request["ENDDATE"]))//如果结束时间有值
            {
                where += " and t.SUBMITTIME<=to_date('" + Request["VALUE4_2"].Replace("00:00:00", "23:59:59") + "','yyyy-mm-dd hh24:mi:ss') ";
            }
            IsoDateTimeConverter iso = new IsoDateTimeConverter();//序列化JSON对象时,日期的处理格式
            iso.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            string sql = @"select * from ENT_ORDER t where t.FILEDECLAREUNITCODE='" + json_user.Value<string>("CUSTOMERHSCODE") + "'" + where;
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

    }
}