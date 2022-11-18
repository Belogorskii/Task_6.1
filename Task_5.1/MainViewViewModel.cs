using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace Task_5._1
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;
        public List<DuctType> DuctTypes { get; } = new List<DuctType>();
        public List <Level> Levels < get;> = new List<Level>();

        public DelegateCommand SaveCommand { get; }
        public double DuctHeight { get;}
        public List<XYZ> Points { get; } = new List<XYZ>();
        public DuctType SelectedDuctType { get; set; }
        public Level SelectedLevel { get; set; }
        public MainViewViewModel(ExternalCommandData commandData)

         {
            _commandData = commandData;
            DuctTypes = DuctsUtils.GetDuctTypes(commandData);
            Levels = LevelsUtils.GetLevels(commandData);
            SaveCommand = new DelegateCommand(OnSaveCommand);
            DuctHeight = 0;
            Points = SelectionUtills.GetPoints(_commandData, "Выберете точки", ObjectSnapTypes.Endpoints);
         }
        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (Points.Count < 2 || 
                SelectedDuctType == null || 
                SelectedLevel == null)
                return;
            var curves = new List<Curve>();
            for (int i=0; i<Points.Count; i++)
            {
                if (i == 0) continue;

                var prevPoint = Points[i - 1];
                var currentPoint = Points[i];
                curves curve = Line.CreateBound(prevPoint, currentPoint);
                curves.Add(curve);
            }
            using (var ts = new Transaction(doc, "Create duct"))
            {
                ts.Start();
                foreach (var curve in curves)
                {
                    Duct.Create(doc, curve, SelectedDuctType.Id, SelectedLevel.Id,
                        UnitUtils.ConvertToInternalUnits(DuctHeight,UnitTypeId.Millimeters))
                        0, false, false);
                }
                ts.Commit();
            }
            RaiseCloseRequest();
        }
        public event EventHandler CloseRequested;
        private void RaiseCloseRequest()
        { 
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
