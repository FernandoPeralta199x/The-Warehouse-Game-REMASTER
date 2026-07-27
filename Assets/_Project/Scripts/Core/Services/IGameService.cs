namespace TW08.Core.Services
{
    public interface IGameService
    {
        void Initialize(ServiceRegistry services);
        void Shutdown();
    }
}
