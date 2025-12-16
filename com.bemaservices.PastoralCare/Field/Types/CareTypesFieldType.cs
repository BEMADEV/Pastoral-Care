// <copyright>
// Copyright by BEMA Software Services
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using com.bemaservices.PastoralCare.Model;
using com.bemaservices.PastoralCare.Web.Cache;
using com.bemaservices.PastoralCare.Web.UI.Controls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Controls;
using Rock.Field;
using Rock.Field.Types;
using Rock.Reporting;
using Rock.SystemGuid;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
namespace com.bemaservices.PastoralCare.Field.Types
{
    /// <summary>
    /// 
    /// </summary>

    [FieldTypeGuid( "E9EA88C6-BAFB-4A2E-9471-CC2FD7951744" )]
    [BooleanField( "Include Inactive",
        Description = "When set, inactive care types will be included in the list.",
        DefaultBooleanValue = false,
        Key = INCLUDE_INACTIVE_KEY,
        Order = 0
        )]
    [IntegerField( "Columns",
            Description = "Select how many columns the list should use before going to the next row. If blank or 0 then 4 columns will be displayed. There is no enforced upper limit however the block this control is used in might add contraints due to available space.",
            IsRequired = false,
            Key = REPEAT_COLUMNS,
            Order = 1
            )]
    public class CareTypesFieldType : UniversalItemPickerFieldType, ICachedEntitiesFieldType
    {
        protected override bool IsMultipleSelection => true;

        #region Attribute Keys

        private const string INCLUDE_INACTIVE_KEY = "includeInactive";
        private const string REPEAT_COLUMNS = "repeatColumns";

        #endregion

        protected override UniversalItemValuePickerDisplayStyle GetDisplayStyle( Dictionary<string, string> privateConfigurationValues )
        {
            return UniversalItemValuePickerDisplayStyle.List;
        }

        protected override int? GetColumnCount( Dictionary<string, string> privateConfigurationValues )
        {
            var columnCount = privateConfigurationValues.GetValueOrDefault( REPEAT_COLUMNS, string.Empty ).AsIntegerOrNull();
            return columnCount;
        }

        /// <summary>
        /// Gets the list of items to be displayed in the picker.
        /// </summary>
        /// <param name="privateConfigurationValues">The configuration values that describe the field type.</param>
        /// <returns>A list of item bags that will be rendered in the picker.</returns>
        protected override List<ListItemBag> GetListItems( Dictionary<string, string> privateConfigurationValues )
        {
            var configuredValues = GetListSource( privateConfigurationValues );

            return configuredValues.Select( item => new ListItemBag
            {
                Value = item.Key,
                Text = item.Value
            } )
            .ToList();
        }

        /// <summary>
        /// Gets the item bags for the values. If an item is not found
        /// (for example, no longer exists), then it should not be included
        /// in the returned list.
        /// </summary>
        /// <param name="values">The individual values that should be retrieved.</param>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A list of <see cref="T:Rock.ViewModels.Utility.ListItemBag" /> objects that have the <see cref="P:Rock.ViewModels.Utility.ListItemBag.Value" /> and <see cref="P:Rock.ViewModels.Utility.ListItemBag.Text" /> properties filled in.</returns>
        protected override List<ListItemBag> GetItemBags( IEnumerable<string> values, Dictionary<string, string> privateConfigurationValues )
        {
            var configuredValues = GetListSource( privateConfigurationValues );
            return configuredValues
                .Where( item => values.Contains( item.Key ) )
                .Select( item => new ListItemBag
                {
                    Value = item.Key,
                    Text = item.Value
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the list source.
        /// </summary>
        /// <value>
        /// The list source.
        /// </value>
        public Dictionary<string, string> GetListSource( Dictionary<string, string> configurationValues )
        {
            var allCareTypes = CareTypeCache.All();

            bool includeInactive = configurationValues.GetValueOrDefault( INCLUDE_INACTIVE_KEY, string.Empty ).AsBoolean();

            var careTypeList = allCareTypes
                .Where( c => !c.IsActive.HasValue || c.IsActive.Value || includeInactive )
                .ToList();

            return careTypeList.ToDictionary( c => c.Guid.ToString(), c => c.Name );
        }

        /// <summary>
        /// Gets the cached entities as a list.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public List<IEntityCache> GetCachedEntities( string value )
        {
            var guids = value.SplitDelimitedValues().AsGuidList();
            var result = new List<IEntityCache>();

            result.AddRange( guids.Select( g => CampusCache.Get( g ) ) );

            return result;
        }

    }
}