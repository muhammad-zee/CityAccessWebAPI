namespace Web.Model.Common
{
    public class ComponentAccessVM
    {
        public long id { get; set; }
        public string text { get; set; }
        public string parent { get; set; }
        public ComponentAccessStateVM state { get; set; } = new ComponentAccessStateVM();
    }

    public class ComponentAccessStateVM
    {
        public bool opened { get; set; }
    }


}
