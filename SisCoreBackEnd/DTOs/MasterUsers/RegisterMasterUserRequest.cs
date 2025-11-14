namespace SisCoreBackEnd.DTOs.MasterUsers
{
    public class RegisterMasterUserRequest
    {
        public int TenantUserId { get; set; } // ID del usuario en la BD tenant
        public string TenantSubdomain { get; set; } = default!; // Subdomain de la empresa (ej: "siscore")
        public bool IsGod { get; set; } = false; // Solo usuarios God pueden asignar este rol
    }
}

