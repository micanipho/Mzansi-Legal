using backend.Configuration.Dto;
using System.Threading.Tasks;

namespace backend.Configuration;

public interface IConfigurationAppService
{
    Task ChangeUiTheme(ChangeUiThemeInput input);
}


