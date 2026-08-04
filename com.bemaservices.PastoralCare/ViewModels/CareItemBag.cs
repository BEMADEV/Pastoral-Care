using System;
using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace com.bemaservices.PastoralCare.ViewModels
{
    /// <summary>
    /// The item details for the Care Item Detail block.
    /// </summary>
    public class CareItemBag : EntityBagBase
    {
        public DateTime? ContactDateTime { get; set; }

        public ListItemBag ContactorPersonAlias { get; set; }

        public List<string> CareTypeNames { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public ListItemBag LastCareContact { get; set; }

        public ListItemBag PersonAlias { get; set; }
    }
}
