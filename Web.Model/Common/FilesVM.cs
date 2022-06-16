namespace Web.Model.Common
{
    public class FilesVM
    {
        public int Id { get; set; }
        public int OrgId { get; set; }
        public string Base64Str { get; set; }
        public string FileName { get; set; }
        public string Type { get; set; }
        public string CodeType { get; set; }
        public int CodeNumber { get; set; }
    }
}
