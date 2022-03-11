using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.Util;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.MFG.App;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LM_BOM
{
    [HotUpdate]
    public class LM_BOMList : AbstractDynamicFormPlugIn
    {
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            int index = this.Model.GetEntryRowCount("FTreeEntity");//获取总行数
            int idx = this.Model.GetEntryCurrentRowIndex("FTreeEntity");//获取当前行

            //判断 单据状态为新增 修改时|| this.View.OpenParameter.Status.Equals(OperationStatus.EDIT) 
            if (this.View.OpenParameter.Status.Equals(OperationStatus.ADDNEW))
            {
                //当物子项料不为空的时候
                if (this.Model.GetValue("FMATERIALIDCHILD", idx).IsNullOrEmptyOrWhiteSpace() == false)
                {
                    Entity entity = this.View.BillBusinessInfo.GetEntity("FTreeEntity");


                    DynamicObject FMATERIALID = this.Model.GetValue("FMATERIALIDCHILD", idx) as DynamicObject;
                    string Fmaterialid = FMATERIALID[0].ToString();
                    //这个是取得 采购订单 该物料最新采购价 （应该改为获取该物料 应付单对应物料的含税单价）
                    //string sql = string.Format(@"select  
                    //                         pe.FMATERIALID
                    //                        , FUNITID 
                    //                        , FTAXPRICE 
                    //                        , FDATE
                    //                        from t_PUR_POOrderEntry pe
                    //                        join t_PUR_POOrder p on p.FID = pe.FID
                    //                        join T_PUR_POORDERENTRY_F pef on pef.FENTRYID = pe.FENTRYID
                    //                        where
                    //                        FMATERIALID = {0}
                    //                        order by FDATE desc", Fmaterialid);
                    string sql = string.Format(@"select  top 1
                                            FMATERIALID
                                            , FPRICEUNITID
                                            , FTAXPRICE
                                            , FDATE
                                            from T_AP_PAYABLEENTRY pe
                                            join T_AP_PAYABLE p on p.FID = pe.FID and p.FDOCUMENTSTATUS = 'C'
                                            where FMATERIALID = {0}
                                            order by FDATE desc", Fmaterialid);
                    DataSet ds = DBServiceHelper.ExecuteDataSet(this.Context, sql);
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        this.Model.SetValue("F_ORA_ZXMXLASTAMOUNT", dt.Rows[i]["FTAXPRICE"], idx);//变换前采购价
                        this.Model.SetValue("F_ORA_FINALPRICE", dt.Rows[i]["FTAXPRICE"], idx);//变换后采购价
                        //this.View.InvokeFieldUpdateService("FUNITIDLOT", index);//获取值更新
                    }
                }
            }
            else
            {
                for (int i = 0; i < index; i++)
                {
                    Entity entity = this.View.BillBusinessInfo.GetEntity("FTreeEntity");
                    string Funitid = "";
                    string Fmaterialid = "";
                    //this.View.InvokeFieldUpdateService("FUNITID",     index);//获取值更新

                    //取应付单 的含税单价 计价单位 根据 这些  去查单位转化列表 对应物料 对应目标原单位的转化率 
                    //获取 物料 和 后单位
                    if (this.Model.GetValue("FMATERIALIDCHILD", i).IsNullOrEmptyOrWhiteSpace() == true || this.Model.GetValue("FCHILDUNITID", i).IsNullOrEmptyOrWhiteSpace() == true)
                    {
                        Fmaterialid = "";
                        Funitid = "";
                    }
                    else
                    {
                        DynamicObject FMATERIALID = this.Model.GetValue("FMATERIALIDCHILD", i) as DynamicObject;
                        Fmaterialid = FMATERIALID[0].ToString();
                        DynamicObject FUNITIID = this.Model.GetValue("FCHILDUNITID", i) as DynamicObject;
                        Funitid = FUNITIID[0].ToString();

                        //string sqlAmount = $@"select top 1  FMATERIALID as 物料,FUNITID  ,FTAXPRICE  ,FDATE as 日期
                        //                from t_PUR_POOrder p
                        //                join t_PUR_POOrderEntry pe on p.FID = pe.FID
                        //                join T_PUR_POORDERENTRY_F pef on pef.FENTRYID = pe.FENTRYID
                        //                where FMATERIALID = {Fmaterialid}
                        //                order by FDATE desc";

                        string sqlAmount = $@"select  top 1 FMATERIALID ,FPRICEUNITID ,FTAXPRICE  ,FDATE
                                            from T_AP_PAYABLEENTRY pe
                                            join T_AP_PAYABLE p on p.FID = pe.FID and p.FDOCUMENTSTATUS = 'C'
                                            where FMATERIALID = {Fmaterialid}
                                            order by FDATE desc";
                        DataSet dsAmount = DBServiceHelper.ExecuteDataSet(this.Context, sqlAmount);
                        DataTable dtAmount = dsAmount.Tables[0];
                        string DW = "";
                        string Amount = "";
                        for (int l = 0; l < dtAmount.Rows.Count; l++)
                        {
                            Amount = dtAmount.Rows[0]["FTAXPRICE"].ToString();//更新前最新采购价
                            DW = dtAmount.Rows[0]["FPRICEUNITID"].ToString();//采购单位
                            

                        }
                        decimal zje;
                        string sql = $@"EXEC [dbo].[LM_Bom] '{Fmaterialid}','{Funitid}'";//{Funitid}
                        DataSet ds = DBServiceHelper.ExecuteDataSet(this.Context, sql);
                        DataTable dt = ds.Tables[0];

                        if (dt.Rows.Count > 0)
                        {
                            if (Funitid == "10101")
                            {
                                this.Model.SetValue("F_ORA_ZXMXLASTAMOUNT", Amount, i);
                                this.Model.SetValue("F_ORA_BESTNEWUNITID", Funitid, i);//最新采购价单位
                                this.View.InvokeFieldUpdateService("F_ORA_ZXMXLASTAMOUNT",i);

                            }
                            else
                            {
                                for (int j = 0; j < dt.Rows.Count; j++)
                                {
                                    if (Amount == "")
                                    {
                                        this.Model.SetValue("F_ORA_FINALPRICE", 0, i);//更新前金额
                                        this.Model.SetValue("F_ORA_ZXMXLASTAMOUNT", 0, i);//更新后金额
                                        this.Model.SetValue("F_ORA_BESTNEWUNITID", Funitid, i);//最新采购价单位
                                        this.View.InvokeFieldUpdateService("F_ORA_ZXMXLASTAMOUNT", i);
                                    }
                                    else
                                    {
                                        decimal zhl = Convert.ToDecimal(dt.Rows[j]["zhl"]);
                                        zje = Convert.ToDecimal(Amount) / zhl;
                                        string Funit = dt.Rows[j]["FCURRENTUNITID"].ToString();
                                        this.Model.SetValue("F_ORA_ZXMXLASTAMOUNT", zje, i);
                                        //this.Model.SetValue("CHILDUNITID", Funitid, i );//子项单位
                                        this.Model.SetValue("F_ORA_BESTNEWUNITID", Funitid, i);//最新采购价单位
                                        this.View.InvokeFieldUpdateService("F_ORA_ZXMXLASTAMOUNT", i);
                                    }
                                }
                            }
                        }
                        else
                        {
                            //当金额为空的时候 赋值 0
                            if (Amount == "")
                            {
                                this.Model.SetValue("F_ORA_FINALPRICE", 0, i);//更新前金额
                                this.Model.SetValue("F_ORA_ZXMXLASTAMOUNT", 0, i);//更新后金额
                                this.Model.SetValue("F_ORA_BESTNEWUNITID", Funitid, i);//最新采购价单位
                                this.View.InvokeFieldUpdateService("F_ORA_ZXMXLASTAMOUNT", i);

                            }
                            else
                            {
                                this.Model.SetValue("F_ORA_FINALPRICE", Amount, i);
                                this.Model.SetValue("F_ORA_ZXMXLASTAMOUNT", Amount, i);
                                this.Model.SetValue("F_ORA_BESTNEWUNITID", Funitid, i);//最新采购价单位
                                this.View.InvokeFieldUpdateService("F_ORA_ZXMXLASTAMOUNT", i);
                            }
                        }
                    }
                }
                if (this.Model.GetValue("FNUMBER").IsNullOrEmptyOrWhiteSpace() == false)
                {
                    int sumqty;
                    string FNumber = this.Model.GetValue("FNUMBER").ToString();
                    string sqla = string.Format(@"select sum(F_ORA_STANDARDWORKHOURS) as am from T_ENG_BOMLABORENTRY be
                                        join T_ENG_BOM b on b.FID = be.FID
                                        where FNUMBER = '{0}'", FNumber);
                    DataSet dsa = DBServiceHelper.ExecuteDataSet(this.Context, sqla);
                    DataTable dta = dsa.Tables[0];
                    for (int i = 0; i < dta.Rows.Count; i++)
                    {
                        if (dta.Rows[0]["am"].ToString() == "")
                        {
                            sumqty = 0;
                            this.Model.SetValue("F_ORA_SUMWORKHOURQTY", sumqty);
                            this.Model.SetValue("F_ORA_COMBO", 60);
                        }
                        else
                        {
                            this.Model.SetValue("F_ORA_SUMWORKHOURQTY", dta.Rows[0]["am"]);
                            this.Model.SetValue("F_ORA_COMBO", 60);
                        }

                    }
                }
            }
        }
        public override void DataChanged(DataChangedEventArgs e)
        {

            if (e.Field.Key.Equals("FCHILDUNITID"))
            {
                var mat = this.Model.GetValue("FMATERIALIDCHILD", e.Row) as DynamicObject;
                if (mat.IsNullOrEmpty())
                {
                    //this.View.ShowMessage("Test1");
                    return;
                }

                string sql = $@"SELECT TOP 1 P_E.FMATERIALID, P_E.FPRICEUNITID, P_E.FTAXPRICE
                                FROM
	                                T_AP_PAYABLE P
	                                JOIN T_AP_PAYABLEENTRY P_E ON P.FID = P_E.FID
                                WHERE
	                                P.FDOCUMENTSTATUS = 'C'
	                                AND P_E.FMATERIALID = {mat[0]}
                                ORDER BY
	                                FENTRYID DESC";
                var result = DBServiceHelper.ExecuteDataSet(Context, sql);
                if (result.Tables[0].Rows.Count > 0)
                {
                    var dt = result.Tables[0];
                    //var desUnit = e.NewValue as DynamicObject;
                    if (e.NewValue.IsNullOrEmpty())
                    {
                        //this.View.ShowMessage(e.NewValue.ToString());
                        return;
                    }

                    var desUnit = Convert.ToInt64(e.NewValue);
                    var rate = getBasePolicyRate(Convert.ToInt64(mat[0])
                        , Convert.ToInt64(mat["msterID"])
                        , Convert.ToInt64(dt.Rows[0]["FPRICEUNITID"])
                        , Convert.ToInt64(desUnit));

                    var price = Convert.ToDecimal(dt.Rows[0]["FTAXPRICE"]) / rate;

                    this.Model.SetValue("F_ora_ZXMXLastAmount", price, e.Row);
                    this.View.UpdateView("F_ora_ZXMXLastAmount");
                }

            }

            base.DataChanged(e);
        }

        private decimal getBasePolicyRate(long materialId, long masterId, long srcUnitId, long desUnitId)
        {
            GetUnitConvertRateArgs param = new GetUnitConvertRateArgs
            {
                MasterId = masterId,
                SourceUnitId = srcUnitId,
                DestUnitId = desUnitId
            };
            return AppServiceContext.BDService.GetUnitConvertRate(base.Context, param).ConvertQty(1M, "");
        }
    }
}
