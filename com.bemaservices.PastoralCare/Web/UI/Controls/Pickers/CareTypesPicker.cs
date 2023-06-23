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
using System.Web.UI.WebControls;

using com.bemaservices.PastoralCare.Model;
using com.bemaservices.PastoralCare.Web.Cache;

using Rock;
using Rock.Web.UI.Controls;

namespace com.bemaservices.PastoralCare.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class CareTypesPicker : RockCheckBoxList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CareTypesPicker" /> class.
        /// </summary>
        public CareTypesPicker()
            : base()
        {
            Label = "Care Types";
            this.RepeatDirection = RepeatDirection.Horizontal;
        }

        /// <summary>
        /// By default the care types picker is not visible if there is only one care type.
        /// Set this to true if it should be displayed regardless of the number of active care types.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [force visible]; otherwise, <c>false</c>.
        /// </value>
        public bool ForceVisible { get; set; } = false;

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                LoadItems(null);
            }
        }

        /// <summary>
        /// Gets or sets the care type ids.
        /// </summary>
        /// <value>
        /// The care type ids.
        /// </value>
        private List<int> CareTypeIds
        {
            get
            {
                return ViewState["CareTypeIds"] as List<int>;
            }

            set
            {
                ViewState["CareTypeIds"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [include inactive].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include inactive]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeInactive
        {
            get
            {
                return ViewState["IncludeInactive"] as bool? ?? true;
            }

            set
            {
                ViewState["IncludeInactive"] = value;
                LoadItems(null);
            }
        }

        /// <summary>
        /// Gets or sets the care types.
        /// </summary>
        /// <value>
        /// The care types.
        /// </value>
        public List<CareType> CareTypes
        {
            set
            {
                CareTypeIds = value?.Select(ct => ct.Id).ToList();
                LoadItems(null);
            }
        }

        /// <summary>
        /// Gets the available care type ids.
        /// </summary>
        /// <value>
        /// The available care type ids.
        /// </value>
        public List<int> AvailableCareTypeIds
        {
            get
            {
                return this.Items.OfType<ListItem>().Select(a => a.Value).AsIntegerList();
            }
        }

        /// <summary>
        /// Gets the selected care type ids.
        /// </summary>
        /// <value>
        /// The selected care type ids.
        /// </value>
        public List<int> SelectedCareTypeIds
        {
            get
            {
                return this.Items.OfType<ListItem>()
                    .Where(l => l.Selected)
                    .Select(a => a.Value).AsIntegerList();
            }

            set
            {
                CheckItems(value);

                foreach (ListItem careTypeItem in this.Items)
                {
                    careTypeItem.Selected = value.Exists(a => a.Equals(careTypeItem.Value.AsInteger()));
                }
            }
        }

        /// <summary>
        /// Checks the items.
        /// </summary>
        /// <param name="values">The values.</param>
        public void CheckItems( List<int> values )
        {
            if (values.Any())
            {
                foreach (int value in values)
                {
                    if (this.Items.FindByValue(value.ToString()) == null &&
                    CareTypeCache.Get(value) != null)
                    {
                        LoadItems(values);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Loads the items.
        /// </summary>
        /// <param name="selectedValues">The selected values.</param>
        private void LoadItems( List<int> selectedValues )
        {
            // If we don't have a list of IDs then create it.
            var careTypeIds = this.CareTypeIds ?? CareTypeCache.All().Select(a => a.Id).ToList();

            // Get all the care types
            var careTypes = CareTypeCache.All()
                .Where(c =>
                   (careTypeIds.Contains(c.Id) && (!c.IsActive.HasValue || c.IsActive.Value || IncludeInactive)) ||
                   (selectedValues != null && selectedValues.Contains(c.Id)))
                .OrderBy(c => c.Name)
                .ToList();

            var selectedItems = Items.Cast<ListItem>()
                .Where(i => i.Selected)
                .Select(i => i.Value).AsIntegerList();

            // If there is more than one care type then show the picker, otherwise hide it
            if (careTypes.Count == 1)
            {
                this.Visible = ForceVisible;
            }
            else
            {
                this.Visible = true;
            }

            Items.Clear();

            foreach (CareTypeCache careType in careTypes)
            {
                var li = new ListItem(careType.Name, careType.Id.ToString());
                li.Selected = selectedItems.Contains(careType.Id);
                Items.Add(li);
            }
        }
    }
}