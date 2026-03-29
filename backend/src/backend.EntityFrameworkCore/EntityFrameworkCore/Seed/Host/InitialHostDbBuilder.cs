namespace backend.EntityFrameworkCore.Seed.Host;

public class InitialHostDbBuilder
{
    private readonly backendDbContext _context;

    public InitialHostDbBuilder(backendDbContext context)
    {
        _context = context;
    }

    public void Create()
    {
        new DefaultEditionCreator(_context).Create();
        new DefaultLanguagesCreator(_context).Create();
        new HostRoleAndUserCreator(_context).Create();
        new DefaultSettingsCreator(_context).Create();
        new DefaultCategoriesCreator(_context).Create();
        new LegalDocumentRegistrar(_context).Create();

        _context.SaveChanges();
    }
}


