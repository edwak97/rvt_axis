
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
        private static List<Grid> elList;
        private static XYZ gridDirection;
        private static Document currentDoc;
        public Result Execute(ExternalCommandData externalCommandData, ref string str, ElementSet element)
        {
            UIDocument UIcurrentDoc = externalCommandData.Application.ActiveUIDocument;
            currentDoc = UIcurrentDoc.Document;
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
            //RenameGridsFinish();
            return Result.Succeeded;
        }
        private static bool areGridsParallel()
        {
            gridDirection = (elList[0].Curve as Line).Direction.Normalize();
            foreach (Grid item in elList)
            {
                XYZ dir2 = (item.Curve as Line).Direction;
                if (!dir2.IsAlmostEqualTo(gridDirection))
                {
                    return false;
                }
            }
            return true;
        }
        private bool DoesContain(Grid el)
        {
            foreach (Grid grid in elList)
            {
                if (el.Id == grid.Id)
                {
                    return true;
                }
            }
            return false;
        }
        private static XYZ NormalVector
        {
            get
            {
                /*  x1*x_n + y1*y_n = 0;
                 where x1 = gridDirection.X; y1 = gridDirection.Y;
                 x_n = 1; y_n =  x1/y1 EXCEPT y1=0*/
                return (gridDirection.IsAlmostEqualTo(new XYZ(1, 0, 0)) || gridDirection.IsAlmostEqualTo(new XYZ(-1, 0, 0))) ? new XYZ(0, 1, 0) : new XYZ(1, -gridDirection.X / gridDirection.Y, 0);
            }
        }
        private void RenameGridsFinish()
        {
            string[] gridNames = new string[elList.Count];
            for (int i = 0; i < elList.Count; i++) //Reading Grid names and writing them to the array
            {
                gridNames[i] = elList[i].Name;
            }
            for (int i = 0; i < gridNames.Length; i++) // it won't be int. But enough for testing
            {
                int tempval = System.Int32.Parse(gridNames[i]);
                for (int k = i - 1; k > -1; i++)
                {
                    //body
                }
            }
            using (Transaction trnsct = new Transaction(currentDoc))
            {
                trnsct.Start("Grid renaming");
                for (int i = 0; i < elList.Count; i++)
                {
                    elList[0].Name = gridNames[i];
                }
                trnsct.Commit();
            }
        }
    }
}
