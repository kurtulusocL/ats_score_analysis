using ATS.Application.Abstract.AI;

namespace ATS.Infrastructure.Concrete.AI
{
    public class SelectedProviderChatModelIdentityProvider : IChatModelIdentityProvider
    {
        private readonly AiProviderClientResolver _aiProviderClientResolver;

        public SelectedProviderChatModelIdentityProvider(AiProviderClientResolver aiProviderClientResolver)
        {
            _aiProviderClientResolver = aiProviderClientResolver;
        }

        public string GetChatModelIdentity() => _aiProviderClientResolver.Resolve().ChatModelIdentity;
    }
}
