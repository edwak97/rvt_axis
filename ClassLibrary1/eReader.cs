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
    [Transaction(TransactionMode.ReadOnly)]
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
            Element familyInst_el = null;
            foreach(ElementId elId in collection)
            {
                familyInst_el = currentDoc.GetElement(elId);
            }
            if (familyInst_el == null) { TaskDialog.Show("Addin", "It is null."); return Result.Failed; }
            TaskDialog.Show("Addin", String.Format("The following object name is belongs to the family {0}", familyInst_el.Category.Name));
            return Result.Succeeded;
        }
    }
}
