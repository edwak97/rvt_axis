
using System.Collections.Generic;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using System.Reflection;


namespace ElementReader
{
    public class ElementReader : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("Grid Handler");
            string path = Assembly.GetExecutingAssembly().Location;
            PushButtonData buttonData = new PushButtonData("Grid.Rename", "Grid handler", path, "ElementReader.SubCmd");
            PushButton pushButton = (PushButton)ribbonPanel.AddItem(buttonData);
            pushButton.ToolTip = "Select grids before applying this function";
            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    public class SubCmd : IExternalCommand
    {
        public static List<Grid> elList;
        public Result Execute(ExternalCommandData externalCommandData, ref string str, ElementSet element)
        {
            UIDocument UIcurrentDoc = externalCommandData.Application.ActiveUIDocument;
            Document currentDoc = UIcurrentDoc.Document;
            Selection selection = UIcurrentDoc.Selection;
            ICollection<ElementId> collection = selection.GetElementIds(); //Retrieving element Ids from the selection
            if (collection.Count == 0)
            {
                TaskDialog.Show("AxisAddin", "You haven't selected any grids. Select grids before applying this function.");
                return Result.Failed;
            }
            /*Getting the element by Id and checking if the element belongs to Axis type*/
            elList = new List<Grid>();
            foreach (ElementId elId in collection)
            {
                Element current_el = currentDoc.GetElement(elId);
                if (!(current_el is Grid))
                {
                    TaskDialog.Show("Addin", "Sorry. It is not Grid");
                    return Result.Failed;
                }
                elList.Add(current_el as Grid);
            }
            // checking if the grids are parallel
            if (!areGridsParallel())
            {
                TaskDialog.Show("Addin", "Sorry, they are not parallel to each other.");
                return Result.Failed;
            }
            selection.SetElementIds(new List<ElementId>());
            Reference pickedObj = selection.PickObject(ObjectType.Element, "Please, choose the first grid.");
            ElementId thfrst = pickedObj.LinkedElementId;
            if (!(currentDoc.GetElement(thfrst) is Grid) || !elList.Contains((Grid)currentDoc.GetElement(thfrst)))
            {
                TaskDialog.Show("Addin", "You need to choose the object that belongs to the selected ones");
                return Result.Failed;
            }
            TaskDialog.Show("Addin", "Well done. Keep going");
            return Result.Succeeded;
        }
        private bool areGridsParallel()
        {
            XYZ dirb = (elList[0].Curve as Line).Direction;
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
            foreach (Grid item in elList)
            {
                XYZ dir2 = (item.Curve as Line).Direction;
                bool local_res = dir2.IsAlmostEqualTo(dirb) ? true : false;
                if (!local_res)
                {
                    return false;
                }
            }
            return global_res;
        }
    }
}
