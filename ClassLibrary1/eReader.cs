
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
            string s = "The coordinates of the selected grids are:";
            XYZ[] nodeCoords0 = new XYZ[elList.Count];
            double[] dArr = new double[elList.Count];
            var nvector = (X: NormalVector.X, Y: NormalVector.Y);
            for (int i = 0; i < elList.Count; i++)
            {
                nodeCoords0[i] = (elList[i].Curve as Line).GetEndPoint(0);
                dArr[i] = (nvector.X * nodeCoords0[i].X + nvector.Y * nodeCoords0[i].Y);
                s += string.Format("\n{2}.0: ({0}, {1})\nlocalX: {3}.", nodeCoords0[i].X, nodeCoords0[i].Y, elList[i].Name, dArr[i]);
            }
            TaskDialog.Show("Addin", s);

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
                if (gridDirection.IsAlmostEqualTo(new XYZ(1, 0, 0)) || gridDirection.IsAlmostEqualTo(new XYZ(-1, 0, 0)))
                {
                    return new XYZ(0, 1, 0);
                }
                return new XYZ(1, -gridDirection.X / gridDirection.Y, 0).Normalize();
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
                int tempval = int.Parse(gridNames[i]);
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
