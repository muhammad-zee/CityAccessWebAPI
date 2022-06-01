using System.ComponentModel;

namespace Web.Services.Enums
{
    public enum UCLEnums
    {
        States = 1,
        Genders = 8,
        OrgType = 10,
        Stroke = 1101,
        STEMI = 1102,
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

        [Description("/Home/Codes/code-strok-form")]
        CodeStrokeForm,

        [Description("/Home/Codes/code-sepsis-form")]
        CodeSepsisForm,

        [Description("/Home/Codes/code-STEMI-form")]
        CodeSTEMIForm,

        [Description("/Home/Codes/code-trauma-form")]
        CodeTraumaForm,

        [Description("/Home/Codes/code-blue-form")]
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
