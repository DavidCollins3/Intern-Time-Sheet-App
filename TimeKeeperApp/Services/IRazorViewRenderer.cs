using System.Threading.Tasks;

public interface IRazorViewRenderer
{
    Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);
}