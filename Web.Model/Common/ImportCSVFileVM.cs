namespace Web.Model.Common
{
    public class ImportCSVFileVM
    {
        public int OrganizationId { get; set; }
        public int ServiceLineId { get; set; }
        public string RoleIds { get; set; }
        public int LoggedinUserId { get; set; }
        public string Base64CSV { get; set; }
        public string FilePath { get; set; }

    }
}
