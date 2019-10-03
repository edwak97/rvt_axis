
using System.Collections.Generic;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using System.Text;


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
            if (collection.Count == 0)
            {
                TaskDialog.Show("AxisAddin", "You haven't selected any grids");
                return Result.Failed;
            }
            /*Getting the element by Id and checking if the element belongs to Axis type*/
            List<Grid> elList = new List<Grid>();
            foreach (ElementId elId in collection)
            {
                Element current_el = currentDoc.GetElement(elId);
                /*BuiltInCategory enumCategory = (BuiltInCategory)current_el.Category.Id.IntegerValue;
                if (enumCategory != BuiltInCategory.OST_Grids) 
                {
                    TaskDialog.Show("Addin", "Sorry. It is not Grid");
                    return Result.Failed; 
                }
                //Too many lines. There is the better way:
                */
                if (!(current_el is Grid))
                {
                    TaskDialog.Show("Addin", "Sorry. It is not Grid");
                    return Result.Failed;
                }
                elList.Add(current_el as Grid);
            }
            // checking if the grids are parallel
            if (!areGridsParallel(elList))
            {
                TaskDialog.Show("Addin", "Sorry, they are not parallel to each other.");
                return Result.Failed;
            }
            TaskDialog.Show("Addin", "Well done:)");
            return Result.Succeeded;
        }
        bool areGridsParallel(List<Grid> grids_list)
        {
            XYZ dirb = (grids_list[0].Curve as Line).Direction;
            bool global_res = true;
            /*string debug_local = "";
            int i = 1;
            foreach (Grid item in grids_list)
            {
                XYZ dir2 = (item.Curve as Line).Direction;
                bool local_res = ((dir2.X != dirb.X) | (dir2.Y != dirb.Y) | (dir2.Z != dirb.Z)) ? false : true;
                global_res = local_res & global_res;
                debug_local += string.Format("\n {4}: {0}, {1}, {2}; local: {3}", dir2.X, dir2.Y, dir2.Z, local_res,i++);
            }
            debug_local = string.Format("Global: {0}.\n{1}", global_res, debug_local);
            TaskDialog.Show("Addin", debug_local); */
            foreach (Grid item in grids_list)
            {
                XYZ dir2 = (item.Curve as Line).Direction;
                bool local_res = dir2.IsAlmostEqualTo(dirb) ? true : false;
                if(!local_res)
                {
                    return false;
                }
            }
            return global_res;
        }
    }
}
