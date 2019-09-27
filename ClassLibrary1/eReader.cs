using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;
using System.Reflection;
using Autodesk.Revit.Attributes;


namespace ElementReader
{
    public class ElementReader : IExternalCommand
    {
        public Result Execute(ExternalCommandData externalCommandData, ref string str, ElementSet element)
        {
            UIDocument UIcurrentDoc = externalCommandData.Application.ActiveUIDocument;
            Document currentDoc = UIcurrentDoc.Document;
            Selection selection = UIcurrentDoc.Selection;
            ICollection<ElementId> collection = selection.GetElementIds(); //Retrieving element Ids from the selection
            if (collection.Count == 0 | collection.Count > 1)
            {
                if (collection.Count == 0)
                {
                    TaskDialog.Show("AxisAddin", "Sorry, man you haven't selected anything.");
                    return Result.Failed;
                }
                else
                {
                    TaskDialog.Show("AxisAddin", "Please, choose only a one object.");
                    return Result.Failed;
                }
            }
            /*Getting the element by Id and checking if the element belongs to Axis type*/
            Element current_el = null;
            foreach (ElementId elId in collection)
            {
                current_el = currentDoc.GetElement(elId);
            }
            Category category = current_el.Category;

            BuiltInCategory enumCategory = (BuiltInCategory)category.Id.IntegerValue;
            if(enumCategory!=BuiltInCategory.OST_Grids)
            {
                TaskDialog.Show("Addin", "The object you have selected is is not AXIS");
                return Result.Failed;
            }
            
            TaskDialog.Show("Addin", String.Format( "Its category is {0}",enumCategory.ToString()));

            return Result.Succeeded;
        }
    }
}
