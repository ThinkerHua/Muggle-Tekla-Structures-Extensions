namespace Muggle.TsExtensions.CodingHelper.Generators {
    
    /// <summary>
    /// Register fields from data class to plugin class.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class FieldsFromDataAttribute : System.Attribute {
        
        public FieldsFromDataAttribute() {
            
        }
    }
}