using Autodesk.Revit.DB;


namespace rvtRebars
{
    class Helpers
    {
            public static FamilySymbol GetFamilySymbolByName(Document doc, string familyName, string symbolName)
    {
        FilteredElementCollector collector = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilySymbol));

        foreach (FamilySymbol symbol in collector)
        {
            if (symbol.FamilyName == familyName && symbol.Name == symbolName)
            {
                return symbol;
            }
        }

        return null;
    }
		
    }
}