using ATS.Application.Abstract.AI;

namespace ATS.Tests.TestSupport
{
    public sealed class FakeChatModelIdentityProvider(string identity) : IChatModelIdentityProvider
    {
        public int CallCount { get; private set; }

        public string GetChatModelIdentity()
        {
            CallCount++;
            return identity;
        }
    }
}
