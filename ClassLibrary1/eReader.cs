
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
        private static GridsTemplate[] gridTemplates;
        private static XYZ gridDirection;
        private static Document currentDoc;
        public Result Execute(ExternalCommandData externalCommandData, ref string str, ElementSet element)
        {
            UIDocument UIcurrentDoc = externalCommandData.Application.ActiveUIDocument;
            currentDoc = UIcurrentDoc.Document;
            Selection selection = UIcurrentDoc.Selection;
            {
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
            }
            // checking if the grids are parallel
            if (!areGridsParallel())
            {
                TaskDialog.Show("Addin", "Sorry, they are not parallel to each other.");
                return Result.Failed;
            }
            RenameGridsFinish();
            string s = "";
            for (int i = 0; i < gridTemplates.Length; i++)
            {
                s += string.Format("\n{0}: {1}, {2}", i, gridTemplates[i].Name, gridTemplates[i].Projection);
            }
            TaskDialog.Show("Addin", s);
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
            gridTemplates = new GridsTemplate[elList.Count];
            for (int i = 0; i < gridTemplates.Length; i++)
            {
                XYZ NodeCoord = (elList[i].Curve as Line).GetEndPoint(0);
                double projection = (NormalVector.X * NodeCoord.X + NormalVector.Y * NodeCoord.Y);
                gridTemplates[i] = new GridsTemplate(elList[i], projection, elList[i].Name);
            } /*writing data to the structs*/
            for (int i = 1; i < gridTemplates.Length; i++)
            {
                GridsTemplate tempo = gridTemplates[i];
                int k = i - 1;
                while ((k > -1) && (gridTemplates[k].Projection > tempo.Projection))
                {
                    gridTemplates[k + 1] = gridTemplates[k--];
                }
                gridTemplates[k + 1] = tempo;
            }/*sorting gridTemplates by projections so that we know the actual order but names are still wrong*/
            bool alreadyDone = true;
            for (int i = 1; i < gridTemplates.Length; i++)
            {
                string tempo = gridTemplates[i].Name;
                int k = i - 1;
                while ((k > -1) && GridsTemplate.MoreThan(gridTemplates[k].Name, tempo))
                {
                    alreadyDone = false;
                    gridTemplates[k + 1].Name = gridTemplates[k--].Name;
                }
                gridTemplates[k + 1].Name = tempo;
            }
            if (alreadyDone)//reverse the order if done
            {
                int i = 0;
                while (!(i+1 > gridTemplates.Length / 2))
                {
                    string temp = gridTemplates[i].Name;
                    gridTemplates[i].Name = gridTemplates[gridTemplates.Length - i - 1].Name;
                    gridTemplates[gridTemplates.Length - i++ - 1].Name = temp;
                }
            }
            using (Transaction trnsct = new Transaction(currentDoc))//rewriting
            {
                trnsct.Start("Grid pseudo renaming");
                for (int i = 0; i < elList.Count; i++)
                {
                    gridTemplates[i].gridInstance.Name = "gridTemplates" + i;
                }
                trnsct.Commit();
                trnsct.Start("renaming");
                for (int i = 0; i < elList.Count; i++)
                {
                    gridTemplates[i].gridInstance.Name = gridTemplates[i].Name;
                }
                trnsct.Commit();
            }
            using (Transaction trnsct2 = new Transaction(currentDoc))//rewriting
            {
                trnsct2.Start("renaming");
                for (int i = 0; i < elList.Count; i++)
                {
                    gridTemplates[i].gridInstance.Name = gridTemplates[i].Name;
                }
                trnsct2.Commit();
            }
        }
    }

}
public struct GridsTemplate
{
    public Grid gridInstance;
    public double Projection;
    public string Name;
    public GridsTemplate(Grid instance, double projection, string name)
    {
        gridInstance = instance;
        Projection = projection;
        Name = name;
    }
    public static bool MoreThan(string left, string right)
    {
        int l; int r;
        bool bl = int.TryParse(left, out l);
        bool br = int.TryParse(right, out r);
        if (bl && br)
        {
            return l > r;
        }
        return string.Compare(left, right, false, System.Globalization.CultureInfo.CurrentCulture) > 0;
    }




}
