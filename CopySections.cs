using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq; 
using Autodesk.Revit.DB.Structure;
using System;

namespace rvtRebars
{
    [Transaction(TransactionMode.Manual)]
    public class CopySections : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

			
			 Element secEle = doc.GetElement(uidoc.Selection.PickObject(ObjectType.Element, "Pick a ViewSection"));
			 
			 //TaskDialog.Show("E", secEle.Name);
    
        	Element collector = new FilteredElementCollector(doc)
        	.OfClass(typeof(ViewSection))
        	.Where(x=>x.Name == secEle.Name).First();
    
    	
			ViewSection viewSec = collector as ViewSection;
			
			 //TaskDialog.Show("E", viewSec.Name);
			 
			Rebar rebarSource = doc.GetElement(uidoc.Selection.PickObject(ObjectType.Element, "Pick a bar in a slice")) as Rebar;
			
			string sourceUid = rebarSource.LookupParameter("LOR_UniqueID (SRC_FBA)").AsValueString();
			string sourceSlice = rebarSource.LookupParameter("FBA_Slice").AsValueString();
			
				var rebars = new FilteredElementCollector(doc)
				    .OfClass(typeof(Rebar))
				    .Cast<Rebar>()
				    .ToList();
					
				var barLayers = rebars
					.Where(x=>x.LookupParameter("LOR_UniqueID (SRC_FBA)").AsValueString() == sourceUid && x.LookupParameter("FBA_Slice").AsValueString() == sourceSlice)
					
					       .GroupBy(x=>x.LookupParameter("FBA_SliceLayer").AsValueString())
					.ToList();
			
			
				
				
			 
			    // Category to filter = Rebars
				IList<ElementId> categories = new List<ElementId> { new ElementId(BuiltInCategory.OST_Rebar) };
	



			    using (Transaction tx = new Transaction(doc, "Slice Sections"))
					    {
					        tx.Start();
					        
					        //loop through each layer
					        foreach (var barLayer in barLayers) {
					        	
					        	
					        	
					        	//exclude layers that do not begin with L
					        	if (barLayer.Key.StartsWith("L")){
					        	
						        	//TaskDialog.Show("R", barLayer.Key + "-" + barLayer.Count().ToString());
					        		
						        	string filterLORuid = sourceUid;
					        		
					        		List<FilterRule> filterRules = new List<FilterRule>();
					        
							        //check if filter exist
									ParameterFilterElement parameterFilterElement = new FilteredElementCollector(doc)
										    .OfClass(typeof(ParameterFilterElement))
										    .Cast<ParameterFilterElement>()
										    .FirstOrDefault(f => f.Name == "not " + barLayer.Key);
										
										if (parameterFilterElement == null)
										{
											parameterFilterElement = ParameterFilterElement.Create(doc, "not " + barLayer.Key, categories);
										}
					        
										
										
									//set LOR Unique Id
									//Guid uniqueIdGuid = new Guid("bec5ea93-555b-406d-8034-3df471cf3274");
									Guid sliceLayerGuid = new Guid("1f865887-69b6-4adc-9140-885a5611a68c");
							      FilteredElementCollector rebarsCollector = new FilteredElementCollector(doc).OfClass(typeof(Rebar));
							      
							      Rebar rebar = rebarsCollector.FirstElement() as Rebar;
							
							      if (rebar != null)
							      {
							          Parameter sharedParam = rebar.get_Parameter(sliceLayerGuid);
							          ElementId sharedParamId = sharedParam.Id;
							
							          filterRules.Add(ParameterFilterRuleFactory.CreateNotEqualsRule(sharedParamId, barLayer.Key));
							      }
          
							      	ElementFilter elemFilter = CreateElementFilterFromFilterRules(filterRules);
          							parameterFilterElement.SetElementFilter(elemFilter);
							      
							 	 ElementId newViewId = viewSec.Duplicate(ViewDuplicateOption.Duplicate);
					            View newView = doc.GetElement(newViewId) as View;
					
					            if (newView != null)
					            {
					            	newView.Name = sourceUid + "-" + sourceSlice +"-"+barLayer.Key;
					            }
					            
					                      // Apply filter to view
						          newView.AddFilter(parameterFilterElement.Id);
						          newView.SetFilterVisibility(parameterFilterElement.Id, false);
								  newView.DetailLevel = ViewDetailLevel.Fine;
			       							
								  
					        	}
					        	
					        	
					        	
					        }
					        
					        	

					        tx.Commit();
					    }
			    
			    TaskDialog.Show("R", "Done");

            return Result.Succeeded;
        }


        		          private static ElementFilter CreateElementFilterFromFilterRules(IList<FilterRule> filterRules)
{
   // We use a LogicalAndFilter containing one ElementParameterFilter
   // for each FilterRule. We could alternatively create a single
   // ElementParameterFilter containing the entire list of FilterRules.
   IList<ElementFilter> elemFilters = new List<ElementFilter>();
   foreach (FilterRule filterRule in filterRules)
   {
      ElementParameterFilter elemParamFilter = new ElementParameterFilter(filterRule);
      elemFilters.Add(elemParamFilter);
   }
   LogicalAndFilter elemFilter = new LogicalAndFilter(elemFilters);

   return elemFilter;
} 
    }
}