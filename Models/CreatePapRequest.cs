using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace DupdGrowth.Web.Models
{
    public class CreatePapRequest
    {
        public int PriorityNo { get; set; }
        public string PapName { get; set; } = string.Empty;
        public string ResponsiblePerson { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public string TimeFrameStart { get; set; } = string.Empty;
        public string TimeFrameEnd { get; set; } = string.Empty;
        public string SupportOffice { get; set; } = string.Empty;
        public string? AlignmentGrowth { get; set; }
        public string? AlignmentAchieve { get; set; }
        public string RemarksType { get; set; } = string.Empty;
        public string? RemarksTypeOther { get; set; }
        public string? Remarks { get; set; }
        public string? StatusQ1 { get; set; }
        public string? StatusQ2 { get; set; }
        public string? StatusQ3 { get; set; }
        public string? StatusQ4 { get; set; }
    }

    public class Projects
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int Priority { get; set; }
        public DateTime Deadline { get; set; }
        public List<string> Responsibilities { get; set; }
        public List<string> TeamMembers { get; set; }
    }
}