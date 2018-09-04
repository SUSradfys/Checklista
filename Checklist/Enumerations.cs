using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Checklist
{
    public enum ChecklistType
    {
        Eclipse,
        [Description("Eclipse (VMAT)")]
        EclipseVMAT,
        [Description("Eclipse (gating)")]
        EclipseGating,
        [Description("Eclipse (ConformalArc)")]
        EclipseConformal,
        MasterPlan,
        [Description("MasterPlan (IMRT)")]
        MasterPlanIMRT
        //[Description("Fullständig")]
        //Complete
    }

    public enum TreatmentUnitManufacturer
    {
        Varian,
        Elekta,
        Multiple,
        None,
        Unknown
    }

    public enum TreatmentSide
    {
        MinusX,
        PlusX,
        Unknown,
    }

    public enum AutoCheckStatus
    {
        PASS,
        FAIL,
        WARNING,
        MANUAL,
        UNKNOWN
    }

    public static class Extensions
    {
        public static bool Passed(this AutoCheckStatus checkStatus)
        {
            return checkStatus == AutoCheckStatus.PASS;
        }
    }
}
