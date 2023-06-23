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
using System.Collections.Generic;
using System.Web.UI.WebControls;

using com.bemaservices.PastoralCare.Model;

using Rock;
using Rock.Web.UI.Controls;

namespace com.bemaservices.PastoralCare.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class CareItemPicker : RockDropDownList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CareItem" /> class.
        /// </summary>
        public CareItemPicker()
            : base()
        {
            Label = "Care Item";
        }

        /// <summary>
        /// Gets or sets the care items.
        /// </summary>
        /// <value>
        /// The care items.
        /// </value>
        public List<CareItem> CareItems
        {
            set
            {
                this.Items.Clear();
                this.Items.Add( new ListItem() );

                foreach ( CareItem careItem in value )
                {
                    this.Items.Add( new ListItem(careItem.Name, careItem.Id.ToString() ) );
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected care item identifier.
        /// </summary>
        /// <value>
        /// The selected care item identifier.
        /// </value>
        public int? SelectedCareItemId
        {
            get
            {
                return this.SelectedValueAsInt();
            }

            set
            {
                int id = value.HasValue ? value.Value : 0;
                var li = this.Items.FindByValue( id.ToString() );
                if ( li != null )
                {
                    li.Selected = true;
                }
            }
        }
    }
}