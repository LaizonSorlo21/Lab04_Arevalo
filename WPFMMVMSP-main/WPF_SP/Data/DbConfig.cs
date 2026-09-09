namespace WPF_SP.Data;

public static class DbConfig
{
    // Ajustar Server si la instancia local de SQL Server tiene otro nombre.
    public const string ConnectionString =
        @"Server=.\SQLEXPRESS;Database=NeptunoDB;Trusted_Connection=True;TrustServerCertificate=True;";
}
