
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
            /*Getting the element by Id and checking if the element belongs to Grid type*/
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
            Reference pickedObj = selection.PickObject(ObjectType.Element, "Please, choose the first grid.");
            ElementId picked_el = pickedObj.ElementId;
            if (!(currentDoc.GetElement(picked_el) is Grid)||!DoesContain(currentDoc.GetElement(picked_el) as Grid))
            {
                TaskDialog.Show("Addin", string.Format("You need to choose the object that belongs to the previously selected ones."));
                return Result.Failed;
            }
            /*GetGridsRightOrder()*/
            return Result.Succeeded;
        }
        private bool areGridsParallel()
        {
            XYZ dirb = (elList[0].Curve as Line).Direction;
            foreach (Grid item in elList)
            {
                XYZ dir2 = (item.Curve as Line).Direction;
                if (!dir2.IsAlmostEqualTo(dirb))
                {
                    return false;
                }
            }
            return true;
        }
        private bool DoesContain(Grid el)
        {
            foreach(Grid grid in elList)
            {
                if(el.Id==grid.Id)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
