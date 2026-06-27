using System;

namespace Muggle.TsExtensions.CodingHelper.Generators {
    /// <summary>
    /// Register default values for chamfer fields.
    /// </summary>
    /// <remarks>
    /// You need to manually call the "SetDataToDefaultIfUnset" method.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = true)]
    public class ChamferFieldDefaultValuesAttribute : Attribute {
        public string Id { get; set; }
        public int Type { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Dz1 { get; set; }
        public double Dz2 { get; set; }

        /// <summary>
        /// Register preset default values by the given id.
        /// </summary>
        /// <remarks>The preset default values is: Type = 0, X = 0.0, Y = 0.0, Dz1 = 0.0, Dz2 = 0.0.</remarks>
        /// <param name="id">The id registered by <see cref="ChamferFieldsAttribute"/>.</param>
        public ChamferFieldDefaultValuesAttribute(uint id) {
            this.Id = id.ToString();
            this.Type = 0;
            this.X = 0.0;
            this.Y = 0.0;
            this.Dz1 = 0.0;
            this.Dz2 = 0.0;
        }

        /// <summary>
        /// Register preset default values by the given id.
        /// </summary>
        /// <remarks>The preset default values is: Type = 0, X = 0.0, Y = 0.0, Dz1 = 0.0, Dz2 = 0.0.</remarks>
        /// <param name="id">The id registered by <see cref="ChamferFieldsAttribute"/>.</param>
        public ChamferFieldDefaultValuesAttribute(string id) {
            this.Id = id;
            this.Type = 0;
            this.X = 0.0;
            this.Y = 0.0;
            this.Dz1 = 0.0;
            this.Dz2 = 0.0;
        }

        /// <summary>
        /// Register default values by the given id.
        /// </summary>
        /// <param name="id">The id registered by <see cref="ChamferFieldsAttribute"/>.</param>
        /// <param name="type">The default value for the "Type" property of "Chamfer".</param>
        /// <param name="x">The default value for the "X" property of "Chamfer".</param>
        /// <param name="y">The default value for the "Y" property of "Chamfer"</param>
        /// <param name="dz1">The default value for the "DZ1" property of "Chamfer"</param>
        /// <param name="dz2">The default value for the "DZ2" property of "Chamfer"</param>
        public ChamferFieldDefaultValuesAttribute(
            uint id, int type = 0, double x = 0.0, double y = 0.0, double dz1 = 0.0, double dz2 = 0.0) {
            
            this.Id = id.ToString();
            this.Type = type;
            this.X = x;
            this.Y = y;
            this.Dz1 = dz1;
            this.Dz2 = dz2;
        }
        
        /// <summary>
        /// Register default values by the given id.
        /// </summary>
        /// <param name="id">The id registered by <see cref="ChamferFieldsAttribute"/>.</param>
        /// <param name="type">The default value for the "Type" property of "Chamfer".</param>
        /// <param name="x">The default value for the "X" property of "Chamfer".</param>
        /// <param name="y">The default value for the "Y" property of "Chamfer"</param>
        /// <param name="dz1">The default value for the "DZ1" property of "Chamfer"</param>
        /// <param name="dz2">The default value for the "DZ2" property of "Chamfer"</param>
        public ChamferFieldDefaultValuesAttribute(
            string id, int type = 0, double x = 0.0, double y = 0.0, double dz1 = 0.0, double dz2 = 0.0) {
            
            this.Id = id;
            this.Type = type;
            this.X = x;
            this.Y = y;
            this.Dz1 = dz1;
            this.Dz2 = dz2;
        }
    }
}