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
using Rock.Data;
using Rock.Field;
using Rock.Field.Types;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace com.bemaservices.PastoralCare.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    public class CareTypesFieldType : FieldType, ICachedEntitiesFieldType
    {
        #region Configuration

        private const string INCLUDE_INACTIVE_KEY = "includeInactive";
        private const string REPEAT_COLUMNS = "repeatColumns";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = base.ConfigurationKeys();
            configKeys.Add(REPEAT_COLUMNS);
            configKeys.Add(INCLUDE_INACTIVE_KEY);
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = base.ConfigurationControls();

            // Add checkbox for deciding if the list should include inactive items
            var cb = new RockCheckBox();
            controls.Add(cb);
            cb.AutoPostBack = true;
            cb.CheckedChanged += OnQualifierUpdated;
            cb.Label = "Include Inactive";
            cb.Text = "Yes";
            cb.Help = "When set, inactive care types will be included in the list.";

            var tbRepeatColumns = new NumberBox();
            tbRepeatColumns.Label = "Columns";
            tbRepeatColumns.Help = "Select how many columns the list should use before going to the next row. If blank or 0 then 4 columns will be displayed. There is no upper limit enforced here however the block this is used in might add contraints due to available space.";
            tbRepeatColumns.MinimumValue = "0";
            tbRepeatColumns.AutoPostBack = true;
            tbRepeatColumns.TextChanged += OnQualifierUpdated;
            controls.Add(tbRepeatColumns);

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();

            string description = "When set, inactive care types will be included in the list.";
            configurationValues.Add(INCLUDE_INACTIVE_KEY, new ConfigurationValue("Include Inactive", description, string.Empty));

            description = "Select how many columns the list should use before going to the next row. If blank 4 is used.";
            configurationValues.Add(REPEAT_COLUMNS, new ConfigurationValue("Repeat Columns", description, string.Empty));

            if (controls != null)
            {
                if (controls.Count > 0 && controls[0] != null && controls[0] is CheckBox)
                {
                    configurationValues[INCLUDE_INACTIVE_KEY].Value = ((CheckBox)controls[0]).Checked.ToString();
                }

                if (controls.Count > 1 && controls[1] != null && controls[1] is NumberBox)
                {
                    configurationValues[REPEAT_COLUMNS].Value = ((NumberBox)controls[1]).Text;
                }
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="configurationValues">The configuration values.</param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if (controls != null && configurationValues != null)
            {
                if (controls.Count > 0 && controls[0] != null && controls[0] is CheckBox && configurationValues.ContainsKey(INCLUDE_INACTIVE_KEY))
                {
                    ((CheckBox)controls[0]).Checked = configurationValues[INCLUDE_INACTIVE_KEY].Value.AsBoolean();
                }

                if (controls.Count > 1 && controls[1] != null && controls[1] is NumberBox && configurationValues.ContainsKey(REPEAT_COLUMNS))
                {
                    ((NumberBox)controls[1]).Text = configurationValues[REPEAT_COLUMNS].Value;
                }
            }
        }

        #endregion Configuration

        #region Formatting

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            if (value == null)
            {
                return string.Empty;
            }
            var valueGuidList = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).AsGuidList();
            return this.GetListSource(configurationValues).Where(a => valueGuidList.Contains(a.Key.AsGuid())).Select(s => s.Value).ToList().AsDelimited(", ");
        }

        #endregion

        #region Edit Control 

        /// <summary>
        /// Gets the list source.
        /// </summary>
        /// <value>
        /// The list source.
        /// </value>
        public Dictionary<string, string> GetListSource( Dictionary<string, ConfigurationValue> configurationValues )
        {
            var allCareTypes = CareTypeCache.All();

            bool includeInactive = (configurationValues != null && configurationValues.ContainsKey(INCLUDE_INACTIVE_KEY) && configurationValues[INCLUDE_INACTIVE_KEY].Value.AsBoolean());

            var careTypeList = allCareTypes
                .Where(c => !c.IsActive.HasValue || c.IsActive.Value || includeInactive)
                .ToList();

            return careTypeList.ToDictionary(c => c.Guid.ToString(), c => c.Name);
        }

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            RockCheckBoxList editControl = new RockCheckBoxList { ID = id };
            editControl.RepeatDirection = RepeatDirection.Horizontal;

            // Fixed bug preventing what was is stated in the 'Columns' help text: "If blank or 0 then 4 columns..."
            if (configurationValues.ContainsKey(REPEAT_COLUMNS) && configurationValues[REPEAT_COLUMNS].Value.AsInteger() != 0)
            {
                editControl.RepeatColumns = configurationValues[REPEAT_COLUMNS].Value.AsInteger();
            }

            var listSource = GetListSource(configurationValues);

            if (listSource.Any())
            {
                foreach (var item in listSource)
                {
                    ListItem listItem = new ListItem(item.Value, item.Key);
                    editControl.Items.Add(listItem);
                }

                return editControl;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            List<string> values = new List<string>();

            if (control != null && control is RockCheckBoxList)
            {
                RockCheckBoxList cbl = (RockCheckBoxList)control;
                foreach (ListItem li in cbl.Items)
                    if (li.Selected)
                        values.Add(li.Value);
                return values.AsDelimited<string>(",");
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if (value != null)
            {
                List<string> values = new List<string>();
                values.AddRange(value.SplitDelimitedValues());

                if (control != null && control is RockCheckBoxList)
                {
                    RockCheckBoxList cbl = (RockCheckBoxList)control;
                    foreach (ListItem li in cbl.Items)
                        li.Selected = values.Contains(li.Value, StringComparer.OrdinalIgnoreCase);
                }
            }
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Gets the type of the filter comparison.
        /// </summary>
        /// <value>
        /// The type of the filter comparison.
        /// </value>
        public override Rock.Model.ComparisonType FilterComparisonType
        {
            get
            {
                return ComparisonHelper.ContainsFilterComparisonTypes;
            }
        }

        /// <summary>
        /// Gets the filter value control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            var ddlList = new RockDropDownList();
            ddlList.ID = string.Format("{0}_ddlList", id);
            ddlList.AddCssClass("js-filter-control");

            if (!required)
            {
                ddlList.Items.Add(new ListItem());
            }

            var listSource = GetListSource(configurationValues);

            if (listSource.Any())
            {
                foreach (var item in listSource)
                {
                    ListItem listItem = new ListItem(item.Value, item.Key);
                    ddlList.Items.Add(listItem);
                }

                return ddlList;
            }

            return null;
        }

        /// <summary>
        /// Gets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if (control != null && control is RockDropDownList)
            {
                return ((RockDropDownList)control).SelectedValue;
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if (control != null && control is RockDropDownList)
            {
                ((RockDropDownList)control).SetValue(value);
            }
        }

        /// <summary>
        /// Formats the filter value value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override string FormatFilterValueValue( Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var values = new List<string>();
            var listSource = GetListSource(configurationValues);

            foreach (string key in value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (listSource.ContainsKey(key))
                {
                    values.Add(listSource[key]);
                }
            }

            return AddQuotes(values.ToList().AsDelimited("' OR '"));
        }

        #endregion

        /// <summary>
        /// Gets the cached entities as a list.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public List<IEntityCache> GetCachedEntities( string value )
        {
            var guids = value.SplitDelimitedValues().AsGuidList();
            var result = new List<IEntityCache>();

            result.AddRange(guids.Select(g => CampusCache.Get(g)));

            return result;
        }

    }
}