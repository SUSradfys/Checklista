using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Checklist
{
    public class ChecklistItem
    {
        private string shortInfo;
        private string detailedInfo;
        private string shortResult;
        private string detailedResult;
        private bool status;
        private string autoCheckStatus;

        public ChecklistItem(string shortInfo, string detailedInfo, string shortResult, string detailedResult, AutoCheckStatus autoCheckStatus)
        {
            this.shortInfo = shortInfo;
            this.detailedInfo = detailedInfo;
            this.shortResult = shortResult;
            this.detailedResult = detailedResult;
            this.autoCheckStatus = autoCheckStatus.ToString();
        }

        public ChecklistItem(string shortInfo, string detailedInfo, string shortResult, AutoCheckStatus autoCheckStatus)
            : this(shortInfo, detailedInfo, shortResult, null, autoCheckStatus)
        {
        }

        public ChecklistItem(string deliminatorTitle)
        {
            this.shortInfo = deliminatorTitle;
            this.detailedInfo = "DELIMINATOR";
            this.autoCheckStatus = "NONE";
        }

        public string ShortInfo { get { return shortInfo; } }
        public string DetailedInfo { get { return detailedInfo; } }
        public string ShortResult { get { return shortResult; } }
        public string DetailedResult { get { return detailedResult; } }
        public bool Status { get { return status; } set { status = value; } }
        public string AutoCheckStatus { get { return autoCheckStatus; } }
    }
}
