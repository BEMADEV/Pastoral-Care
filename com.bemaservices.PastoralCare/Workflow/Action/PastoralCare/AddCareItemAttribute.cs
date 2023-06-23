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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using com.bemaservices.PastoralCare.Model;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Workflow;

namespace com.bemaservices.PastoralCare.Workflow.Actions.PastoralCare
{
    /// <summary>
    /// Creates a reservation.
    /// </summary>
    [ActionCategory("Pastoral Care")]
    [Description("Adds a Care Item Attribute.")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata("ComponentName", "Care Item Attribute Add")]

    // Reservation Property Fields
    [WorkflowAttribute("Care Item", "The care item attribute to set the value to the care item created.", true, "", "", 6, null,
        new string[] { "com.bemaservices.PastoralCare.Field.Types.CareItemFieldType" })]

    [WorkflowAttribute("Care Types", "The attribute that contains the care types the care item attribute is for. Leave blank if this is a shared attribute.",
        false, "", "", 1, null, new string[] { "com.bemaservices.PastoralCare.Field.Types.CareTypesFieldType" })]

    [WorkflowTextOrAttribute("Attribute Key", "Attribute Key Attribute", "The key of the attribute to set. <span class='tip tip-lava'></span>", true, "", "", 2, "AttributeKey")]

    [WorkflowTextOrAttribute("Attribute Value", "Attribute Value Attribute", "The value to set. <span class='tip tip-lava'></span>", false, "", "", 3, "AttributeValue")]


    public class AddCareItemAttribute : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            var mergeFields = GetMergeFields(action);
            var careItemService = new CareItemService(rockContext);
            var careTypeItemService = new CareTypeItemService(rockContext);

            // Get care item
            CareItem careItem = null;
            Guid careItemGuid = action.GetWorkflowAttributeValue(GetAttributeValue(action, "CareItem").AsGuid()).AsGuid();
            careItem = careItemService.Get(careItemGuid);
            if (careItem == null)
            {
                errorMessages.Add("Invalid Care Item Attribute!");
                return false;
            }

            // Get care types
            List<CareType> careTypeList = new List<CareType>();
            var careTypeService = new CareTypeService(rockContext);
            Guid? careTypeAttributeGuid = GetAttributeValue(action, "CareTypes").AsGuidOrNull();
            if (careTypeAttributeGuid.HasValue)
            {
                careTypeList = careTypeService.GetByGuids(action.GetWorkflowAttributeValue(careTypeAttributeGuid.Value).SplitDelimitedValues().AsGuidList()).ToList();
            }

            // Get the property settings
            string attributeKey = GetAttributeValue(action, "AttributeKey", true).ResolveMergeFields(mergeFields);
            string attributeValue = GetAttributeValue(action, "AttributeValue", true).ResolveMergeFields(mergeFields);

            var careTypeIdList = careTypeList.Select(ct => ct.Id).ToList();
            var careTypeItemList = careItem.CareTypeItems.Where(cti => !careTypeList.Any() || careTypeIdList.Contains(cti.CareTypeId));
            foreach (var careTypeItem in careTypeItemList)
            {
                careTypeItem.LoadAttributes(rockContext);
                careTypeItem.SetAttributeValue(attributeKey, attributeValue);

                try
                {
                    careTypeItem.SaveAttributeValue(attributeKey, rockContext);
                }
                catch (Exception ex)
                {
                    errorMessages.Add(string.Format("Could not save value ('{0}')! {1}", attributeValue, ex.Message));
                    return false;
                }
            }

            action.AddLogEntry(string.Format("Set '{0}' attribute to '{1}'.", attributeKey, attributeValue));

            return true;
        }
    }
}