
using System.Collections.Generic;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;


namespace ElementReader
{
    [Transaction(TransactionMode.Manual)]
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
            if (enumCategory != BuiltInCategory.OST_Grids) /**/
            {
                TaskDialog.Show("Addin", "The object you have selected is is not AXIS.\nIt is: "+ category.Name);
            }
            TaskDialog.Show("Addin", getElementParameters(current_el, currentDoc));
            return Result.Succeeded;
        }
        string getElementParameters(Element el, Document currentDoc)
        {
            string messageBack = "The parameters of this element are:";
            ParameterSet parameterSet = el.Parameters;
            foreach (Parameter parameter in parameterSet)
            {
                string former_para = "\n" + parameter.Definition.Name + " : ";
                switch (parameter.StorageType)
                {
                    case StorageType.Integer:
                        if (parameter.Definition.ParameterType == ParameterType.YesNo)
                        {
                            if (parameter.AsInteger() == 0)
                            {
                                former_para += "False";
                            }
                            else
                            {
                                former_para += "True";
                            }
                            break;
                        }
                        former_para += parameter.AsInteger();
                        break;
                    case StorageType.String:
                        former_para += parameter.AsString();
                        break;
                    case StorageType.Double:
                        former_para += parameter.AsValueString();
                        break;
                    case StorageType.ElementId:
                        ElementId temp_id = parameter.AsElementId();
                        if (temp_id.IntegerValue >= 0)
                        {
                            former_para += currentDoc.GetElement(temp_id).Name;
                            break;
                        }
                        former_para += temp_id.IntegerValue;
                        break;
                    default:
                        former_para += "UNKNOWN";
                        break;
                }
                messageBack += former_para + ";";
            }
            return messageBack;
        }

    }
}
