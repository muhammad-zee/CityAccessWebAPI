using System.ComponentModel;

namespace Web.Services.Enums
{
    public enum UCLEnums
    {
        States = 1,
        Genders = 8,
        OrgType = 10,
        Stroke = 1101,
        Stemi = 1102,
        Sepsis = 1105,
        Trauma = 1106,
        Blue = 1116,
        
        [Description("EMS Code")]
        EMS,
        [Description("Code")]
        InhouseCode
    }

    public enum RouteEnums 
    { 

        /// <summary>
        /// Inhouse Codes Routes
        /// </summary>
        [Description("/Home/Codes")]
        InhouseCodeGrid,

        [Description("/Home/Codes/Code%20Stroke")]
        CodeStrokeForm,

        [Description("/Home/Codes/Code%20Sepsis")]
        CodeSepsisForm,

        [Description("/Home/Codes/Code%20Stemi")]
        CodeSTEMIForm,

        [Description("/Home/Codes/Code%20Trauma")]
        CodeTraumaForm,

        [Description("/Home/Codes/Code%20Blue")]
        CodeBlueForm,

        /// <summary>
        /// EMS Code Routes
        /// </summary>
        [Description("/Home/EMS%20Codes")]
        ActiveEMS,

        [Description("/Home/EMS%20Codes/activateCode")]
        EMSForms,


        /// <summary>
        /// Dashboard Route
        /// </summary>
        [Description("/Home/Dashboard")]
        Dashboard,
        
    }
}
