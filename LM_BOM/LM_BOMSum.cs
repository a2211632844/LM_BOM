using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LM_BOM
{
    [HotUpdate]
    public class LM_BOMSum : AbstractDynamicFormPlugIn
    {
        //public override void DataChanged(DataChangedEventArgs e)
        //{
        //    base.DataChanged(e);
        //    //this.Model.GetValue("F_ORA_SUMWORKHOURQTY").IsNullOrEmptyOrWhiteSpace()==true
        //    //
        //    if (this.View.OpenParameter.Status.Equals(OperationStatus.ADDNEW) || this.View.OpenParameter.Status.Equals(OperationStatus.EDIT))
        //    {
        //        int index = this.Model.GetEntryRowCount("F_ora_Entity1");
        //        string WorkHour = "";//标准工时
        //        decimal ALLsum = 0;
        //        for (int i = 0; i < index; i++)
        //        {
        //            //F_ORA_WORKHOUR 工时单位 //下拉列表
        //            string Time = this.View.Model.GetValue("F_ORA_WORKHOUR", i).ToString();//时分秒
        //            WorkHour = this.Model.GetValue("F_ORA_STANDARDWORKHOURS", i).ToString();//获取标准工时
        //            decimal sum = Convert.ToDecimal(Time) * Convert.ToDecimal(WorkHour);
        //            this.Model.SetValue("F_ORA_WORKHOURSECOND", sum, i);
        //            ALLsum += sum;
        //        }
        //        this.Model.SetValue("F_ORA_SUMWORKHOURQTY", ALLsum);

        //        //先获取单据头时分秒下拉框选项
        //        string HeadWorkHour = this.View.Model.GetValue("F_ora_Combo").ToString();
        //        decimal check = 0;
        //        if (HeadWorkHour == "3600")
        //        {
        //            check = ALLsum / 3600;
        //            this.Model.SetValue("F_ORA_SUMWORKHOURQTY", check);
        //        }
        //        if (HeadWorkHour == "60")
        //        {
        //            check = ALLsum / 60;
        //            this.Model.SetValue("F_ORA_SUMWORKHOURQTY", check);
        //        }
        //        if (HeadWorkHour == "1")
        //        {
        //            check = ALLsum / 1;
        //            this.Model.SetValue("F_ORA_SUMWORKHOURQTY", check);
        //        }
        //    }
        //}
    }
}
