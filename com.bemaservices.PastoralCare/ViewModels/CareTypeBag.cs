using Rock.ViewModels.Utility;

namespace com.bemaservices.PastoralCare.ViewModels
{
    /// <summary>
    /// The item details for the Care Type Detail block.
    /// </summary>
    public class CareTypeBag : EntityBagBase
    {
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public string Name { get; set; }
    }
}
