namespace SisCoreBackEnd.DTOs.MasterUsers
{
    public class AssignCompanyToMasterUserRequest
    {
        public int MasterUserId { get; set; }
        public int CompanyId { get; set; }
        public string Role { get; set; } = "viewer"; // god, owner, admin, viewer
    }
}

